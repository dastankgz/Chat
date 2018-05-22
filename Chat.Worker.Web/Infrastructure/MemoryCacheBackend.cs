using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Chat.Abstract;
using Chat.Abstract.Model;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// MemoryCacheBackend
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class MemoryCacheBackend : IBackendApi
    {
        private readonly IBackendApi _backend;
        private readonly MemoryCache _cache = new MemoryCache("MemoryCacheBackend");

        public MemoryCacheBackend(IBackendApi backend)
        {
            _backend = backend;
        }

        public ResponseModel<User> ValidateToken(IWorkerContext context, TokenModel model)
        {
            var key = "validate_" + model.Token;
            var response = _cache[key] as ResponseModel<User>;
            if (response == null)
            {
                response = _backend.ValidateToken(context, model);
                _cache.Add(key, response, DateTimeOffset.Now.AddMinutes(5));
            }

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
            var key = "setOnline_" + userId;
            var response = _cache[key] as ResponseModel<bool>;
            if (response == null)
            {
                response = _backend.SetOnline(context, userId);
                _cache.Add(key, response, DateTimeOffset.Now.AddMinutes(5));
            }

            return response;
        }

        public ResponseModel<bool> SetOffline(IWorkerContext context, string userId)
        {
            var key = "setOffline_" + userId;
            var response = _cache[key] as ResponseModel<bool>;
            if (response == null)
            {
                response = _backend.SetOffline(context, userId);
                _cache.Add(key, response, DateTimeOffset.Now.AddMinutes(5));
            }

            return response;
        }

        public ResponseModel<List<Abstract.Model.Chat>> GetChats(IWorkerContext context, string userId)
        {
            var key = GetChatKey(userId);
            var response = _cache[key] as ResponseModel<List<Abstract.Model.Chat>>;
            if (response == null)
            {
                response = _backend.GetChats(context, userId);
                _cache.Add(key, response, DateTimeOffset.Now.AddMinutes(5));
            }

            return response;
        }

        public ResponseModel<bool> AddToChat(IWorkerContext context, JoinToChatModel model)
        {
            var response = _backend.AddToChat(context, model);
            if (response.Success)
            {
                var key = GetChatKey(model.UserIdToJoin);
                _cache.Remove(key);
            }

            return response;
        }

        public ResponseModel<bool> RemoveFromChat(IWorkerContext context, RemoveFromChatModel model)
        {
            var response = _backend.RemoveFromChat(context, model);
            if (response.Success)
            {
                var key = GetChatKey(model.UserIdToRemove);
                _cache.Remove(key);
            }

            return response;
        }

        public ResponseModel<string> CreateChat(IWorkerContext context, CreateChatModel model)
        {
            var response= _backend.CreateChat(context, model);
            if (response.Success)
            {
                foreach (var userId in model.Users)
                {
                    var key = GetChatKey(userId);
                    _cache.Remove(key);
                }
            }

            return response;
        }

        private string GetChatKey(string userId)
        {
            return "getChats_" + userId;
        }
    }
}