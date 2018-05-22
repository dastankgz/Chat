using System;
using System.Collections.Generic;
using Chat.Abstract;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// RedisConnectionMapper
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class RedisConnectionMapper : IConnectionMapper<string>
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly string _allKey;
        private readonly string _keyPrefix;

        public RedisConnectionMapper(string key, IConnectionMultiplexer multiplexer)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _allKey = key + "_RedisConnectionMapper_all";
            _keyPrefix = key + "_RedisConnectionMapper_";
            _redis = multiplexer;
        }

        private string Pref(string key)
        {
            return _keyPrefix + key;
        }

        public long GetCount()
        {
            return _redis.GetDatabase().SetLength(_allKey);
        }

        public void Add(string key, string connectionId)
        {
            key = Pref(key);
            _redis.GetDatabase().SetAdd(key, connectionId);
            _redis.GetDatabase().SetAdd(_allKey, key, CommandFlags.FireAndForget);
        }

        public void Remove(string key, string connectionId)
        {
            key = Pref(key);
            _redis.GetDatabase().SetRemove(key, connectionId);
            _redis.GetDatabase().SetRemove(_allKey, key, CommandFlags.FireAndForget);
        }

        public void Remove(string key)
        {
            key = Pref(key);
            _redis.GetDatabase().KeyDelete(key);
        }

        public IList<string> GetConnections(string key)
        {
            key = Pref(key);
            return _redis.GetDatabase().SetMembers(key).ToStringArray();
        }

        public bool Any(string key)
        {
            key = Pref(key);
            return _redis.GetDatabase().KeyExists(key);
        }
    }
}