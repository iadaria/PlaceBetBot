using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placer.Ecambi
{
    //Copy
    public class RaceInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public string OriginalStartTimeStr { get; set; }
        public string Region { get; set; }
        public string State { get; set; } //Ex. "NOT_STARTED"

    }
}
