using System.Collections.Generic;
using Chat.Abstract.Model;

namespace Chat.Abstract
{
    public interface ILiveStreamWorkerApi
    {
        bool Create(StreamSimpleModel model);
        bool Delete(StreamSimpleModel model);
        bool Like(StreamModel model);
        bool AddComment(Comment comment);
        bool AddToStream(StreamModel model);
        bool RemoveFromStream(StreamModel model);
        IEnumerable<string> GetStreamMembers(StreamSimpleModel model);
        int GetStreamMembersCount(StreamSimpleModel model);
    }
}