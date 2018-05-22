using System.Collections.Generic;
using Chat.Abstract;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// MemoryConnectionMapper
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class MemoryConnectionMapper : IConnectionMapper<string>
    {
        private readonly Dictionary<string, List<string>> _connections =
            new Dictionary<string, List<string>>();

        private readonly IList<string> _empty = new List<string>(0);

        public long GetCount()
        {
            return _connections.Count;
        }

        public void Add(string key, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out var connections))
                {
                    connections = new List<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    var exist = connections.Contains(connectionId);
                    if (!exist)
                    {
                        connections.Add(connectionId);
                    }
                }
            }
        }

        public void Remove(string key, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out var connections))
                    return;

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                        _connections.Remove(key);
                }
            }
        }

        public void Remove(string key)
        {
            _connections.Remove(key);
        }

        public IList<string> GetConnections(string key)
        {
            if (_connections.TryGetValue(key, out var connections))
                return connections;

            return _empty;
        }

        public bool Any(string key)
        {
            return  _connections.ContainsKey(key);
        }
    }
}