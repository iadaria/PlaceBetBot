using Betting.Core;
using Newtonsoft.Json.Linq;
using System;

namespace Placer.Ecambi
{
    public class HorseInfo : ICloneable
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public MarketType MarketType { get; set; }
        public JToken RaceHorseJData { get; set; }
        public RaceInfo Race { get; set; }
        public decimal Odds { get; set; }
        public int PlaceLimit { get; set; }
        public JToken SelectedOutcomeJData {get; set;}

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
