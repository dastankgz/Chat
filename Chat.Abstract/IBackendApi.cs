using System.Collections.Generic;
using Chat.Abstract.Model;

namespace Chat.Abstract
{
    public interface IBackendApi
    {
        ResponseModel<User> ValidateToken(IWorkerContext context, TokenModel model);

        ResponseModel<bool> SaveMessage(IWorkerContext context, Message message);

        ResponseModel<bool> MessageDelivered(IWorkerContext context, SimpleMessage message);

        ResponseModel<bool> MessageViewed(IWorkerContext context, SimpleMessage message);

        ResponseModel<bool> SetOnline(IWorkerContext context, string userId);

        ResponseModel<bool> SetOffline(IWorkerContext context, string userId);

        ResponseModel<List<Model.Chat>> GetChats(IWorkerContext context, string userId);

        ResponseModel<bool> AddToChat(IWorkerContext context, JoinToChatModel model);

        ResponseModel<bool> RemoveFromChat(IWorkerContext context, RemoveFromChatModel model);

        ResponseModel<string> CreateChat(IWorkerContext context, CreateChatModel model);
    }
}