using System.Collections.Generic;

namespace Chat.Abstract
{
    public interface IConnectionMapper<T>
    {
        long GetCount();
        void Add(T key, string connectionId);
        void Remove(T key, string connectionId);
        void Remove(T key);
        IList<string> GetConnections(T key);
        bool Any(T key);
    }
}