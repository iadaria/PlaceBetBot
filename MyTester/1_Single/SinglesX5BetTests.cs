﻿using Betting.Core;
using Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.SmartInspect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Placer.Ecambi;
using Placer.Sport888;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTester
{
    [TestClass]
    public class SinglesX5BetTests
    {
        private BetType betType;
        private double stake;
        private bool ew;
        private BetslipHelper helper;
        private JArray outcomes;
        private ILogger logger;

        [TestInitialize]
        public void Init()
        {
            betType = BetType.Singles;
            stake = 0.1;
            ew = false;
            helper = new BetslipHelper();
            outcomes = InitOutcomes();
            logger = InitLogger();

            logger.LogDebug($"Was generated outcome:\n{outcomes.GetDump()}");
        }
        [TestMethod]
        public void Get_Success_ValidateCoupon_Tests()
        {
            using (logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var toValidateCoupon = helper.CreateCouponToValidate(betType, outcomes, true);

                logger.LogDebug("Generated to validate coupon:\n" + toValidateCoupon.GetDump());
                logger.LogDebug("Validate coupon was got from site:\n" + ToValidateCouponBySite["requestCoupon"].GetDump());

                Assert.IsTrue(JToken.DeepEquals(toValidateCoupon, ToValidateCouponBySite["requestCoupon"]));
            }
        }
        [TestMethod]
        public void Get_Success_PlaceBetCoupon_Tests()
        {
            using (logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var placeBet
                    = helper.GenerateCouponToPlaceBet(outcomes, CheckedCouponBySite, betType, stake, ew);

                logger.LogDebug("Validate coupon:\n" + CheckedCouponBySite.GetDump());
                logger.LogDebug("Generated place bet coupon:\n" + placeBet.GetDump());
                logger.LogDebug("Place bet coupon was got from site:\n" + PlaceBetBySite.GetDump());

                Assert.IsTrue(JToken.DeepEquals(placeBet, PlaceBetBySite));
            }
        }

        /**** Data was generated by site ****/
        private JToken ToValidateCouponBySite
        {
            get
            {
                var json = "{\"requestCoupon\":{\"type\":\"RCT_SYSTEM\",\"odds\":[1250,7000,2380,4500,3750],\"outcomeIds\":[[2697163406],[2697163429],[2697432681],[2697432704],[2697432752]],\"selection\":[[],[],[],[],[]],\"betsPattern\":\"1111111111111111111111111111111\",\"isUserLoggedIn\":true}}";
                return JToken.Parse(json);
            }
        }

        private JToken PlaceBetBySite
        {
            get
            {
                var json = "{\"id\":1,\"trackingData\":{\"hasTeaser\":false,\"isBetBuilderCombination\":false,\"selectedOutcomes\":[{\"id\":2697163406,\"outcomeId\":2697163406,\"betofferId\":2197796095,\"eventId\":1006023635,\"approvedOdds\":1250,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":250,\"approvedEachWayPlaceLimit\":2,\"eachWayFractionMilli\":250,\"eachWayPlaceLimit\":2,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"},{\"id\":2697163429,\"outcomeId\":2697163429,\"betofferId\":2197796101,\"eventId\":1006023636,\"approvedOdds\":7000,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":200,\"approvedEachWayPlaceLimit\":5,\"eachWayFractionMilli\":200,\"eachWayPlaceLimit\":5,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"},{\"id\":2697432681,\"outcomeId\":2697432681,\"betofferId\":2197879119,\"eventId\":1006025535,\"approvedOdds\":2380,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":250,\"approvedEachWayPlaceLimit\":2,\"eachWayFractionMilli\":250,\"eachWayPlaceLimit\":2,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"},{\"id\":2697432704,\"outcomeId\":2697432704,\"betofferId\":2197879125,\"eventId\":1006025536,\"approvedOdds\":4500,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":200,\"approvedEachWayPlaceLimit\":3,\"eachWayFractionMilli\":200,\"eachWayPlaceLimit\":3,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"},{\"id\":2697432752,\"outcomeId\":2697432752,\"betofferId\":2197879131,\"eventId\":1006025537,\"approvedOdds\":3750,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":250,\"approvedEachWayPlaceLimit\":3,\"eachWayFractionMilli\":250,\"eachWayPlaceLimit\":3,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}]},\"requestCoupon\":{\"allowOddsChange\":\"AOCT_NO\",\"odds\":[1250,7000,2380,4500,3750],\"stakes\":[100,100,100,100,100],\"outcomeIds\":[[2697163406],[2697163429],[2697432681],[2697432704],[2697432752]],\"type\":\"RCT_SYSTEM\",\"betsPattern\":\"1101000100000001000000000000000\",\"systemCombinations\":[{\"outcomeIds\":[2697432752],\"outcomePositions\":[5],\"patternIndex\":0},{\"outcomeIds\":[2697432704],\"outcomePositions\":[4],\"patternIndex\":1},{\"outcomeIds\":[2697432752,2697432704],\"outcomePositions\":[5,4],\"patternIndex\":2},{\"outcomeIds\":[2697432681],\"outcomePositions\":[3],\"patternIndex\":3},{\"outcomeIds\":[2697432752,2697432681],\"outcomePositions\":[5,3],\"patternIndex\":4},{\"outcomeIds\":[2697432704,2697432681],\"outcomePositions\":[4,3],\"patternIndex\":5},{\"outcomeIds\":[2697432752,2697432704,2697432681],\"outcomePositions\":[5,4,3],\"patternIndex\":6},{\"outcomeIds\":[2697163429],\"outcomePositions\":[2],\"patternIndex\":7},{\"outcomeIds\":[2697432752,2697163429],\"outcomePositions\":[5,2],\"patternIndex\":8},{\"outcomeIds\":[2697432704,2697163429],\"outcomePositions\":[4,2],\"patternIndex\":9},{\"outcomeIds\":[2697432752,2697432704,2697163429],\"outcomePositions\":[5,4,2],\"patternIndex\":10},{\"outcomeIds\":[2697432681,2697163429],\"outcomePositions\":[3,2],\"patternIndex\":11},{\"outcomeIds\":[2697432752,2697432681,2697163429],\"outcomePositions\":[5,3,2],\"patternIndex\":12},{\"outcomeIds\":[2697432704,2697432681,2697163429],\"outcomePositions\":[4,3,2],\"patternIndex\":13},{\"outcomeIds\":[2697432752,2697432704,2697432681,2697163429],\"outcomePositions\":[5,4,3,2],\"patternIndex\":14},{\"outcomeIds\":[2697163406],\"outcomePositions\":[1],\"patternIndex\":15},{\"outcomeIds\":[2697432752,2697163406],\"outcomePositions\":[5,1],\"patternIndex\":16},{\"outcomeIds\":[2697432704,2697163406],\"outcomePositions\":[4,1],\"patternIndex\":17},{\"outcomeIds\":[2697432752,2697432704,2697163406],\"outcomePositions\":[5,4,1],\"patternIndex\":18},{\"outcomeIds\":[2697432681,2697163406],\"outcomePositions\":[3,1],\"patternIndex\":19},{\"outcomeIds\":[2697432752,2697432681,2697163406],\"outcomePositions\":[5,3,1],\"patternIndex\":20},{\"outcomeIds\":[2697432704,2697432681,2697163406],\"outcomePositions\":[4,3,1],\"patternIndex\":21},{\"outcomeIds\":[2697432752,2697432704,2697432681,2697163406],\"outcomePositions\":[5,4,3,1],\"patternIndex\":22},{\"outcomeIds\":[2697163429,2697163406],\"outcomePositions\":[2,1],\"patternIndex\":23},{\"outcomeIds\":[2697432752,2697163429,2697163406],\"outcomePositions\":[5,2,1],\"patternIndex\":24},{\"outcomeIds\":[2697432704,2697163429,2697163406],\"outcomePositions\":[4,2,1],\"patternIndex\":25},{\"outcomeIds\":[2697432752,2697432704,2697163429,2697163406],\"outcomePositions\":[5,4,2,1],\"patternIndex\":26},{\"outcomeIds\":[2697432681,2697163429,2697163406],\"outcomePositions\":[3,2,1],\"patternIndex\":27},{\"outcomeIds\":[2697432752,2697432681,2697163429,2697163406],\"outcomePositions\":[5,3,2,1],\"patternIndex\":28},{\"outcomeIds\":[2697432704,2697432681,2697163429,2697163406],\"outcomePositions\":[4,3,2,1],\"patternIndex\":29},{\"outcomeIds\":[2697432752,2697432704,2697432681,2697163429,2697163406],\"outcomePositions\":[5,4,3,2,1],\"patternIndex\":30}],\"selection\":[[],[],[],[],[]]}}";
                return JToken.Parse(json);
            }
        }

        private JToken CheckedCouponBySite
        {
            get
            {
                var json = "{\"responseCoupon\":{\"status\":200,\"requestCoupon\":{\"type\":\"RCT_SYSTEM\",\"outcomeIds\":[[2697163406],[2697163429],[2697432681],[2697432704],[2697432752]],\"odds\":[1250,7000,2380,4500,3750],\"betsPattern\":\"1111111111111111111111111111111\",\"allowOddsChange\":\"AOCT_NO\",\"selection\":[[],[],[],[],[]],\"suggestedRetailPunterCategory\":null}}}";
                return JToken.Parse(json)["responseCoupon"]["requestCoupon"];
            }
        }
        /****************************************/
        private JArray InitOutcomes()
        {
            // horse and event info
            var json = 
                "[{\n\"id\": 2697163406,\n\"outcomeId\": 2697163406,\n\"betofferId\": 2197796095,\n\"eventId\": 1006023635,\n\"approvedOdds\": 1250,\n\"oddsApproved\": true,\n\"approvedEachWayFractionMilli\": 250,\n\"approvedEachWayPlaceLimit\": 2,\n\"eachWayFractionMilli\": 250,\n\"eachWayPlaceLimit\": 2,\n\"eachWayApproved\": true,\n\"isLiveBetoffer\": false,\n\"isPrematchBetoffer\": true,\n\"fromBetBuilder\": false,\n\"source\": \"Event List View\"\n}," +
                "{\n\"id\": 2697163429,\n\"outcomeId\": 2697163429,\n\"betofferId\": 2197796101,\n\"eventId\": 1006023636,\n\"approvedOdds\": 7000,\n\"oddsApproved\": true,\n\"approvedEachWayFractionMilli\": 200,\n\"approvedEachWayPlaceLimit\": 5,\n\"eachWayFractionMilli\": 200,\n\"eachWayPlaceLimit\": 5,\n\"eachWayApproved\": true,\n\"isLiveBetoffer\": false,\n\"isPrematchBetoffer\": true,\n\"fromBetBuilder\": false,\n\"source\": \"Event List View\"\n}," +
                "{\n\"id\": 2697432681,\n\"outcomeId\": 2697432681,\n\"betofferId\": 2197879119,\n\"eventId\": 1006025535,\n\"approvedOdds\": 2380,\n\"oddsApproved\": true,\n\"approvedEachWayFractionMilli\": 250,\n\"approvedEachWayPlaceLimit\": 2,\n\"eachWayFractionMilli\": 250,\n\"eachWayPlaceLimit\": 2,\n\"eachWayApproved\": true,\n\"isLiveBetoffer\": false,\n\"isPrematchBetoffer\": true,\n\"fromBetBuilder\": false,\n\"source\": \"Event List View\"\n}," +
                "{\n\"id\": 2697432704,\n\"outcomeId\": 2697432704,\n\"betofferId\": 2197879125,\n\"eventId\": 1006025536,\n\"approvedOdds\": 4500,\n\"oddsApproved\": true,\n\"approvedEachWayFractionMilli\": 200,\n\"approvedEachWayPlaceLimit\": 3,\n\"eachWayFractionMilli\": 200,\n\"eachWayPlaceLimit\": 3,\n\"eachWayApproved\": true,\n\"isLiveBetoffer\": false,\n\"isPrematchBetoffer\": true,\n\"fromBetBuilder\": false,\n\"source\": \"Event List View\"\n}," +
                "{\n\"id\": 2697432752,\n\"outcomeId\": 2697432752,\n\"betofferId\": 2197879131,\n\"eventId\": 1006025537,\n\"approvedOdds\": 3750,\n\"oddsApproved\": true,\n\"approvedEachWayFractionMilli\": 250,\n\"approvedEachWayPlaceLimit\": 3,\n\"eachWayFractionMilli\": 250,\n\"eachWayPlaceLimit\": 3,\n\"eachWayApproved\": true,\n\"isLiveBetoffer\": false,\n\"isPrematchBetoffer\": true,\n\"fromBetBuilder\": false,\n\"source\": \"Event List View\"\n}]";
            
            return JToken.Parse(json) as JArray;
        }

        private ILogger InitLogger()
        {
            AppLogger.LoggerFactory.AddSmartInspect(new SmartInspectConfiguration
            {
                AppName = "Tester",
                Enabled = true,
                Level = LogLevel.Trace,
                DefaultLevel = LogLevel.Trace,
                Connections = @"pipe(reconnect=""true"", reconnect.interval=""5"", async.enabled=""true"", async.throttle=""false"")"
            });
            return AppLogger.CreateLogger("Main");
        }
    }
}
