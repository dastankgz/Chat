using Chat.Abstract.Model;

namespace Chat.Abstract
{
    public interface IWorkerApi
    {
        Message SendMessage(Message message);
        bool MessageDelivered(SimpleMessage message);
        bool MessageViewed(SimpleMessage message);
        bool JoinChat(JoinToChatModel model);
        bool LeaveChat(RemoveFromChatModel model);
        string CreateChat(CreateChatModel model);
        bool IsUserOnline(string userId);
        string Ping(string value);
    }
}