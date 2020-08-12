using Betting.Core;
using FluentScraper.Extension;
using Logging;
using Microsoft.Extensions.Logging;
using NetworkLib462.Net;
using Newtonsoft.Json.Linq;
using Placer.BetslipContainer;
using Placer.Core;
using ProxySupport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Placer.Ecambi
{
    public abstract partial class EcambiPlacer
    {
        protected string _mainDomain, _market, _lang;

        protected string _sessionToken = string.Empty;
        protected string _token = string.Empty;
        protected string _sessionFile;
        protected double? _minStake = null;
        protected bool _isLogged = false;
        protected EcambiWeb _web;
        public BetslipHelper _helper; //public for test

        protected EcambiPlacer(string mainDomain, string market, string lang)
        {
            _mainDomain = mainDomain;
            _market = market;
            _lang = lang;

            _helper = new BetslipHelper();
            CurrentBetslip = new Betslip();
            Logger = AppLogger.CreateLogger<EcambiPlacer>();
        }

        protected ILogger Logger { get; }

        protected string SessionToken
        {
            get
            {
                if (string.IsNullOrEmpty(_sessionToken))
                    throw new Exception("SessionToken was not updated");
                return _sessionToken;
            }
            set { _sessionToken = value; }
        }

        protected string Token
        {
            get
            {
                if (string.IsNullOrEmpty(_token))
                    throw new Exception("Tokcen(ticket or sdkToken) was not updated");
                return _token;
            }
            set { _token = value; }
        }

        protected abstract bool IsSuccessInitData();
        protected abstract string GetSessionId(string token);
        protected abstract void CreateWeb(string userAgent, string language, string encoding, IWebProxy proxy, string mainDomain, string market, string lang);

        protected void InitWeb(string mainDomain, string market, string lang)
        {
            const string userAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 78.0.3904.108 Safari / 537.36";
            const string encoding = "gzip, deflate, br";
            const string language = "en-GB,en;q=0.8";

            var proxy = ProxySettings.CreateProxy462();

            CreateWeb(userAgent, language, encoding, proxy, mainDomain, market, lang);
        }

        private JArray CreateOutcomes(Bet bet)
        {
            dynamic selectedOutcome;
            if (bet.BetType.Code == BetTypeCode.Single)
            {
                var data = (HorseInfo)bet.Data;
                selectedOutcome = data.SelectedOutcomeJData;
            }
            else
            {
                var data = bet.Data as IEnumerable<HorseInfo>;
                selectedOutcome = data.Select(item => item.SelectedOutcomeJData);
            }

            return new JArray(selectedOutcome);
        }

        protected JToken PlaceBet(string sessionId, JToken placeBet)
        {
            _web.PlaceOption(sessionId);
            return _web.Place(sessionId, placeBet);
        }

        /* Проверяем coupon, с авторизацией(after login) */
        protected bool IsValidateCoupon(
           string sessionId,
           JToken requestCoupon,
           out JToken responseCheckCoupon)
        {
            _web.ValidateCouponOption(sessionId);
            var response = _web.ValidateCoupon(sessionId, requestCoupon);
            responseCheckCoupon = response["responseCoupon"];
            return (int)responseCheckCoupon["status"] < 300;
        }

        /* Проверяем coupon, без авторизации, для тестирования */
        protected bool IsValidateCouponNoAuth(
            JToken requestCoupon,
            out JToken responseCheckCoupon)
        {
            _web.ValidateCouponOptionNoAuth();
            var response = _web.ValidateCouponNoAuth(requestCoupon);
            responseCheckCoupon = response["responseCoupon"];
            return (int)responseCheckCoupon["status"] < 300;
        }
        /* Проверяем результат валидации coupon-а на ошибки 
        *  "betErrors": [
                {"betIndex": 0,
                    "errors": [{"type": "VET_INVALID_BET"}]
                    }],
                "outcomeErrors": [
                    {"outcomeId": 2699153202,
                     "outcomeIds": [2699153202],
                     "errors": [
                          {"type": "VET_ODDS_CHANGED",
                           "arguments": ["6000"]}
        *    ]}]*/
        protected void ManageValidateResult(Bet bet, JToken responseCheckCoupon)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var outcomeErrors = responseCheckCoupon["outcomeErrors"];
                if (outcomeErrors != null)
                {
                    for (var i = 0; i < outcomeErrors.Count(); i++)
                    {
                        var id = (long)outcomeErrors[i]["outcomeId"];
                        var betError = CurrentBetslip.GetSelectionByKey(id);

                        IdentifyError(betError, outcomeErrors[i]["errors"]);
                    }
                }
                if (bet.BetType.Code != BetTypeCode.Single)
                {
                    bet.AddError(BetError.Unexpected);
                }
                CheckSameEvent(responseCheckCoupon["requestCoupon"]?["outcomeIds"]);
            }
        }

        /* Проверяем response после сделанной ставки(place bet) */
        protected void ManagePlaceResult(Bet bet, JToken placeResult)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var responseCoupon = placeResult["responseCoupon"];
                var errors = responseCoupon["generalErrors"];

                if (errors != null)
                {
                    IdentifyError(bet, errors);
                    return;
                }

                SetMatchedSingleOdds(responseCoupon);

                var historyCoupon = responseCoupon["historyCoupon"];
                var totalStake = (int?)historyCoupon["stake"] ?? 0;
                var countStakes = historyCoupon["systems"]?.First()["bets"]?.Count() ?? 0;
                var unitStake = totalStake / countStakes;

                CurrentBetslip.Status = BetslipStatus.Placed;
                var betToPlace = CurrentBetslip.GetBetToPlace();
                CurrentBetslip.SetMatched(betToPlace, (double)unitStake / 1000, (double)totalStake / 1000, bet.EW);
            }
        }

        private void SetMatchedSingleOdds(JToken responseCoupon)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var historyCoupon = responseCoupon["historyCoupon"];
                foreach (var system in historyCoupon["systems"])
                {
                    var betOutcomes = system["betOutcomes"];
                    for (int i = 0; i < betOutcomes?.Count(); i++)
                    {
                        var id = (long)betOutcomes[i]["id"];
                        var odds = (int)betOutcomes[i]["playedOdds"];
                        var selection = CurrentBetslip.GetSelectionByKey(id);
                        CurrentBetslip.SetMatchedOdds(selection, (double)odds / 1000);
                    }
                }
            }
        }
        private void IdentifyError(Bet bet, JToken errors)
        {

            foreach (var error in errors)
            {
                var arguments = (JArray)error["arguments"];
                switch (error["type"].Val())
                {
                    case "VET_STAKE_TOO_LOW":
                        bet.AddError(BetError.StakeTooLow);
                        bet.MinStake = arguments.Count() > 0 ? (double?)arguments[0] / 1000 : 0;
                        return;
                    case "VET_ODDS_CHANGED":
                        bet.AddError(BetError.PriceChanged);
                        var newOdds = arguments.Count() > 0 ? (int)arguments[0] : 0;
                        UpdateOdds(bet, newOdds);
                        return;
                    case "VET_BET_OFFER_CLOSED":
                        bet.AddError(BetError.Ended);
                        return;
                    default:
                        CurrentBetslip.Status = BetslipStatus.UnknownError;
                        return;
                }
            }
        }
        /* Обновляем коэффициент при ошибке "VET_ODDS_CHANGED" */
        private void UpdateOdds(Bet bet, int newOdds)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var selection = CurrentBetslip.GetSelectionByKey(bet.Key);
                var horseInfo = (HorseInfo)selection.Data;
                horseInfo.Odds = newOdds / 1000;
                horseInfo.SelectedOutcomeJData["approvedOdds"] = newOdds;
                CurrentBetslip.UpdateSelection(selection.Key, (double)newOdds / 1000, selection.Stake, selection.EW, horseInfo);
            }
        }
        /* Проверяем, если лошади добавлены из одной скачки */
        private void CheckSameEvent(JToken outcomesIds)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var sameEvent = string.Empty;
                var seenIds = new List<long>();
                var horsesInfo = new List<HorseInfo>();
                foreach (long outcomeId in outcomesIds.Select(outcomeId => outcomeId.First()))
                {
                    var horse = (HorseInfo)CurrentBetslip.GetSelectionByKey(outcomeId).Data;
                    horsesInfo.Add(horse);
                }

                var eventIds = horsesInfo.Select(horse => horse.Race.Id) ?? new List<long>();
                foreach (long id in eventIds)
                {
                    var sameIds = eventIds.Where(eventId => eventId == id && !seenIds.Contains(id));
                    if (sameIds.Count() > 1)
                    {
                        var names = horsesInfo.Where(horse => horse.Race.Id == id).Select(horse => horse.Name);
                        sameEvent += "The same event: " + string.Join(" & ", names) + "\n";
                        seenIds.Add(id);
                    }
                }
                if (!string.IsNullOrEmpty(sameEvent))
                    throw new Exception(sameEvent);
            }
        }

        protected Bet GetBetToPlace()
        {
            var bet = CurrentBetslip.GetBetToPlace();
            if (bet == null)
            {
                throw new Exception("No bets to place");
            }
            return bet;
        }

        private bool LoadSession(string sesFile)
        {
            var prevSession = BookieSessionUni.Load(sesFile);
            if (prevSession == null) return false;

            Logger.LogInformation("Session loaded for user: ", UserName);
            if (prevSession.User != UserName) return false;

            _web.SetCookies(prevSession.Cookies.Select(x => new Cookie(x.Name, x.Value, x.Path, x.Domain)//"www." + x.Domain)
            {
                Expires = x.Expires,
                HttpOnly = x.HttpOnly,
                Path = x.Path,
                Secure = x.Secure,
                Version = x.Version,
                // Port = x.Port
            }).ToList());

            return true;
        }
        protected bool LoadSessionAndInit(string sesFile)
        {
            if (LoadSession(sesFile) && IsSuccessInitData())
            {
                Logger.LogInformation("Session still valid");
                return true;
            }
            Logger.LogInformation("Session not valid");
            return false;
        }

        protected void SaveSession(string sesFile, IEnumerable<Uri> domains)
        {
            var cookies = new List<Cookie>();
            foreach (var domain in domains)
            {
                var sesCookies = _web.GetCookies(domain);
                cookies.AddRange(sesCookies.Cast<Cookie>());
            }

            var newSession = new BookieSessionUni
            {
                TimeStamp = DateTime.Now,
                User = UserName,
                Cookies = cookies.Select(x => new UniCookie(x.Name, x.Value, x.Domain, x.Expires, x.HttpOnly, x.Path, x.Secure, x.Version, x.Port)).ToList(),
            };

            BookieSessionUni.Save(newSession, sesFile);
            Logger.LogInformation("Session saved");
        }
        /* Генерируем возможные ставки */
        protected List<BetType> FillBetTypes(int numberOfNotRelatedSelections)
        {
            return BetTypeUtils.GetAllowedBetTypes(
                numberOfNotRelatedSelections,
                BetType.Singles,
                BetType.Double,
                BetType.Doubles,
                BetType.Treble,
                BetType.Trebles,
                BetType.Trixie,
                BetType.Fourfold,
                BetType.Fourfolds,
                BetType.Fivefold,
                BetType.Fivefolds,
                BetType.Patent,
                BetType.Yankee,
                BetType.Lucky15,
                BetType.Lucky31);
        }

        private HorseInfo FindBet(SelectionRequest selection)
        {
            HorseInfo horse = null;
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    JToken allRacesJData = _web.GetHorseRacing();
                    var racesAtTime = GetAllRacesAtTime(allRacesJData, selection.RaceStartTime);
                    var firstRace = SortRacesBySimilarity(racesAtTime, selection.EventName).FirstOrDefault();
                    JToken raceHorsesJData = _web.GetBetoffer(firstRace.Id);
                    var horses = ExtractHorses(raceHorsesJData);
                    horse = FindHorseByName(horses, selection.Name);
                    horse.Race = firstRace;

                    return horse;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Horse racing or betoffer are not found!\n{ex.Message}");
                }
            }
        }
   
        private List<HorseInfo> ExtractHorses(JToken raceHorsesJData)
        {
            var horses = new List<HorseInfo>();
            foreach (var betOffer in raceHorsesJData["betOffers"])
            {
                var extractedHorses = betOffer["outcomes"].Select(outcome =>
                {
                    var marketName = betOffer["betOfferType"]["name"].Val();
                    var marketType = MarketTypeByName(marketName);

                    if (marketType != MarketType.Win) return null;

                    var selectedOutcome = CreateSelectedOutcome(betOffer);

                    return ExtractHorsesFromOutcome((MarketType)marketType, outcome, selectedOutcome);

                }).ToList();

                extractedHorses.ForEach(_horses => { if (_horses != null) horses.AddRange(_horses); });
            }
            //horses.ForEach(horse => Logger.LogDebug(horse.GetAllDump())); //Debug

            return horses;
        }

        /* Добавляем возможные ставки */
        private void GenerateMultiple()
        {
            var count = CurrentBetslip.Selections.Count();
            var allowed = FillBetTypes(count);

            Logger.LogInformation($"allow {allowed.Count()} types");

            CurrentBetslip.StartUpdateMultiples();
            allowed.ForEach(type =>
            {
                CurrentBetslip.UpdateMultipleBet(
                    betType: type,
                    key: type,
                    data: CurrentBetslip.Selections
                        .Select(item => (HorseInfo)item.Data)
                        .OrderBy(item => item.Id)
                );
            });
            CurrentBetslip.EndUpdateMultiples();
        }

        /* Заранее создаем поле json для place bet - "SelectedOutcomeJData",
         * чтобы в будущем сохранить поярдок следования полей */
        private JObject CreateSelectedOutcome(JToken betOffer)
        {
            var eachWay = betOffer["eachWay"];
            var selectedOutcome = new JObject
            {
                { "id", 0 },
                { "outcomeId", 0 },
                { "betofferId", betOffer["id"]  },
                { "eventId", betOffer["eventId"] },
                { "approvedOdds", null },
                { "oddsApproved", true },
                { "approvedEachWayFractionMilli", betOffer["eachWay"]?["fractionMilli"] },//?? null },
                { "approvedEachWayPlaceLimit", betOffer["eachWay"]?["placeLimit"] },
                { "eachWayFractionMilli", betOffer["eachWay"]?["fractionMilli"] },
                { "eachWayPlaceLimit", betOffer["eachWay"]?["placeLimit"] },
                { "eachWayApproved", true },
                { "isLiveBetoffer", false },
                { "isPrematchBetoffer", true },
                { "fromBetBuilder", false },
                { "source", "Event List View" }

            };

            if (eachWay == null)
            {
                selectedOutcome.Property("approvedEachWayFractionMilli").Remove();
                selectedOutcome.Property("approvedEachWayPlaceLimit").Remove();
                selectedOutcome.Property("eachWayFractionMilli").Remove();
                selectedOutcome.Property("eachWayPlaceLimit").Remove();
            }

            return selectedOutcome;
        }

        private List<HorseInfo> ExtractHorsesFromOutcome(
            MarketType marketType,
            JToken outcomeJData,
            JObject selectedOutcomeJData)
        {
            var horses = new List<HorseInfo>();

            var horse = ExtractHorseFromOutcome(marketType, outcomeJData, selectedOutcomeJData);
            horses.Add(horse);

            return horses;
        }

        private HorseInfo ExtractHorseFromOutcome(
            MarketType marketType,
            JToken outcomeJData,
            JObject selectedOutcomeJData)
        {
            var horseId = (long)outcomeJData["id"];
            var horseName = outcomeJData["participant"].Val();
            var odds = outcomeJData["odds"]?.Val<decimal>() / 1000 ?? -1m;

            selectedOutcomeJData["id"] = outcomeJData["id"];
            selectedOutcomeJData["outcomeId"] = outcomeJData["id"];

            if (odds > 0)
            {
                selectedOutcomeJData["approvedOdds"] = outcomeJData["odds"];
                selectedOutcomeJData["oddsApproved"] = true;
            }
            else
            {
                selectedOutcomeJData.Property("approvedOdds").Remove();
            }

            return new HorseInfo
            {
                Id = horseId,
                Name = horseName,
                Odds = odds,
                MarketType = marketType,
                //RaceHorseJData = outcomeJData
                SelectedOutcomeJData = selectedOutcomeJData
            };
        }
        private MarketType? MarketTypeByName(string marketName)
        {
            switch (marketName)
            {
                case "Win or Each Way":
                    return MarketType.EW;
                case "Winner":
                    return MarketType.Win;
                case "(1-2)":
                case "(1-3)":
                case "Head to Head":
                    return null;
            }

            throw new Exception($"Can't determinate market by name: {marketName}");
        }

        private HorseInfo FindHorseByName(List<HorseInfo> horses, string requestName)
        {
            foreach (var horse in horses)
            {
                var curHorseNameNorm = NameNormalizer.NormaliseName($"{horse.Name}", true, false);

                if (Levenshtein.GetSimilarityPercentage($"{curHorseNameNorm}", requestName, true) < 0.8f)
                    continue;

                return horse;
            }

            throw new Exception($"Can't find horse with name: {requestName}");
        }

        private static IEnumerable<RaceInfo> SortRacesBySimilarity(
            IEnumerable<RaceInfo> races,
            string raceName)
        {
            double GetSimilarityValue(string name1, string name2)
            {
                name1 = NameNormalizer.NormaliseName(name1, true, false);
                name2 = NameNormalizer.NormaliseName(name2, true, false);
                return Levenshtein.GetSimilarityPercentage(name1, name2, true);
            }

            var result = races.OrderByDescending(item => GetSimilarityValue(item.Name, raceName));
            return result.ToList();
        }

        private List<RaceInfo> GetAllRacesAtTime(JToken allMeetings, DateTime selectionRaceStartTime)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var allRaces = ExtractRaces(allMeetings);
                var racesAtTime = allRaces.FindAll(item =>
                    item.StartTime.ToString("dd:HH:mm") == selectionRaceStartTime.ToString("dd:HH:mm"));

                return racesAtTime;
            }
        }

        private List<RaceInfo> ExtractRaces(JToken meetings)
        {
            var allRaces = new List<RaceInfo>();

            foreach (var meeting in meetings)
            {
                var context = meeting["context"];
                var name = context["course"]["name"].Val();
                var region = context["region"]["name"].Val();
                var races = meeting["events"].Select(race => ParseToRace(race, name, region)).ToList();

                allRaces.AddRange(races);
            }
            return allRaces;
        }

        protected RaceInfo ParseToRace(JToken race, string raceName, string raceRegion)
        {
            var prov = new CultureInfo("en-us");

            var id = (long)race["id"];
            var startTimeStr = race["startTime"].Val();
            var startTime = DateTime.Parse(startTimeStr, prov);
            var originalStartTimeStr = race["originalStartTime"].Val();
            var state = race["state"].Val();

            return new RaceInfo
            {
                Id = id,
                Name = raceName,
                StartTime = startTime,
                OriginalStartTimeStr = originalStartTimeStr,
                Region = raceRegion,
                State = state
            };
        }
    }

}
