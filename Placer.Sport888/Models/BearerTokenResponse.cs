using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placer.Sport888.Models
{
    public class BearerTokenResponse
    {
        public string BearerToken { get; set; }
        public string RefreshToken { get; set; }
        public long RefreshTokenexperationTimeSpanInMS { get; set; }
        public string CID { get; set; }
        //public object PendingToken { get; set; }
        //public object SpecificNavigation { get; set; }
        //public object SubBrand { get; set; }
        //public object ErrorMessage { get; set; }
        //public string MainPage { get; set; }
        //public int ReasonCode { get; set; }
        //public FullResponseAfterLogin FullResponse { get; set; }
        //public int ErrorCode { get; set; }
        //public object ErrorDescription { get; set; }
        //public bool isOk { get; set; }
    }
}
