using Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Placer.WebCore;
using System;
using Http462;
using Http462.Headers;
using Http462.Content;
using NetworkLib462.Net;
using System.Runtime.Remoting.Messaging;
using FluentScraper.Extension;
using Newtonsoft.Json;
using Placer.Ecambi;

namespace Placer.Red32
{

    internal class Red32Web : EcambiWeb
    {
        public Red32Web(
            string userAgent,
            string acceptLanguage,
            string acceptEncoding,
            IWebProxy proxy,
            string mainDomain,
            string market,
            string lang)
            :base(userAgent, acceptLanguage, acceptEncoding, proxy, mainDomain, "32red", market, lang){}

        protected override FormUrlEncodedContent CreateAuthenticationInfo(string username, string password, string firstname) =>
            new Http462.Content.FormUrlEncodedContent
            {
                {"username", username},
                {"password", password},
                {"state", "Pending+Authentication"},
                {"stateDetails", ""},
                {"firstname", firstname},
                {"recapcha", ""},
            };

        public override JToken Login(string username, string password, string firstname)
        {
            var url = $"{MainDomain}account/login";
            var content = CreateAuthenticationInfo(username, password, firstname);
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Post,
                RequestUri = new Uri(url),
                RequestType = HttpRequestType.Data,
                Content = content,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Content-Length", null},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"Origin", $"{MainDomain}"},
                    {"X-Requested-With", "XMLHttpRequest"},
                    {"User-Agent", UserAgent},
                    {"Content-Type", "application/x-www-form-urlencoded; charset=UTF-8"},
                    {"Sec-Fetch-Site", "same-origin"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                    {"Cookie", null},
                },
            };
            var response = ExecuteAndRead(request);
            return response.ToJson();
        }

        public override void GetSessionIdOption()
        {
            var request= new HttpRequest
            {
                Method = HttpRequestMethod.Options,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/32red/punter/login.json?settings=true&lang={_lang}"),
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
   
        public override JToken GetSessionId(string token, string hash, string userName)
        {
            var contentForGetSession = CreateContentForGetSession(token, hash, userName);
            var serializedContent = JsonConvert.SerializeObject(contentForGetSession);
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Post,
                RequestUri = new Uri("https://al-auth.kambicdn.org/player/api/v2/32red/punter/login.json?settings=true&lang=en_GB"),
                RequestType = HttpRequestType.Data,
                Content = new StringContent(serializedContent),
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
            var response = Web.Execute(request).ReadAsString();
            return response.ToJson();
        }

        private dynamic CreateContentForGetSession(string ticket, string hash, string userName) =>
            new
            {
                attributes = new { fingerprintHash = hash },
                customerData = "",
                punterId = userName,
                streamingAllowed = true,
                ticket = ticket,
                market = _market
            };


        public JToken GetCredentials()
        {
            var url = $"{MainDomain}sportsbook/getCredentials";
            var content = new StringContent("");
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Post,
                RequestUri = new Uri(url),
                RequestType = HttpRequestType.Data,
                Content = content,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Content-Length", null},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"Origin", $"{MainDomain}"},
                    {"X-Requested-With", "XMLHttpRequest"},
                    {"User-Agent", UserAgent},
                    {"Sec-Fetch-Site", "same-origin"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                    {"Cookie", null},
                },
            };
            var response = ExecuteAndRead(request);
            return response.ToJson();
        }

        public JToken GetStatus()
        {
            var url = $"{MainDomain}api/status";
            var request = new HttpRequest
            {
                RequestUri = new Uri(url),
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"X-Requested-With", "XMLHttpRequest"},
                    {"User-Agent", UserAgent},
                    {"Sec-Fetch-Site", "same-origin"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                    {"Cookie", null},
                },
            };
            return ExecuteAndRead(request).ToJson();
        }
    }
}
