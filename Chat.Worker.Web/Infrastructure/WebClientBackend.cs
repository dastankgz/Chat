using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Chat.Abstract;
using Chat.Abstract.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// WebClientBackend
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class WebClientBackend : IBackendApi
    {
        #region WebClient

        private readonly string _uri;
        private readonly string _apiKey;
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public WebClientBackend(string uri, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            _uri = uri.Trim();
            _apiKey = apiKey.Trim();
        }

        private ResponseModel<T> Request<T>(IWorkerContext context, string action, object model, bool post = true)
        {
            var url = _uri.EndsWith("/") ? _uri + action : _uri + "/" + action;
            var json = model != null ? JsonConvert.SerializeObject(model, _settings) : "";
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                using (var client = new WebClient { Encoding = Encoding.UTF8 })
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers["api_key"] = _apiKey;
                    client.Headers["auth"] = context.CurUserToken;

                    var response = post ? client.UploadString(url, json) : client.DownloadString(url);

                    var obj = JsonConvert.DeserializeObject<ResponseModel<T>>(response);

                    if (obj.Success && obj.Body == null)
                        obj.Success = false;

                    return obj;
                }
            }
            catch (WebException e)
            {
                Logger.Error(e, $"[url: {url}; model: {json}; curUserToken: {context.CurUserToken}]");
                return ResponseModel<T>.Fail();
            }
            finally
            {
                watch.Stop();
                Logger.Trace($"[seconds: {watch.Elapsed.TotalSeconds};url: {url}; model: {json}]");
            }
        }

        #endregion

        public ResponseModel<bool> AddToChat(IWorkerContext context, JoinToChatModel model)
        {
            return Request<bool>(context, "addToChat", model);
        }

        public ResponseModel<string> CreateChat(IWorkerContext context, CreateChatModel model)
        {
            return Request<string>(context, "createChat", model);
        }

        public ResponseModel<List<Abstract.Model.Chat>> GetChats(IWorkerContext context, string userId)
        {
            return Request<List<Abstract.Model.Chat>>(context, "getChats", null, false);
        }

        public ResponseModel<bool> MessageDelivered(IWorkerContext context, SimpleMessage message)
        {
            return Request<bool>(context, "messageDelivered", message);
        }

        public ResponseModel<bool> MessageViewed(IWorkerContext context, SimpleMessage message)
        {
            return Request<bool>(context, "messageViewed", message);
        }

        public ResponseModel<bool> RemoveFromChat(IWorkerContext context, RemoveFromChatModel model)
        {
            return Request<bool>(context, "removeFromChat", model);
        }

        public ResponseModel<bool> SaveMessage(IWorkerContext context, Message message)
        {
            return Request<bool>(context, "saveMessage", message);
        }

        public ResponseModel<bool> SetOffline(IWorkerContext context, string userId)
        {
            return Request<bool>(context, "setOffline", userId);
        }

        public ResponseModel<bool> SetOnline(IWorkerContext context, string userId)
        {
            return Request<bool>(context, "setOnline", userId);
        }

        public ResponseModel<User> ValidateToken(IWorkerContext context, TokenModel model)
        {
            return Request<User>(context, "validateToken", model);
        }
    }
}