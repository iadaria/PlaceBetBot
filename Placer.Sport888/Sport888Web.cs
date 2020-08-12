using System;
using HtmlAgilityPack;
using Http462;
using Http462.Headers;
using NetworkLib462.Net;
using FluentScraper.Extension;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Http462.Content;
using System.Linq;
using Placer.Ecambi;

namespace Placer.Sport888
{
    internal class Sport888Web: EcambiWeb//BaseWeb
    {
        public Sport888Web(
            string userAgent, 
            string acceptLanguage, 
            string acceptEncoding, 
            IWebProxy proxy,
            string mainDomain,
            string market,
            string lang)
            :base(userAgent, acceptLanguage, acceptEncoding, proxy, mainDomain, "888", market, lang) {}

        protected override FormUrlEncodedContent CreateAuthenticationInfo(string username, string password, string firstname = "")
        {
            var authenticationInfo = "{\"credentials\":{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}}";

            return new FormUrlEncodedContent
            {
                {"LangID", "eng"},
                {"LoginType", "1"},
                {"BrandID", "8"},
                {"SubBrandId", "8"},
                {"AuthenticationInfo", authenticationInfo},
                {"TargetProductPackage", "9"},
                {"SourceProductPackage", "9"},
                {"ClientVersion", "S-8-1-9"},
                {"ScreenHeight", "1080"},
                {"ScreenWidth", "1920"},
                {"Benchmark", "0"},
                {"DevicePixelRatio", "1"},
            };
        }
        public override JToken Login(string username, string password, string firstname="")
        {
            var url = $"{MainDomain}api/ManualLogin/Post/";

            var authenticationInfo = CreateAuthenticationInfo(username, password);

            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Post,

                RequestUri = new Uri(url),
                RequestType = HttpRequestType.Data,
                Content = authenticationInfo,

                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Cache-Control", "no-cache"},
                    {"Origin", $"{MainDomain}" },//.Remove(-1, 1)},
                    {"Upgrade-Insecure-Requests", "1"},
                    {"Content-Type", "application/x-www-form-urlencoded"},
                    {"User-Agent", UserAgent},
                    {"Sec-Fetch-Site", "same-site"},
                    {"Sec-Fetch-Mode", "nested-navigate"},
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
            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Options,
                RequestUri = new Uri($"https://al-auth.kambicdn.org/player/api/v2/888/punter/login.json?settings=true&lang={_lang}"),
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Pragma", "no-cache"},
                    {"Cache-Control", "no-cache"},
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

        public override JToken GetSessionId(string token, string hash = "", string userName = "")
        {
            var contentForGetSession = CreateContentForGetSession(token);
            var content = JsonConvert.SerializeObject(contentForGetSession);

            var request = new HttpRequest
            {
                Method = HttpRequestMethod.Post,
                RequestUri = new Uri($"https://al-auth-api.kambi.com/player/api/v2/888/punter/login.json?lang={_lang}&settings=false"),
                RequestType = HttpRequestType.Data,
                Content = new StringContent(content),
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Content-Length", null},
                    {"Pragma", "no-cache"},
                    {"Cache-Control", "no-cache"},
                    {"Accept", "application/json, text/plain, */*"},
                    {"Origin", $"{MainDomain}"},
                    {"User-Agent", UserAgent},
                    {"Content-Type", "application/json;charset=UTF-8"},
                    {"Sec-Fetch-Site", "cross-site"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                },
            };
            var response = ExecuteAndRead(request);

            return response.ToJson();
        }

        private dynamic CreateContentForGetSession(string sdkToken) =>
            new
            {
                ticket = sdkToken,
                market = _market,
                customerData = 8,
                streamingAllowed = true,
                punterId = ""
            };
        
        public HtmlDocument GetMainPage(string expireToken = "")
        {
            var request = new HttpRequest
            {
                RequestUri = MainDomain,
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Upgrade-Insecure-Requests", "1"},
                    {"User-Agent", UserAgent},
                    {"ExpirationToken", expireToken},
                    {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
                    {"Sec-Fetch-Site", "none"},
                    {"Sec-Fetch-Mode", "navigate"},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                    {"Cookie", null},
                },
            };
            var response = ExecuteAndRead(request);

            return response.ToHtml();
        }

        public string GetSessionTime(string bearerToken, string expireToken)
        {
            var timestamp = GetCurrentTimeStampMs;
            var request = new HttpRequest
            {
                RequestUri = new Uri($"{MainDomain}api/Timing/GetSessionTime/?_={timestamp}"),
                RequestType = HttpRequestType.Data,
                Headers = new HttpRequestHeaders
                {
                    {"Host", null},
                    {"Connection", "keep-alive"},
                    {"Pragma", "no-cache"},
                    {"Cache-Control", "no-cache"},
                    {"ExpirationToken", expireToken},
                    {"Accept", "application/json, text/javascript, */*; q=0.01"},
                    {"X-Requested-With", "XMLHttpRequest"},
                    {"Authorization", $"Bearer {bearerToken}"},
                    {"User-Agent", UserAgent},
                    {"Content-Type", "application/json; charset=utf-8"},
                    {"Sec-Fetch-Site", "same-origin"},
                    {"Sec-Fetch-Mode", "cors"},
                    {"Referer", null},
                    {"Accept-Encoding", AcceptEncoding},
                    {"Accept-Language", AcceptLanguage},
                    {"Cookie", null},
                },
            };
            var (_, response) = ExecuteAndReadWithResponse(request);

            if (response.Headers.AllKeys.Contains("ExpirationToken")) {
                return response.Headers["ExpirationToken"];
            }

            return string.Empty;
        }
    }
}
