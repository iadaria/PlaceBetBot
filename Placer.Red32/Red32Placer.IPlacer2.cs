using FluentScraper.Extension;
using Logging;
using Microsoft.Extensions.Logging;
using NetworkLib462.Net;
using NetworkLib462.Net.SecureProtocols;
using Placer.Ecambi;
using System;
using System.IO;

namespace Placer.Red32
{
    public class Red32Placer : EcambiPlacer
    {
        private string _fingerPrintHash; 
        private string _fileName;
        public string FirstName { get; set; }
        public Red32Placer(string mainDomain, string firstName, string market = "GB", string lang = "en_GB")
            : base(mainDomain, market, lang)
        {
            _minStake = 0.1;
            _fileName = "fingerPrintHash.txt";
            _fingerPrintHash = getFingerPrintHash() ?? "f8f237bbda40ec3672b9ce488ef48833";
            FirstName = firstName;
            Name = "Red32Placer";

            InitWeb(mainDomain, market, lang);
        }

        public override void Login()
        {
            using (Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                if (LoadSessionAndInit(_sessionFile))
                {
                    _isLogged = true;
                    return;
                }

                var logged = _web.Login(UserName, Password, FirstName);
                _isLogged = (bool)logged["ok"];

                if (!_isLogged || !IsSuccessInitData())
                {
                    Logger.LogDebug("Login failed!");
                    throw new AuthenticationException($"Login failed");
                }

                Logger.LogDebug("Login completed");
            }
        }
        public override double GetBalance()
        {
            using(Logger.BeginScope(LoggerHelper.GetCaller()))
            {
                try
                {
                    var isSuccessBalance = IsSuccessBalance(out decimal balance);

                    if (!isSuccessBalance)
                    {
                        Login();
                        if (!IsSuccessBalance(out balance))
                            throw new Exception("Error login");
                    }
                    

                    return (double)balance;
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, $"{ LoggerHelper.GetCaller()} failed");
                    throw ex.PreserveStackTrace();
                }
            }
        }
        /* fingerprinthash - используется для анонимной идентификации браузера, отрпавляется
         * в запросе на получение sessionId
         * Для генерации fingerprinthash используется специальная библиотека Fingerprintjs2, которая
           на основе данных браузера(имени браузера, часовой пояс, язык, разрешение экрана и т.д.) генерирует hash
           Функция getFingerPrintHash() считывает fingerprinthash из файла куда он записывается при запуске generateFingerPrintHash/index.html
           либо присваивается по умолчанию
           _fingerPrintHash = getFingerPrintHash() ?? "f8f237bbda40ec3672b9ce488ef48833"*/
        private string getFingerPrintHash()
        {
            if (!File.Exists(_fileName))
                return null;

            using (var sr = new StreamReader(_fileName))
            {
                return sr.ReadLine();
            }
        }
        protected override void CreateWeb(string userAgent, string language, string encoding, IWebProxy proxy, string mainDomain, string market, string lang)
        {
            _web = new Red32Web(userAgent, language, encoding, proxy, mainDomain, market, lang)
            {
                LogSettings = { Enabled = true }
            };
        }

        protected override string GetSessionId(string token) =>
            _web.GetSessionId(Token, _fingerPrintHash, UserName)?["sessionId"].Val();

        //true если запрос успешно выполнен, т.е. пользователь успешно залогинился
        protected override bool IsSuccessInitData()
        {
            Token = ((Red32Web)_web).GetCredentials()?["ticket"].Val();
            if (Token.ToLower() == "false")
                return false;

            SessionToken = GetSessionId(Token);

            return
                !String.IsNullOrEmpty(_token) &&
                Token.ToLower() != "false" &&
                !String.IsNullOrEmpty(_sessionToken);
        }
        //true если запрос успешно выполнен, т.е. пользователь успешно залогинился
        private bool IsSuccessBalance(out decimal balance)
        {
            var status = ((Red32Web)_web).GetStatus();
            balance = (decimal?)status["playerInfo"]?["balance"]["Real Balance"] ?? 0m;

            return (bool)status["ok"];
        }
    }
}
