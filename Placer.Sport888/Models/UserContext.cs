using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placer.Sport888.Models
{
    internal class UserContext
    {
        public bool isAuthenticated { get; set; } = false;
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userBalance { get; set; }
        public string realBR { get; set; }
        public string bonusBalance { get; set; }
        public bool showBonusBalance { get; set; }
        public string username { get; set; }
        public int cid { get; set; }
        public bool isVip { get; set; }
        public string deviceType { get; set; }
        public int brandId { get; set; }
        public DateTime? lastLogin { get; set; }
        public int sessionTime { get; set; }
    }
}
