
using Betting.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using FluentScraper.Extension;

namespace Placer.Ecambi
{
    public class BetslipHelper
    {
        // Генерируем json для валидации coupon-а(перед отправкой placebet coupon)
        // Пример для Double
        /* {"requestCoupon": {
                "type": "RCT_SYSTEM",  // Для валидации купона всегда RCT_SYSTEM
                "odds": [3000,4500],
                "outcomeIds": [[2699153081],[2699153205]],
                "selection": [[],[]],
                "betsPattern": "111",      // Всевозможные комбинации(3) для Double(1 + 2 + 12) на каждой позиции всегда 1                                      
                "isUserLoggedIn": true}}*/
        public JToken CreateCouponToValidate(BetType betType, JArray outcomes, bool isUserLoggedIn = false)
        {
            dynamic toCheckCoupon = new ExpandoObject();
            toCheckCoupon.type = "RCT_SYSTEM";
            toCheckCoupon.odds
                = outcomes.Select(outcome => Default(ko.unwrap(outcome["approvedOdds"]), -1));
            toCheckCoupon.outcomeIds
                = outcomes.Select(outcome => new JArray(Default(ko.unwrap(outcome["outcomeId"]), 0)));
            toCheckCoupon.selection = outcomes.Select(outcome => new JArray());
            toCheckCoupon.betsPattern = GetBetsPatternForValidate(betType, outcomes.Count());
            toCheckCoupon.isUserLoggedIn = isUserLoggedIn;

            return JObject.FromObject(toCheckCoupon);
        }
        /* Генерируем json чтобы сделать ставку */
        public JToken GenerateCouponToPlaceBet(
            JArray outcomes,
            JToken checkedCoupon,
            BetType betType,
            double stake,
            bool ew
            )
        {
            var placeBet = new
            {
                id = 1,
                trackingData = CreateTrackingData(outcomes),
                requestCoupon = CreateRequestCoupon(outcomes, checkedCoupon, betType, stake, ew)
            };
            return JObject.FromObject(placeBet);
        }
        /* Создаем поле "requestCoupon" для json ставки */
        public JToken CreateRequestCoupon(
           JArray outcomes,
           JToken checkedCoupon,
           BetType betType,
           double stake,
           bool ew)
        {
            var combinations = CreateSystemCombinations(betType, outcomes);
            var outcomeIds = combinations?.Select(combination => combination["outcomeIds"]);

            var requestCoupon = new
            {
                allowOddsChange = checkedCoupon["allowOddsChange"],
                odds = checkedCoupon["odds"],
                stakes = CreateStakes(betType, outcomes.Count(), (int)(stake * 1000)),
                outcomeIds = checkedCoupon["outcomeIds"],
                type = GetType(betType),
                eachWayFraction = GetEachWayFraction(ew, outcomes),
                eachWayPlaceLimit = GetEachWayPlaceLimit(ew, outcomes),
                betsPattern = GenerateBetsPatternForPlace(betType, new JArray(outcomeIds)),
                systemCombinations = combinations,
                eachWay = GetEachWay(betType, ew, outcomes.Count()),
                selection = checkedCoupon["selection"]
            };

            return JObject.FromObject(requestCoupon, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore});
        }

        /*Выбираем тип ставки: 
            - RCT_SINGLE - для Single
            - RCT_COMBINATION - для не -s, но поддерживающих -s(Double, Treble, ...)
            - RCT_SYSTEM - (Trixie, Yankee, Doubles, Trebles, ...)
        */
        private JToken GetType(BetType betType)
        {
            if (betType.Code == BetTypeCode.Single)
                return "RCT_SINGLE";

            if (BetTypeUtils.IsSingularBetType(betType) && 
                BetTypeUtils.IsSupportMultiple(betType))
            {
                return "RCT_COMBINATION";
            }
            return "RCT_SYSTEM";
        }
        /* Генерируем список значений поля "eachWayFraction" json, если ew:
           напр.: "eachWayFraction": [200,200]*/
        private dynamic GetEachWayFraction(bool ew, JArray outcomes)
        {
            if (ew == false) 
                return null;

            var fractions = outcomes.Select(outcome => outcome["approvedEachWayFractionMilli"]);

            if (fractions.Count() == 0)
                return null;

            return fractions;
        }
        /* Генерируем список значений поля "eachWayPlaceLimit" json, если ew:
           напр.: "eachWayPlaceLimit": [3, 5],*/
        private dynamic GetEachWayPlaceLimit(bool ew, JArray outcomes)
        {
            if (ew == false)
                return null;

            var placeLimits = outcomes.Select(outcome => outcome["approvedEachWayPlaceLimit"]);

            if (placeLimits.Count() == 0)
                return null;

            return placeLimits;
        }
        /*Генерируем поле json "eachWay" прим. Doubles из 3 + EW: "eachWay": [ true, true, true ],*/
        private dynamic GetEachWay(BetType betType, bool ew, int from)
        {
            /*"eachWay" отображаем если ew=true и тип без -s и поддерживает -s*/
            if (ew == false || 
                (BetTypeUtils.IsSingularBetType(betType) && BetTypeUtils.IsSupportMultiple(betType))
               )
                return null;

            var eachWays = new JArray();
            var countBet = BetTypeUtils.GetNumberOfInternalBets(betType, from, false);
            for (int i = 1; i <= countBet; i++) eachWays.Add(true);
            
            return eachWays;
        }
        /*Генерируем поле json - "stakes" прим. Doubles из 3 + EW price 0,1: "stakes": [ 100, 100, 100 ],*/
        private JArray CreateStakes(BetType betType, int from, int stake)
        {
            var stakes = new JArray();
            var countBet = BetTypeUtils.GetNumberOfInternalBets(betType, from, false);
            for (int i = 1; i <= countBet; i++) stakes.Add(stake);
            
            return stakes;
        }
        /* Генерируем поле json - "trakingData" */
        public JObject CreateTrackingData(JArray outcomes) =>
            JObject.FromObject(new
            {
                hasTeaser = false,
                isBetBuilderCombination = false,
                selectedOutcomes = outcomes
            });
        /* Генерируем поле json - "systemCombinations" (прим. Doubles из 3):*/
        /*"systemCombinations": [
             * {"outcomeIds": [2699153205],
                "outcomePositions": [3],
                "patternIndex": 0},
               {"outcomeIds": [2699153081],
                "outcomePositions": [2],
                "patternIndex": 1},
               {"outcomeIds": [2699153205,2699153081],
                "outcomePositions": [3,2],
                "patternIndex": 2},
               {"outcomeIds": [2699153045],
                "outcomePositions": [1],
                "patternIndex": 3},
               {"outcomeIds": [2699153205,2699153045],
                "outcomePositions": [3,1],
                "patternIndex": 4},
               {"outcomeIds": [2699153081,2699153045],
                "outcomePositions": [2,1],
                "patternIndex": 5},
               {"outcomeIds": [2699153205,2699153081,2699153045],
                "outcomePositions": [3,2,1],
                "patternIndex": 6}],*/
        /*Поле "systemCombinations" не создается для ставок: без -s но поддерживающих s - это Double, ...)*/
        public JArray CreateSystemCombinations(BetType betType, JArray outcomes)
        {
            if (BetTypeUtils.IsSingularBetType(betType) &&
                BetTypeUtils.IsSupportMultiple(betType))
                return null;

            var patternIndex = 0;
            var systemCombinations = new JArray();
            var allOutcomeIds = CreateCombinations(outcomes);

            allOutcomeIds.ForEach(outcomeIds =>
            {
                var systemCombination 
                    = CreateSystemCombination(outcomes, outcomeIds, patternIndex);

                systemCombinations.Add(systemCombination);
                patternIndex++;
            });

            return systemCombinations;
        }

        private JToken CreateSystemCombination(JArray outcomes, JArray outcomeIds, int patternIndex) =>
            JObject.FromObject(new
            {
                outcomeIds = outcomeIds,
                outcomePositions = GetIndexesOutcomeIds(outcomes, outcomeIds),
                patternIndex = patternIndex
            });

        /*Создаем поле из занимаемых мест id в поле "outcomeIds" - напр. "outcomePositions": [3,2,1],*/
        public JArray GetIndexesOutcomeIds(JArray outcomes, JArray outcomeIds)
        {
            var indexes = new JArray();

            if (outcomes == null)
                throw new ArgumentNullException("Outcomes array is empty");

            for (int i = 0; i < outcomeIds.Count(); i++)
            {
                var foundOutcome = outcomes.Where(outcome =>
                    (long)outcome["id"] == (long)outcomeIds[i]).FirstOrDefault();
                
                if (foundOutcome != null)
                    indexes.Add(outcomes.IndexOf(foundOutcome) + 1);
            }
            return indexes;
        }
        /*Генерируем паттерн для ставки place bet
         * Для приведенного выше примера Doubles из 3 будет "betsPattern": "0010110"
         * Поле "betsPattern" не создается для ставок: без -s но поддерживающих s - это Double, ...)*/
        public string GenerateBetsPatternForPlace(BetType betType, JArray outcomeIds)
        {
            if (BetTypeUtils.IsSingularBetType(betType) && 
                BetTypeUtils.IsSupportMultiple(betType))
                return null;

            var betsPattern = string.Empty;
            foreach (var ids in outcomeIds)
            {
                betsPattern += CountsBetType(betType).Contains(ids.Count()) ? "1" : "0";
            }
            return betsPattern;
        }
        /* Генерируем паттерн для валидации купона из всевозможных комбинаций, всегда 1*/
        // "1111111" для Treble(1 + 2 + 3 + 12 + 13 + 23 + 123) 7 комбинаций
        public string GetBetsPatternForValidate(BetType betType, int selectionCount)
        {
            var pattern = string.Empty;
            for (int of = 1; of <= selectionCount; of++)
            {
                pattern += new string('1', combination(selectionCount, of));
            }
            return pattern;
        }
        /* Создаем комбинации полученные в виде списка строк с позициями id(AssingCombination), 
         * затем заменяем позиции id на сами id(напр. 1 на [2699153045], 321 на [2699153205,2699153081,2699153045])*/
        public List<JArray> CreateCombinations(JArray outcomes)
        {
            var ids = "";
            var allOutcomeIds = new List<JArray>();
            var combinations = new List<string>();
            
            for (int i = 1; i <= outcomes?.Count(); i++) ids += i.ToString();
            AssignCombination("", ids, combinations);
            combinations.Reverse();

            foreach (var combination in combinations)
            {
                // Разбиваем строку "123" на массив чисел [1, 2, 3]
                var cs = combination.ToCharArray().Select(c => (int)char.GetNumericValue(c)).ToList();
                cs.Reverse();
                // Заменяем позиции id на сами id
                var outcomeIds = new JArray();
                foreach(var c in cs)
                {
                    outcomeIds.Add(outcomes[c - 1]["id"]);
                }
                allOutcomeIds.Add(outcomeIds);
            }

            return allOutcomeIds;
        }

        /* Формула количества возможных комбинаций n!/((n-k)!*k!) */
        public int combination(int from, int of) =>
            factorial(from) / (factorial(from - of) * factorial(of)); 

        private int factorial(int number) =>
            number == 0 ? 1 : number * factorial(number - 1);

        /*Функция генерирующая комбинации из позиций id в виде списка строк*/
        /* "123"
           "12"
           "13"
           "1"
           "23"
           "2"
           "3"
           */
        static void AssignCombination(string active, string rest, List<string> result)
        {
            if (rest.Length == 0)
            {
                if (active != string.Empty)
                    result.Add(active);
            }
            else
            {
                AssignCombination(active + rest[0], rest.Substring(1), result);
                AssignCombination(active, rest.Substring(1), result);
            }
        }
     
        private int[] CountsBetType(BetType betType)
        {
            switch (betType.Code)
            {
                case BetTypeCode.Single:
                case BetTypeCode.Singles:
                    return new[] { 1 };
                case BetTypeCode.Double:
                case BetTypeCode.Doubles:
                    return new[] { 2 };
                case BetTypeCode.Treble:
                case BetTypeCode.Trebles:
                    return new[] { 3 };
                case BetTypeCode.Fourfold:
                case BetTypeCode.Fourfolds:
                    return new[] { 4 };
                case BetTypeCode.Fivefold:
                case BetTypeCode.Fivefolds:
                    return new[] { 5 };
                case BetTypeCode.Trixie:
                    return new[] { 2, 3 };
                case BetTypeCode.Patent:
                    return new[] { 1, 2, 3 };
                case BetTypeCode.Yankee:
                    return new[] { 2, 3, 4 }; //6 двойн, 4 тройн, 1 экспресс из 4-ч вариантов.
                case BetTypeCode.Lucky15:
                    return new[] { 1, 2, 3, 4 }; //Lucky15 15 ставок
                case BetTypeCode.Lucky31:
                    return new[] { 1, 2, 3, 4, 5 }; //1 пять прогнозов(по одному ординару на каждый)+...
                case BetTypeCode.SuperYankee:
                    return new[] { 2, 3, 4, 5 }; //canadian
                default:
                    return new[] { 0 };
            }
        }

        private class ko
        {
            public static dynamic unwrap(dynamic a)
            {
                return a;
            }
        }

        private object Default(object value, object onNull)
        {
            if (JSBool(value))
            {
                return value;
            }
            return onNull;
        }

        private bool JSBool(dynamic b)
        {
            if (b == null)
            {
                return false;
            }
            if (b is bool)
            {
                return b;
            }
            if (b is int)
            {
                return b != 0;
            }
            if (b is long)
            {
                return b != 0;
            }
            if (b is string)
            {
                return b != "";
            }
            if (b is JValue)
            {
                var jb = ((JValue)b);
                switch (jb.Type)
                {
                    case JTokenType.Integer:
                        return b.Value != 0;
                    case JTokenType.String:
                        return b.Value != "";
                    case JTokenType.Boolean:
                        return b.Value;
                }
            }
            return true;
        }
    }
}