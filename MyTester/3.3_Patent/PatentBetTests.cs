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
    public class PatentBetTests
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
            betType = BetType.Patent;
            stake = 0.01;
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

                //logger.LogDebug("Generated to validate coupon:\n" + toValidateCoupon.GetDump());
                //logger.LogDebug("Validate coupon was got from site:\n" + ToValidateCouponBySite["requestCoupon"].GetDump());

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
                var json = "{\"requestCoupon\":{\"type\":\"RCT_SYSTEM\",\"odds\":[-1,-1,-1],\"outcomeIds\":[[2698429397],[2698429472],[2698429573]],\"selection\":[[],[],[]],\"betsPattern\":\"1111111\",\"isUserLoggedIn\":true}}";
                return JToken.Parse(json);
            }
        }

        private JToken CheckedCouponBySite
        {
            get
            {
                var json = "{\"responseCoupon\":{\"status\":200,\"requestCoupon\":{\"type\":\"RCT_SYSTEM\",\"outcomeIds\":[[2698429397],[2698429472],[2698429573]],\"odds\":[-1,-1,-1],\"betsPattern\":\"1111111\",\"allowOddsChange\":\"AOCT_NO\",\"selection\":[[],[],[]],\"suggestedRetailPunterCategory\":null}}}";
                return JToken.Parse(json)["responseCoupon"]["requestCoupon"];
            }
        }
        private JToken PlaceBetBySite
        {
            get
            {
                var json =
                  "{\"id\":1,\"trackingData\":{\"hasTeaser\":false,\"isBetBuilderCombination\":false,\"selectedOutcomes\":" +
                  "[{\"id\":2698429397,\"outcomeId\":2698429397,\"betofferId\":2198205720,\"eventId\":1006030019,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":250,\"approvedEachWayPlaceLimit\":2,\"eachWayFractionMilli\":250,\"eachWayPlaceLimit\":2,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}," +
                  "{\"id\":2698429472,\"outcomeId\":2698429472,\"betofferId\":2198205732,\"eventId\":1006030021,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":200,\"approvedEachWayPlaceLimit\":3,\"eachWayFractionMilli\":200,\"eachWayPlaceLimit\":3,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}," +
                  "{\"id\":2698429573,\"outcomeId\":2698429573,\"betofferId\":2198205750,\"eventId\":1006030024,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":200,\"approvedEachWayPlaceLimit\":3,\"eachWayFractionMilli\":200,\"eachWayPlaceLimit\":3,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}]}," +
                  "\"requestCoupon\":{\"allowOddsChange\":\"AOCT_NO\",\"odds\":[-1,-1,-1],\"stakes\":[10,10,10,10,10,10,10],\"outcomeIds\":[[2698429397],[2698429472],[2698429573]],\"type\":\"RCT_SYSTEM\",\"betsPattern\":\"1111111\",\"systemCombinations\":[{\"outcomeIds\":[2698429573],\"outcomePositions\":[3],\"patternIndex\":0},{\"outcomeIds\":[2698429472],\"outcomePositions\":[2],\"patternIndex\":1},{\"outcomeIds\":[2698429573,2698429472],\"outcomePositions\":[3,2],\"patternIndex\":2},{\"outcomeIds\":[2698429397],\"outcomePositions\":[1],\"patternIndex\":3},{\"outcomeIds\":[2698429573,2698429397],\"outcomePositions\":[3,1],\"patternIndex\":4},{\"outcomeIds\":[2698429472,2698429397],\"outcomePositions\":[2,1],\"patternIndex\":5},{\"outcomeIds\":[2698429573,2698429472,2698429397],\"outcomePositions\":[3,2,1],\"patternIndex\":6}],\"selection\":[[],[],[]]}}";
                return JToken.Parse(json);
            }
        }
        /****************************************/
        private JArray InitOutcomes()
        {
            // horse and event info
            var json =
                    "[{\"id\":2698429397,\"outcomeId\":2698429397,\"betofferId\":2198205720,\"eventId\":1006030019,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":250,\"approvedEachWayPlaceLimit\":2,\"eachWayFractionMilli\":250,\"eachWayPlaceLimit\":2,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}," +
                    "{\"id\":2698429472,\"outcomeId\":2698429472,\"betofferId\":2198205732,\"eventId\":1006030021,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":200,\"approvedEachWayPlaceLimit\":3,\"eachWayFractionMilli\":200,\"eachWayPlaceLimit\":3,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}," +
                    "{\"id\":2698429573,\"outcomeId\":2698429573,\"betofferId\":2198205750,\"eventId\":1006030024,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":200,\"approvedEachWayPlaceLimit\":3,\"eachWayFractionMilli\":200,\"eachWayPlaceLimit\":3,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}]";
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
