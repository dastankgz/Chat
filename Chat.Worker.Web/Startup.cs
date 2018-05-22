using System;
using System.IO;
using Chat.Abstract;
using Chat.Abstract.Model;
using Chat.Worker.Web;
using Chat.Worker.Web.Hubs;
using Chat.Worker.Web.Infrastructure;
using Chat.Worker.Web.Infrastructure.Validation;
using Chat.Worker.Web.Utils;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using NLog;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace Chat.Worker.Web
{
    /// <summary>
    /// Startup
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class Startup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Configuration(IAppBuilder app)
        {
            try
            {
                var counter = CreateCounter();
                var mapper = new MemoryConnectionMapper();
                var backend = CreateBackend();
                GlobalHost.DependencyResolver.Register(typeof(WorkerHub), () => new WorkerHub(mapper, backend, counter));

                var userMapper = new MemoryConnectionMapper();
                var creatorMapper = new MemoryConnectionMapper();
                var notify = Config.Instance.EachEventNotifyInLiveStream;
                var comments = new MemoryMapper<Comment>(Config.Instance.CommentLength);
                var joined = new MemoryMapper<StreamModel>(Config.Instance.JoinedLength);
                GlobalHost.DependencyResolver.Register(typeof(LiveStreamWorker),
                    () => new LiveStreamWorker(mapper, userMapper, creatorMapper, counter, notify, comments, joined));

                var settings = new JsonSerializerSettings { ContractResolver = new SignalRContractResolver() };
                var serializer = JsonSerializer.Create(settings);
                GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

                var authorizer = new TokenAuthorizeAttribute(backend);
                var modul = new AuthorizeModule(authorizer, authorizer);
                GlobalHost.HubPipeline.AddModule(modul);
                GlobalHost.HubPipeline.AddModule(new ValidationModule());
                GlobalHost.HubPipeline.AddModule(new SignalrErrorHandler());

                var groupNotifier = new GroupNotifier(comments, joined, userMapper, 
                    Config.Instance.CommentPauseInSec,
                    Config.Instance.JoinedPauseInSec);
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                LogManager.Flush();
                Environment.Exit(-1);
            }

            var hubConf = new HubConfiguration
            {
                EnableDetailedErrors = Config.Instance.EnableHubDetailedErrors
                // EnableJavaScriptProxies = false
            };

            // if (Config.Instance.EnableScaleout)
            //    GlobalHost.DependencyResolver.UseRedis(Config.Instance.ScaleoutHost, Config.Instance.ScaleoutPort, Config.Instance.ScaleoutPwd, "Chat.Worker.Application");

            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(hubConf);
        }

        private ICounter CreateCounter()
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var path = Path.Combine(programData, "SocketChat", "Counter");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = Path.Combine(path, "Statistics.txt");
            return new DefaultCounter(Config.Instance.CounterIntervalInSec, fileName);
        }

        private IBackendApi CreateBackend()
        {
            var backend = new WebClientBackend(Config.Instance.BackendUrl, Config.Instance.BackendApiKey);
            var memoryCacheBackend = new MemoryCacheBackend(backend);

            return memoryCacheBackend;
        }
    }
}