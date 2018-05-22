using Chat.Abstract.Model;

namespace Chat.Abstract
{
    public interface ILiveStreamClientApi
    {
        void NewComment(Comment comment);
        void UserJoined(StreamModel model);
        void UserLeft(StreamModel model);
        void UserLiked(StreamModel model);
        void Deleted(StreamSimpleModel model);
        void NewComments(Comment[] comments);
        void UsersJoined(UsersJoinedModel model);
    }
}