using FluentScraper.Extension;
using Http462;
using Http462.Content;
using Http462.Headers;
using Logging;
using Microsoft.Extensions.Logging;
using NetworkLib462.Net;
using Newtonsoft.Json.Linq;
using Placer.WebCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;


namespace Placer.Ecambi
{
    public abstract class EcambiWeb : BaseWeb
    {
        public Uri MainDomain;
        protected readonly string _market;
        protected readonly string _lang;
        protected readonly string _name;
        public EcambiWeb(
            string userAgent,
            string acceptLanguage,
            string acceptEncoding,
            IWebProxy proxy,
            string mainDomain,
            string name, //sample 32red or 888
            string market,
            string lang)
            : base(userAgent, acceptLanguage, acceptEncoding, proxy)
        {
            _market = market;
            _lang = lang;
            _name = name;

            MainDomain = new Uri(mainDomain);

            var book = (string)CallContext.LogicalGetData("Bookie");
            var user = (string)CallContext.LogicalGetData("UserName");

            Logger = AppLogger.CreateLogger($"{this.GetType().Name}({book}, {user})");
            Logger.LogDebug($"{this.GetType().Name}");
        }

        public abstract JToken Login(string username, string password, string firstname = "");
        protected abstract FormUrlEncodedContent CreateAuthenticationInfo(string username, string password, string firstname);
        public abstract void GetSessionIdOption();
        public abstract JToken GetSessionId(string token, string hash = "", string userName = "");

        public void ValidateCouponOption(string sessionId)
        {
            var timestamp = GetCurrentTimeStampMs;
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Options,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/{_name}/coupon/validate.json;jsessionid={sessionId}?lang={_lang}&market={_market}&client_id=2&channel_id=1&ncid={timestamp}"),
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Pragma", "no-cache"},           //difference
                    {"Cache-Control", "no-cache"},    //difference
                    {"Access-Control-Request-Method", "POST"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Access-Control-Request-Headers", "content-type"},
                    {"Accept", "*/*"},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            ExecuteAndRead(request);
        }

        public JToken ValidateCoupon(string sessionId, JToken requestCoupon)
        {
            var timestamp = GetCurrentTimeStampMs;

            var content = CreateContentForValidateCoupon(requestCoupon);

            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Post,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/{_name}/coupon/validate.json;jsessionid={sessionId}?lang={_lang}&market={_market}&client_id=2&channel_id=1&ncid={timestamp}"),
                RequestType = HttpRequestType.Data,
                Content = content,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Content-Length", null},
                    {"Pragma", "no-cache"},        //difference
                    {"Cache-Control", "no-cache"}, //differene
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Content-Type", "application/json"},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            var response = Execute(request, false).ReadAsString();

            return response.ToJson();
        }

        private StringContent CreateContentForValidateCoupon(JToken requestCoupon)
        {
            var content = new JObject
            {
                { "requestCoupon", requestCoupon }
            };
            return new StringContent(content.ToString());
        }

        public void ValidateCouponOptionNoAuth()
        {
            var timestamp = GetCurrentTimeStampMs;
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Options,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/{_name}/coupon/validate.json?lang={_lang}&market={_market}&client_id=2&channel_id=1&ncid={timestamp}"),
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Access-Control-Request-Method", "POST"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Access-Control-Request-Headers", "content-type"},
                    {"Accept", "*/*"},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            ExecuteAndRead(request);
        }

        public JToken ValidateCouponNoAuth(JToken requestCoupon)
        {
            var timestamp = GetCurrentTimeStampMs;
            var content = CreateContentForValidateCoupon(requestCoupon);
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Post,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/{_name}/coupon/validate.json?lang={_lang}&market={_market}&client_id=2&channel_id=1&ncid={timestamp}"),
                RequestType = HttpRequestType.Data,
                Content = content,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Content-Length", null},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Content-Type", "application/json"},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            var response = Execute(request, false).ReadAsString();

            return response.ToJson();
        }

        public void PlaceOption(string sessionId)
        {
            var timestamp = GetCurrentTimeStampMs;
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Options,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/{_name}/coupon.json;jsessionid={sessionId}?lang={_lang}&market={_market}&client_id=2&channel_id=1&ncid={timestamp}"),
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Pragma", "no-cache"},
                    {"Cache-Control", "no-cache"},
                    {"Access-Control-Request-Method", "PUT"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Access-Control-Request-Headers", "content-type"},
                    {"Accept", "*/*"},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            ExecuteAndRead(request);
        }
        public JToken Place(string sessionId, JToken placeBet)
        {
            var timestamp = GetCurrentTimeStampMs;
            var content = new StringContent(placeBet.ToString());

            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Put,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/{_name}/coupon.json;jsessionid={sessionId}?lang={_lang}&market={_market}&client_id=2&channel_id=1&ncid={timestamp}"),
                RequestType = HttpRequestType.Data,
                Content = content,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Content-Length", null},
                    {"Pragma", "no-cache"},
                    {"Cache-Control", "no-cache"},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Content-Type", "application/json"},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            var response = Web.Execute(request);
            var responsePlace = response.ReadAsString();

            LogRequest(request);
            LogResponse(response, responsePlace);

            return responsePlace.ToJson();
        }

        public JToken GetHorseRacing()
        {
            var request = new HttpRequest
            {
                RequestUri = new Uri($"https://eu-offering.kambicdn.org/offering/v2018/{_name}/meeting/horse_racing.json?lang={_lang}&market={_market}"),
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            return ExecuteAndRead(request).ToJson();
        }

        public JToken GetBetoffer(long _event)
        {
            var request = new HttpRequest
            {
                RequestUri = new Uri($"https://eu-offering.kambicdn.org/offering/v2018/{_name}/betoffer/event/{_event}.json?lang={_lang}&market={_market}&includeParticipants=false"), //client_id=2&channel_id=1&ncid=1575555383224&
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };

            return ExecuteAndRead(request).ToJson();
        }

        public long GetCurrentTimeStampMs =>
           (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
    }
}
