using Betting.Core;
using Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.SmartInspect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Placer.Ecambi;
using Placer.Sport888;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyTester
{
    [TestClass]
    public class BetslipHelperTests
    {
        private BetslipHelper helper;
        private JArray outcomes;
        private ILogger logger;

        [TestInitialize]
        public void Init()
        {
            helper = new BetslipHelper();
            //outcomes = InitOutcomes();
            logger = InitLogger();
        }

        [TestMethod]
        public void PatternForValidateTest()
        {
            Assert.AreEqual(helper.GetBetsPatternForValidate(BetType.Singles, 1), "1");
            Assert.AreEqual(helper.GetBetsPatternForValidate(BetType.Singles, 2), "111");
            Assert.AreEqual(helper.GetBetsPatternForValidate(BetType.Singles, 3), "1111111");
        }

        [TestMethod]
        public void CreateSystemCombinationsTest()
        {
            var outcomes = InitOutcomesFive();
            var combinations = helper.CreateSystemCombinations(BetType.Singles, outcomes);

            logger.LogDebug(combinations.ToString());
            logger.LogDebug(PlaceBetOutcomesFiveSingles().ToString());

            Assert.AreEqual(PlaceBetOutcomesFiveSingles().ToString(), combinations.ToString());
        }

        [TestMethod]
        public void BetsPatternForFiveInSinglesTest()
        {
            var outcomes = InitOutcomesFive();
            var combinations = helper.CreateSystemCombinations(BetType.Singles, outcomes);
            var outcomeIds = combinations.Select(combination => combination["outcomeIds"]);
            var betsPattern = helper.GenerateBetsPatternForPlace(BetType.Singles, new JArray(outcomeIds));

            Assert.AreEqual(GetBetsPatternForFiveInSingles(), betsPattern);
        }

        [TestMethod]
        public void GetIndexesOutcomeIdsTest()
        {
            outcomes = InitOutcomesThree();

            var outcomeIds = new JArray();
            var reverseOutcomes = new JArray();
            
            foreach(var outcome in outcomes)
            {
                outcomeIds.Add(outcome["id"]);
            }
            reverseOutcomes = Reverse(outcomes);
            var indexes = helper.GetIndexesOutcomeIds(reverseOutcomes, outcomeIds);
            var should = new JArray { 3, 2, 1 };

            CollectionAssert.AreEqual(should, indexes);
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

        public JArray Reverse(JArray outcomes)
        {
            var reverse = new JArray();
            for (int i = outcomes.Count - 1; i >= 0; i--) reverse.Add(outcomes[i]);

            return reverse;
        }

        private JArray InitOutcomesFive()
        {
            var json = 
               "[{ \"id\": 2697110001, \"outcomeId\": 2697110001, \"betofferId\": 2197782190, \"eventId\": 1006023011, \"approvedOdds\": 1800, \"oddsApproved\": true, \"eachWayApproved\": true, \"isLiveBetoffer\": false, \"isPrematchBetoffer\": true, \"fromBetBuilder\": false, \"source\": \"Event List View\" }," +
                "{ \"id\": 2697110019, \"outcomeId\": 2697110019, \"betofferId\": 2197782196, \"eventId\": 1006023012, \"approvedOdds\": 1300, \"oddsApproved\": true, \"eachWayApproved\": true, \"isLiveBetoffer\": false, \"isPrematchBetoffer\": true, \"fromBetBuilder\": false, \"source\": \"Event List View\" }," +
                "{ \"id\": 2697110074, \"outcomeId\": 2697110074, \"betofferId\": 2197782208, \"eventId\": 1006023014, \"approvedOdds\": 1910, \"oddsApproved\": true, \"approvedEachWayFractionMilli\": 200, \"approvedEachWayPlaceLimit\": 3, \"eachWayFractionMilli\": 200, \"eachWayPlaceLimit\": 3, \"eachWayApproved\": true, \"isLiveBetoffer\": false, \"isPrematchBetoffer\": true, \"fromBetBuilder\": false, \"source\": \"Event List View\" },"+
                "{ \"id\": 2697110117, \"outcomeId\": 2697110117, \"betofferId\": 2197782214, \"eventId\": 1006023015, \"approvedOdds\": 4350, \"oddsApproved\": true, \"approvedEachWayFractionMilli\": 200, \"approvedEachWayPlaceLimit\": 3, \"eachWayFractionMilli\": 200, \"eachWayPlaceLimit\": 3, \"eachWayApproved\": true, \"isLiveBetoffer\": false, \"isPrematchBetoffer\": true, \"fromBetBuilder\": false, \"source\": \"Event List View\" },"+
                "{ \"id\": 2697110162, \"outcomeId\": 2697110162, \"betofferId\": 2197782220, \"eventId\": 1006023016, \"approvedOdds\": 2750, \"oddsApproved\": true, \"approvedEachWayFractionMilli\": 340, \"approvedEachWayPlaceLimit\": 2, \"eachWayFractionMilli\": 340, \"eachWayPlaceLimit\": 2, \"eachWayApproved\": true, \"isLiveBetoffer\": false, \"isPrematchBetoffer\": true, \"fromBetBuilder\": false, \"source\": \"Event List View\" }]";

            return JArray.Parse(json) as JArray;//.OrderBy(outcome => (long)outcome["id"]);;
        }

        private JArray InitOutcomesThree()
        {
            var json = "";

            json += "[{\n\"id\": 2697110019,\n\"outcomeId\": 2697110019,\n      \"betofferId\": 2197782196,\n      \"eventId\": 1006023012,\n      \"approvedOdds\": 1300,\n      \"oddsApproved\": true,\n      \"eachWayApproved\": true,\n      \"isLiveBetoffer\": false,\n      \"isPrematchBetoffer\": true,\n      \"fromBetBuilder\": false,\n      \"source\": \"Event List View\"\n    },";
            json += "{\n\"id\": 2697110074,\n\"outcomeId\": 2697110074,\n      \"betofferId\": 2197782208,\n      \"eventId\": 1006023014,\n      \"approvedOdds\": 1910,\n      \"oddsApproved\": true,\n      \"approvedEachWayFractionMilli\": 200,\n      \"approvedEachWayPlaceLimit\": 3,\n      \"eachWayFractionMilli\": 200,\n      \"eachWayPlaceLimit\": 3,\n      \"eachWayApproved\": true,\n      \"isLiveBetoffer\": false,\n      \"isPrematchBetoffer\": true,\n      \"fromBetBuilder\": false,\n      \"source\": \"Event List View\"\n    },";
            json += "{\n\"id\": 2697110162,\n\"outcomeId\": 2697110162,\n\"betofferId\": 2197782220,\n\"eventId\": 1006023016,\n\"approvedOdds\": 2750,\n\"oddsApproved\": true,\n\"approvedEachWayFractionMilli\": 340,\n\"approvedEachWayPlaceLimit\": 2,\n\"eachWayFractionMilli\": 340,\n\"eachWayPlaceLimit\": 2,\n\"eachWayApproved\": true,\n\"isLiveBetoffer\": false,\n\"isPrematchBetoffer\": true,\n\"fromBetBuilder\": false,\n\"source\": \"Event List View\"\n}]";

            return JArray.Parse(json);
        }


        /********************************  Data got from the site *************************************/
        private string GetBetsPatternForFiveInSingles() => "1101000100000001000000000000000";

        private JArray PlaceBetOutcomesFiveSingles()
        {
            var json = "[{\"outcomeIds\":[2697110162],\"outcomePositions\":[5],\"patternIndex\":0},{\"outcomeIds\":[2697110117],\"outcomePositions\":[4],\"patternIndex\":1},{\"outcomeIds\":[2697110162,2697110117],\"outcomePositions\":[5,4],\"patternIndex\":2},{\"outcomeIds\":[2697110074],\"outcomePositions\":[3],\"patternIndex\":3},{\"outcomeIds\":[2697110162,2697110074],\"outcomePositions\":[5,3],\"patternIndex\":4},{\"outcomeIds\":[2697110117,2697110074],\"outcomePositions\":[4,3],\"patternIndex\":5},{\"outcomeIds\":[2697110162,2697110117,2697110074],\"outcomePositions\":[5,4,3],\"patternIndex\":6},{\"outcomeIds\":[2697110019],\"outcomePositions\":[2],\"patternIndex\":7},{\"outcomeIds\":[2697110162,2697110019],\"outcomePositions\":[5,2],\"patternIndex\":8},{\"outcomeIds\":[2697110117,2697110019],\"outcomePositions\":[4,2],\"patternIndex\":9},{\"outcomeIds\":[2697110162,2697110117,2697110019],\"outcomePositions\":[5,4,2],\"patternIndex\":10},{\"outcomeIds\":[2697110074,2697110019],\"outcomePositions\":[3,2],\"patternIndex\":11},{\"outcomeIds\":[2697110162,2697110074,2697110019],\"outcomePositions\":[5,3,2],\"patternIndex\":12},{\"outcomeIds\":[2697110117,2697110074,2697110019],\"outcomePositions\":[4,3,2],\"patternIndex\":13},{\"outcomeIds\":[2697110162,2697110117,2697110074,2697110019],\"outcomePositions\":[5,4,3,2],\"patternIndex\":14},{\"outcomeIds\":[2697110001],\"outcomePositions\":[1],\"patternIndex\":15},{\"outcomeIds\":[2697110162,2697110001],\"outcomePositions\":[5,1],\"patternIndex\":16},{\"outcomeIds\":[2697110117,2697110001],\"outcomePositions\":[4,1],\"patternIndex\":17},{\"outcomeIds\":[2697110162,2697110117,2697110001],\"outcomePositions\":[5,4,1],\"patternIndex\":18},{\"outcomeIds\":[2697110074,2697110001],\"outcomePositions\":[3,1],\"patternIndex\":19},{\"outcomeIds\":[2697110162,2697110074,2697110001],\"outcomePositions\":[5,3,1],\"patternIndex\":20},{\"outcomeIds\":[2697110117,2697110074,2697110001],\"outcomePositions\":[4,3,1],\"patternIndex\":21},{\"outcomeIds\":[2697110162,2697110117,2697110074,2697110001],\"outcomePositions\":[5,4,3,1],\"patternIndex\":22},{\"outcomeIds\":[2697110019,2697110001],\"outcomePositions\":[2,1],\"patternIndex\":23},{\"outcomeIds\":[2697110162,2697110019,2697110001],\"outcomePositions\":[5,2,1],\"patternIndex\":24},{\"outcomeIds\":[2697110117,2697110019,2697110001],\"outcomePositions\":[4,2,1],\"patternIndex\":25},{\"outcomeIds\":[2697110162,2697110117,2697110019,2697110001],\"outcomePositions\":[5,4,2,1],\"patternIndex\":26},{\"outcomeIds\":[2697110074,2697110019,2697110001],\"outcomePositions\":[3,2,1],\"patternIndex\":27},{\"outcomeIds\":[2697110162,2697110074,2697110019,2697110001],\"outcomePositions\":[5,3,2,1],\"patternIndex\":28},{\"outcomeIds\":[2697110117,2697110074,2697110019,2697110001],\"outcomePositions\":[4,3,2,1],\"patternIndex\":29},{\"outcomeIds\":[2697110162,2697110117,2697110074,2697110019,2697110001],\"outcomePositions\":[5,4,3,2,1],\"patternIndex\":30}]";

            return JArray.Parse(json);
        }


    }
}
