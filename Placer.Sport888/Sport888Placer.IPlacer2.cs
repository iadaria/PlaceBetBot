using FluentScraper.Extension;
using Logging;
using Microsoft.Extensions.Logging;
using NetworkLib462.Net;
using NetworkLib462.Net.SecureProtocols;
using Newtonsoft.Json;
using Placer.Ecambi;
using Placer.Sport888.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Placer.Sport888
{
    public class Sport888Placer: EcambiPlacer
    {
        private string _expireToken = string.Empty;
        private long _lastTimeGettingExpireTokenMs = -1;
        private BearerTokenResponse BearerToken { get; set; }
        private UserContext UserContext { get; set; }


        public Sport888Placer(string mainDomain, string market = "GB", string lang = "en_GB")
            : base(mainDomain, market, lang)
        {
            _minStake = 0.1;
            Name = "Sport888Placer";
            UserContext = new UserContext();

            InitWeb(mainDomain, market, lang);
        }

        public override void Login()
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                if (LoadSessionAndInit(_sessionFile))
                {
                    return;
                }
                _web.Login(UserName, Password);

                if (!IsSuccessInitData())
                {
                    Logger.LogDebug("Login failed!");
                    throw new AuthenticationException($"Login failed");
                }

                Logger.LogDebug("Login completed");

                StartRefreshToken();
            }
        }

        public override double GetBalance()
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    var isLogged = IsSuccessInitData();

                    if (!isLogged)
                    {
                        Login();
                    }

                    return (double)UserContext.userBalance.ExtractDecimal();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller()} failed");
                    throw ex.PreserveStackTrace();
                }
            }
        }

        /*Token с ограниченным временем действия */
        public string ExpireToken
        {
            get
            {
                if ( _expireToken == "" && 
                     _web.GetCurrentTimeStampMs > HowLongWorkedMs)

                    throw new Exception("ExpireToken was not updated");

                return _expireToken;
            }
            set
            {
                _expireToken = value;
                _lastTimeGettingExpireTokenMs = _web.GetCurrentTimeStampMs;
            }
        }
        // Вычисляем сколько прошло временни с последнего получения ExpireToken
        private long HowLongWorkedMs => 
            _lastTimeGettingExpireTokenMs + BearerToken.RefreshTokenexperationTimeSpanInMS;
        protected override void CreateWeb(string userAgent, string language, string encoding, IWebProxy proxy, string mainDomain, string market, string lang)
        {
            _web = new Sport888Web(userAgent, language, encoding, proxy, mainDomain, market, lang)
            {
                LogSettings = { Enabled = true }
            };
        }

        /* Обращаемся к главной странице и получяем все данные для работы*/
        protected override bool IsSuccessInitData()
        {
            var mainPage = ((Sport888Web)_web).GetMainPage();

            UserContext = ParseUserContext(mainPage.Text);
            _isLogged = UserContext.isAuthenticated;
            if (UserContext.isAuthenticated == false)
                return false;

            BearerToken = ParseBearer(mainPage.Text);
            ExpireToken = BearerToken.RefreshToken;
            Token = ParseSdkToken(mainPage.Text);
            if (UserContext.isAuthenticated && !String.IsNullOrEmpty(_token))
            {
                SessionToken = GetSessionId(Token);
            }      

            return
                UserContext.isAuthenticated && 
                !String.IsNullOrEmpty(_token) && 
                !String.IsNullOrEmpty(BearerToken.BearerToken);
        }

        private UserContext ParseUserContext(string mainPage)
        {
            var userInfoStr = mainPage.Match("({\"userContext\":{.+}})").Groups[1].Value;
            return userInfoStr.ToJson()["userContext"].ToObject<UserContext>();
        }

        private BearerTokenResponse ParseBearer(string mainPage)
        {
            var bearerTokenStr = mainPage.Match("({\"BearerToken\":.+})")?.Groups[1].Value;
            return JsonConvert.DeserializeObject<BearerTokenResponse>(bearerTokenStr);
        }

        private string ParseSdkToken(string mainPage) =>
            Regex.Match(mainPage, "Kambi=\"(Token_[A-Za-z0-9+-_=]+)\";")?.Groups[1].Value;

        /* Получаем SessionToken */
        protected override string GetSessionId(string token)
        {
            _web.GetSessionIdOption();
            var sessionToken = _web.GetSessionId(token);

            return sessionToken["sessionId"].Val();
        }

        private async void StartRefreshToken()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(300000);
                    Logger.LogDebug(ExpireToken);
                    RefreshToken();
                }
            });
        }
        /* Получение нового ExpireToken-а */
        private void RefreshToken()
        {
            var newExpaireToken = ((Sport888Web)_web).GetSessionTime(BearerToken.BearerToken, ExpireToken);
            ExpireToken = newExpaireToken;
        }
    }
}