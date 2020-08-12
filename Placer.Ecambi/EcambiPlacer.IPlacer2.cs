using Betting.Core;
using Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Placer.BetslipContainer;
using Placer.Core;
using Placer.Core.Meta.Properties;
using Placer.Core.Meta.Settings;
using ProxySupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placer.Ecambi
{
    public abstract partial class EcambiPlacer: IPlacer2
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ProxySettings ProxySettings { get; set; }
        public PlacerBaseSettings Settings { get; set; }
        public PlacerBaseProperties Properties { get; set; }
        public TimeSpan KeepAliveFrequency { get; set; }
        public Betslip CurrentBetslip { get; }
        public object RootPlacer => this;
        public event EventHandler<PlacerLogDataEventArgs> LogEvent;

        public abstract void Login();
        public abstract double GetBalance();


        public void Initialize()
        {
            _sessionFile = $"../../Sessions/{Name}_{UserName}.ses";

            InitWeb(_mainDomain, _market, _lang);
        }

        public void Place()
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    var bet = GetBetToPlace();
                    var outcomes = CreateOutcomes(bet);

                    var toCheckCoupon = _helper.CreateCouponToValidate(bet.BetType, outcomes, _isLogged);
                    Logger.LogDebug("Was generated to validate coupon: " + toCheckCoupon.GetDump());

                    if (_isLogged &&
                        IsValidateCoupon(SessionToken, toCheckCoupon, out JToken responseCheckCoupon))
                    {
                        var checkedCoupon = responseCheckCoupon["requestCoupon"];
                        var placeBet
                            = _helper.GenerateCouponToPlaceBet(outcomes, checkedCoupon, bet.BetType, bet.Stake, bet.EW);
                        Logger.LogDebug("Was generated placebet: " + placeBet.GetDump());
                        var placeResult = _web.Place(SessionToken, placeBet);
                        ManagePlaceResult(bet, placeResult);
                    }
                    //For test without authentification
                    else if (IsValidateCouponNoAuth(toCheckCoupon, out responseCheckCoupon))
                    {
                        //For test
                        Logger.LogDebug("Checked coupon: " + responseCheckCoupon.GetDump());
                        var checkedCoupon = responseCheckCoupon["requestCoupon"];
                        var placeBet
                            = _helper.GenerateCouponToPlaceBet(outcomes, checkedCoupon, bet.BetType, bet.Stake, bet.EW);
                        Logger.LogDebug("Was generated placebet: " + placeBet.GetDump());
                        //var jsonResult = "";
                        //var placeResult = JToken.Parse(jsonResult);
                        //ManagePlaceResult(bet, placeResult);
                    }
                    else
                    {
                        Logger.LogDebug($"Validate coupon is fault.\n{responseCheckCoupon.GetDump()}");
                        ManageValidateResult(bet, responseCheckCoupon);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller()} failed");
                    throw ex.PreserveStackTrace();
                }
            }
        }

        public void AddSelectionToBetslip(SelectionRequest selectionRequest)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    var horse = FindBet(selectionRequest);
                    Logger.LogDebug("horse was found:\n" + horse.GetDump());
                    var selection = new Selection(horse.Id, selectionRequest)
                    {
                        Data = horse,
                        Odds = (double)horse.Odds,
                        Stake = 0,
                        EW = false
                    };
                    CurrentBetslip.AddSelection(selection);
                    GenerateMultiple();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller() } failed");
                    throw ex.PreserveStackTrace();
                }
            }
        }

        /* Генерируем возможные ставки в зависимости от количетсва single-ставок в CurrentBetslip*/
        public void SetRequested(BetType betType, double? stake, bool? ew)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    if ((stake.HasValue ^ ew.HasValue) == false)
                        throw new Exception($"Stake or ew must have value, stake: {stake} ew: {ew}");

                    var selection = CurrentBetslip.GetMultipleBetByType(betType);
                    CurrentBetslip.SetRequested(selection, stake, ew);

                    if (stake.HasValue)
                    {
                        CurrentBetslip.UpdateMultipleBet(
                            betType,
                            selection.Odds,
                            stake.Value,
                            selection.EW,
                            selection.Data,
                            selection.MinStake ?? _minStake ?? 0.1,
                            selection.MaxStake);
                    }

                    if (ew.HasValue)
                    {
                        CurrentBetslip.UpdateMultipleBet(
                            betType,
                            selection.Odds,
                            selection.Stake,
                            (bool)ew,
                            selection.Data,
                            selection.MinStake ?? _minStake ?? 0.1,
                            selection.MaxStake);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller()} failed");
                    throw ex.PreserveStackTrace();
                }
            };
        }

        public void SetRequested(SelectionRequest selectionRequest, double? stake, bool? ew)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    if ((stake.HasValue ^ ew.HasValue) == false)
                        throw new Exception($"Stake or ew must have value, stake: {stake} ew: {ew}");

                    var selection = CurrentBetslip.GetSelectionByQuery(selectionRequest);
                    CurrentBetslip.SetRequested(selection, stake, ew);

                    if (stake.HasValue)
                    {
                        CurrentBetslip.UpdateSelection(
                            selection.Key,
                            selection.Odds,
                            stake.Value,
                            selection.EW,
                            selection.Data,
                            selection.MinStake ?? _minStake ?? 0.1,
                            selection.MaxStake);
                    }

                    if (ew.HasValue)
                    {
                        CurrentBetslip.UpdateSelection(
                            selection.Key,
                            selection.Odds,
                            selection.Stake,
                            (bool)ew,
                            selection.Data,
                            selection.MinStake ?? _minStake ?? 0.1,
                            selection.MaxStake);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller()} failed");
                    UpdateBetslip();
                    throw ex.PreserveStackTrace();
                }
            }
        }

        public void AcceptChanges(SelectionRequest selectionRequest)
        {
            var selection = CurrentBetslip.GetSelectionByQuery(selectionRequest);
            CurrentBetslip.AcceptBetOffer(selection);
        }

        public void AcceptChanges(BetType betType)
        {
            var multipleBet = CurrentBetslip.GetMultipleBetByType(betType);
            CurrentBetslip.AcceptBetOffer(multipleBet);
        }

        public void CheckBet()
        {
            CurrentBetslip.Status = BetslipStatus.BetChecked;
        }

        public void ChooseBet(Bet bet)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                CurrentBetslip.SetBetToPlace(bet);
            }
        }

        public void ClearBetslip()
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    CurrentBetslip.ClearBetslip();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller()} failed");
                    throw ex.PreserveStackTrace();
                }
            }
        }

        public void KeepAlive()
        {
            throw new NotImplementedException();
        }

        public void RemoveSelectionFromBetslip(SelectionRequest selectionQuery)
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    var selection = CurrentBetslip.GetSelectionByQuery(selectionQuery);
                    CurrentBetslip.RemoveSelection(selection, true);
                    GenerateMultiple();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller()} failed");
                    throw ex.PreserveStackTrace();
                }
            }
        }

        public void AcceptBetslipOffer()
        {
            throw new NotImplementedException();
        }

        public void BeforeStop()
        {
            SaveSession(_sessionFile, new List<Uri> { _web.MainDomain });
        }

        public void UpdateBetslip()
        {
            throw new NotImplementedException();
        }

    }
}
