﻿using Betting.Core;
using Logging;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.SmartInspect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Placer.Ecambi;
using Placer.Sport888;

namespace MyTester
{
    [TestClass]
    public class SingleBetTests
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
            betType = BetType.Single;
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
                var toValidateCoupon = helper.CreateCouponToValidate(betType, outcomes);
                var toValidateCouponBySite = ToValidateCouponGeneratedBySite;

                logger.LogDebug("Generated to validate coupon:\n" + toValidateCoupon.GetDump());
                logger.LogDebug("Validate coupon was got from site:\n" + toValidateCouponBySite["requestCoupon"].GetDump());

                Assert.IsTrue(JToken.DeepEquals(toValidateCoupon, toValidateCouponBySite["requestCoupon"]));
            }
        }
        [TestMethod]
        public void Get_Success_PlaceBetCoupon_Tests()
        {
            using(logger.BeginScope(LoggerHelper.GetCaller()))
            {
                var validatedCoupon = GotValidatedCouponFromSite();
                var placeBet
                    = helper.GenerateCouponToPlaceBet(outcomes, validatedCoupon, betType, stake, ew);

                logger.LogDebug("Validate coupon:\n" + validatedCoupon.GetDump());
                logger.LogDebug("Generated place bet coupon:\n" + placeBet.GetDump());
                logger.LogDebug("Place bet coupon was got from site:\n" + PlaceBetGeneratedBySite.GetDump());

                Assert.IsTrue(JToken.DeepEquals(placeBet, PlaceBetGeneratedBySite));
            }        
        }

        private JArray InitOutcomes()
        {
            // horse and event info
            var json = "{ \"id\": 2697432752, \"outcomeId\": 2697432752, \"betofferId\": 2197879131, \"eventId\": 1006025537, \"approvedOdds\": 3750, \"oddsApproved\": true, \"approvedEachWayFractionMilli\": 250, \"approvedEachWayPlaceLimit\": 3, \"eachWayFractionMilli\": 250, \"eachWayPlaceLimit\": 3, \"eachWayApproved\": true, \"isLiveBetoffer\": false, \"isPrematchBetoffer\": true, \"fromBetBuilder\": false, \"source\": \"Event List View\" }";
            return new JArray(JToken.Parse(json));
        }

        /**** Data was generated by site ****/
        private JToken ToValidateCouponGeneratedBySite
        {
            get
            {
                var json = "{\"requestCoupon\":{\"type\":\"RCT_SYSTEM\",\"odds\":[3750],\"outcomeIds\":[[2697432752]],\"selection\":[[]],\"betsPattern\":\"1\",\"isUserLoggedIn\":false}}";
                return JToken.Parse(json);
            }
        }

        private JToken PlaceBetGeneratedBySite
        {
            get
            {
                var json = "{\"id\":1,\"trackingData\":{\"hasTeaser\":false,\"isBetBuilderCombination\":false,\"selectedOutcomes\":[{\"id\":2697432752,\"outcomeId\":2697432752,\"betofferId\":2197879131,\"eventId\":1006025537,\"approvedOdds\":3750,\"oddsApproved\":true,\"approvedEachWayFractionMilli\":250,\"approvedEachWayPlaceLimit\":3,\"eachWayFractionMilli\":250,\"eachWayPlaceLimit\":3,\"eachWayApproved\":true,\"isLiveBetoffer\":false,\"isPrematchBetoffer\":true,\"fromBetBuilder\":false,\"source\":\"Event List View\"}]},\"requestCoupon\":{\"allowOddsChange\":\"AOCT_NO\",\"odds\":[3750],\"stakes\":[10],\"outcomeIds\":[[2697432752]],\"type\":\"RCT_SINGLE\",\"selection\":[[]]}}";
                return JToken.Parse(json);
            }
        }

        private JToken GotValidatedCouponFromSite()
        {
            var json = "{\"type\":\"RCT_SYSTEM\",\"outcomeIds\":[[2697432752]],\"odds\":[3750],\"betsPattern\":\"1\",\"allowOddsChange\":\"AOCT_NO\",\"selection\":[[]],\"suggestedRetailPunterCategory\":null}";
            return JToken.Parse(json);
        }
        /****************************************/
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
