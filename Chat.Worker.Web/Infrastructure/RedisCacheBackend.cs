using System;
using System.Collections.Generic;
using Chat.Abstract;
using Chat.Abstract.Model;
using Newtonsoft.Json;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// RedisCacheBackend
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class RedisCacheBackend : IBackendApi
    {
        private readonly IBackendApi _backend;
        private readonly IConnectionMultiplexer _redis;
        private readonly string _keyPrefix = "RedisCacheBackend_";

        private readonly int _validateTokenExpiryInMin;
        private readonly int _setOnlineExpiryInMin;
        private readonly int _setOfflineExpiryInMin;
        private readonly int _chatsExpiryInMin;

        public RedisCacheBackend(IBackendApi backend, IConnectionMultiplexer multiplexer, int validateTokenExpiryInMin,
            int setOnlineExpiryInMin, int setOfflineExpiryInMin, int chatsExpiryInMin)
        {
            _validateTokenExpiryInMin = validateTokenExpiryInMin;
            _setOnlineExpiryInMin = setOnlineExpiryInMin;
            _setOfflineExpiryInMin = setOfflineExpiryInMin;
            _chatsExpiryInMin = chatsExpiryInMin;
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            _redis = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
        }

        private string Pref(string key)
        {
            return _keyPrefix + key;
        }

        public ResponseModel<User> ValidateToken(IWorkerContext context, TokenModel model)
        {
            var key = Pref("ValidateToken_" + model.Token);
            var userId = _redis.GetDatabase().StringGet(key);
            if (userId.HasValue)
                return ResponseModel<User>.Ok(new User {UserId = userId});

            var response = _backend.ValidateToken(context, model);

            if (response.Success)
                _redis.GetDatabase()
                      .StringSet(key, response.Body.UserId, TimeSpan.FromMinutes(_validateTokenExpiryInMin));

            return response;
        }

        public ResponseModel<bool> SaveMessage(IWorkerContext context, Message message)
        {
            return _backend.SaveMessage(context, message);
        }

        public ResponseModel<bool> MessageDelivered(IWorkerContext context, SimpleMessage message)
        {
            return _backend.MessageDelivered(context, message);
        }

        public ResponseModel<bool> MessageViewed(IWorkerContext context, SimpleMessage message)
        {
            return _backend.MessageViewed(context, message);
        }

        public ResponseModel<bool> SetOnline(IWorkerContext context, string userId)
        {
            var key = Pref("SetOnline_" + userId);
            var exist = _redis.GetDatabase().KeyExists(key);
            if (exist)
                return ResponseModel<bool>.Ok(true);

            _redis.GetDatabase().StringSet(key, "True", TimeSpan.FromMinutes(_setOnlineExpiryInMin), When.Always, CommandFlags.FireAndForget);
            return _backend.SetOnline(context, userId);
        }

        public ResponseModel<bool> SetOffline(IWorkerContext context, string userId)
        {
            var key = Pref("SetOffline_" + userId);
            var exist = _redis.GetDatabase().KeyExists(key);
            if (exist)
                return ResponseModel<bool>.Ok(true);

            _redis.GetDatabase().StringSet(key, "True", TimeSpan.FromMinutes(_setOfflineExpiryInMin), When.Always, CommandFlags.FireAndForget);
            return _backend.SetOffline(context, userId);
        }

        public ResponseModel<List<Abstract.Model.Chat>> GetChats(IWorkerContext context, string userId)
        {
            var key = Pref("GetChats_" + userId);
            var json = _redis.GetDatabase().StringGet(key);
            if (json.HasValue)
            {
                var model = JsonConvert.DeserializeObject<ResponseModel<List<Abstract.Model.Chat>>>(json);
                if (model != null)
                    return model;
            }

            var resp = _backend.GetChats(context, userId);
            if (resp.Success)
            {
                var js = JsonConvert.SerializeObject(resp);
                _redis.GetDatabase().StringSet(key, js, TimeSpan.FromMinutes(_chatsExpiryInMin));
            }

            return resp;
        }

        public ResponseModel<bool> AddToChat(IWorkerContext context, JoinToChatModel model)
        {
            var response = _backend.AddToChat(context, model);

            if (response.Success)
            {
                var key = Pref("GetChats_" + model.UserIdToJoin);
                _redis.GetDatabase().KeyDelete(key, CommandFlags.FireAndForget);
            }

            return response;
        }

        public ResponseModel<bool> RemoveFromChat(IWorkerContext context, RemoveFromChatModel model)
        {
            var response = _backend.RemoveFromChat(context, model);

            if (response.Success)
            {
                var key = Pref("GetChats_" + model.UserIdToRemove);
                _redis.GetDatabase().KeyDelete(key, CommandFlags.FireAndForget);
            }

            return response;
        }

        public ResponseModel<string> CreateChat(IWorkerContext context, CreateChatModel model)
        {
            var response = _backend.CreateChat(context, model);

            if (response.Success)
            {
                foreach (var userId in model.Users)
                {
                    var key = Pref("GetChats_" + userId);
                    _redis.GetDatabase().KeyDelete(key, CommandFlags.FireAndForget);
                }
            }

            return response;
        }
    }
}