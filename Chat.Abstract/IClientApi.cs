using Chat.Abstract.Model;

namespace Chat.Abstract
{
    public interface IClientApi
    {
        void NewMessage(Message message);
        void MessageDelivered(SimpleMessage message);
        void MessageViewed(SimpleMessage message);
        void UserJoinedToChat(JoinToChatModel model);
        void UserLeftChat(RemoveFromChatModel model);
        void UserOnline(string userId);
        void UserOffline(string userId);
    }
}