using System.Collections.Generic;
using System.Linq;

namespace Chat.Worker.Web.Infrastructure
{
    public class MemoryMapper<T>
    {
        private readonly Dictionary<string, List<T>> _connections = new Dictionary<string, List<T>>();
        private readonly int _length;

        public MemoryMapper(int length)
        {
            _length = length;
        }

        public void Add(string key, T value)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out var connections))
                {
                    connections = new List<T>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    var exist = connections.Contains(value);
                    if (!exist)
                        connections.Add(value);
                }
            }
        }

        public void Remove(string key, T value)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out var connections))
                    return;

                lock (connections)
                {
                    var exist = connections.Contains(value);
                    if (exist)
                        connections.Remove(value);

                    if (connections.Count == 0)
                        _connections.Remove(key);
                }
            }
        }

        public void Remove(string key)
        {
            lock (_connections)
            {
                _connections.Remove(key);
            }
        }

        public IList<T> GetAll(string key)
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(key, out var connections))
                    return connections;
            }

            return Model.Empty.Items;
        }

        public IList<string> GetKeys()
        {
            lock (_connections)
                return _connections.Keys.ToList();
        }

        public Model GetNextAndClear(string key)
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(key, out var connections))
                {
                    var model = new Model
                    {
                        TotalSize = connections.Count,
                        Items = connections.Take(_length).ToList()
                    };
                    connections.Clear();
                    return model;
                }
            }

            return Model.Empty;
        }

        public bool Any(string key)
        {
            lock (_connections)
                return _connections.ContainsKey(key);
        }

        public class Model
        {
            public int TotalSize { get; set; }
            public IList<T> Items { get; set; }

            public static Model Empty = new Model {Items = new List<T>(0)};
        }
    }
}