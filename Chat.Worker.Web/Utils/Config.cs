using System;
using System.ComponentModel;
using System.Web.Configuration;
using NLog;

namespace Chat.Worker.Web.Utils
{
    /// <summary>
    /// Config
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class Config
    {
        public string Url { get; set; }
        public string RedisConnString { get; set; }
        public string BackendUrl { get; set; }
        public string BackendApiKey { get; set; }
        public bool EnableHubDetailedErrors { get; set; }

        public bool InformBackendOnlineOffline { get; set; }

        public int CounterIntervalInSec { get; set; }

        public bool EnableScaleout { get; set; }
        public string ScaleoutHost { get; set; }
        public int ScaleoutPort { get; set; }
        public string ScaleoutPwd { get; set; }
        public bool EachEventNotifyInLiveStream { get; set; }
        public int CommentLength { get; set; }
        public int JoinedLength { get; set; }
        public int CommentPauseInSec { get; set; }
        public int JoinedPauseInSec { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Config() { }

        private static Config _instance;

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        Init();
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal(e);
                        LogManager.Flush();
                        Environment.Exit(-1);
                    }
                }
                return _instance;
            }
        }


        private static void Init()
        {
            _instance = new Config
            {
				Url = Read<string>("url", "http://*:88"),
                RedisConnString = Read<string>("redisConnString", "10.2.0.74:7000,password=pwd"),
                BackendUrl = Read<string>("backendUrl", "https://backend.com"),
                BackendApiKey = Read<string>("backendApiKey", "api_key"),
                EnableHubDetailedErrors = Read<bool>("enableHubDetailedErrors", "False"),
                InformBackendOnlineOffline = Read<bool>("informBackendOnlineOffline", "False"),

                CounterIntervalInSec = Read<int>("counterIntervalInSec", "900"),
                EachEventNotifyInLiveStream = Read<bool>("eachEventNotifyInLiveStream", "False"),
                CommentLength = Read<int>("commentLength", "3"),
                JoinedLength = Read<int>("joinedLength", "3"),
                CommentPauseInSec = Read<int>("commentPauseInSec", "1"),
                JoinedPauseInSec = Read<int>("joinedPauseInSec", "2"),

//                EnableScaleout = Read<bool>("enableScaleout", "False"),
//                ScaleoutHost = Read<string>("scaleoutHost", "8.8.8.8"),
//                ScaleoutPort = Read<int>("scaleoutPort", "7777"),
//                ScaleoutPwd = Read<string>("scaleoutPwd", "pwd")
            };
        }

        private static T Read<T>(string key, string defaultValue)
        {
            var value = WebConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception($"Пожалуйста, укажите {key} в App.config (<appSettings><add key=\"{key}\" value=\"{defaultValue}\"/></addSettings>)");

            T result;
            var convertor = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                result = (T)convertor.ConvertFromString(value);
            }
            catch
            {
                throw new Exception($"Параметр {key} (value='{value}') не является типa {typeof(T).Name}");
            }
            return result;
        }
    }
}