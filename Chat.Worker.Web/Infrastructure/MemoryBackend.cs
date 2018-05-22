using System;
using System.Collections.Generic;
using Chat.Abstract;
using Chat.Abstract.Model;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// MemoryBackend
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class MemoryBackend : IBackendApi
    {
        public ResponseModel<User> ValidateToken(IWorkerContext context, TokenModel model)
        {
            return ResponseModel<User>.Ok(new User { UserId = model.Token });
        }

        public ResponseModel<bool> SaveMessage(IWorkerContext context, Message message)
        {
            return ResponseModel<bool>.Ok(true);
        }

        public ResponseModel<bool> MessageDelivered(IWorkerContext context, SimpleMessage message)
        {
            return ResponseModel<bool>.Ok(true);
        }

        public ResponseModel<bool> MessageViewed(IWorkerContext context, SimpleMessage message)
        {
            return ResponseModel<bool>.Ok(true);
        }

        public ResponseModel<bool> SetOnline(IWorkerContext context, string userId)
        {
            return ResponseModel<bool>.Ok(true);
        }

        public ResponseModel<bool> SetOffline(IWorkerContext context, string userId)
        {
            return ResponseModel<bool>.Ok(true);
        }

        public ResponseModel<List<Abstract.Model.Chat>> GetChats(IWorkerContext context, string userId)
        {
            return ResponseModel<List<Abstract.Model.Chat>>.Ok(new List<Abstract.Model.Chat>());
        }

        public ResponseModel<bool> AddToChat(IWorkerContext context, JoinToChatModel model)
        {
            return ResponseModel<bool>.Ok(true);
        }

        public ResponseModel<bool> RemoveFromChat(IWorkerContext context, RemoveFromChatModel model)
        {
            return ResponseModel<bool>.Ok(true);
        }

        public ResponseModel<string> CreateChat(IWorkerContext context, CreateChatModel model)
        {
            return ResponseModel<string>.Ok(Guid.NewGuid().ToString());
        }
    }
}