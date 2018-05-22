using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat.Abstract;
using Chat.Abstract.Model;
using Chat.Worker.Web.Infrastructure;
using Chat.Worker.Web.Infrastructure.Validation;
using Chat.Worker.Web.Utils;
using Microsoft.AspNet.SignalR;
using NLog;

namespace Chat.Worker.Web.Hubs
{
    /// <summary>
    /// LiveStreamWorker
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class LiveStreamWorker : Hub<ILiveStreamClientApi>, ILiveStreamWorkerApi
    {
        private readonly IConnectionMapper<string> _connectionMapper;
        private readonly IConnectionMapper<string> _streamUserMapper;
        private readonly IConnectionMapper<string> _streamCreatorMapper;

        private readonly MemoryMapper<Comment> _comments;
        private readonly MemoryMapper<StreamModel> _joined;

        private readonly ICounter _counter;

        private readonly bool _eachEventNotify;

        private string CurUserId => Context.Request.Environment[Param.UserId] as string;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string LogKey => $"[UserId: {CurUserId}; ConId: {Context.ConnectionId}] ";

        public LiveStreamWorker(IConnectionMapper<string> mapper,
            IConnectionMapper<string> streamUserMapper,
            IConnectionMapper<string> streamCreatorMapper, ICounter counter, 
            bool eachEventNotify, 
            MemoryMapper<Comment> comments, MemoryMapper<StreamModel> joined)
        {
            _connectionMapper = mapper;
            _streamUserMapper = streamUserMapper;
            _streamCreatorMapper = streamCreatorMapper;
            _counter = counter;
            _eachEventNotify = eachEventNotify;
            _comments = comments;
            _joined = joined;
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var streams = _streamCreatorMapper.GetConnections(Context.ConnectionId);
                _streamCreatorMapper.Remove(Context.ConnectionId);
                if (streams != null)
                {
                    foreach (var stream in streams)
                    {
                        var model = new StreamSimpleModel { StreamId = stream };
                        _streamUserMapper.Remove(model.StreamId);
                        Clients.OthersInGroup(model.StreamId).Deleted(model);
                        _comments.Remove(model.StreamId);
                        _joined.Remove(model.StreamId);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return base.OnDisconnected(stopCalled);
        }

        [Validate]
        public bool Create(StreamSimpleModel model)
        {
            try
            {
                var any = _streamUserMapper.Any(model.StreamId);
                if (any)
                {
                    Logger.Warn("Stream exist!" + model);
                    return false;
                }

                foreach (var connection in _connectionMapper.GetConnections(CurUserId))
                    Groups.Add(connection, model.StreamId);

                _streamUserMapper.Add(model.StreamId, CurUserId);
                _streamCreatorMapper.Add(Context.ConnectionId, model.StreamId);

                Logger.Debug(LogKey + model);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        [Validate]
        public bool Delete(StreamSimpleModel model)
        {
            try
            {
                var any = _streamUserMapper.Any(model.StreamId);
                if (!any)
                {
                    Logger.Warn("Stream not exist!" + model);
                    return false;
                }

                _streamUserMapper.Remove(model.StreamId);
                Clients.OthersInGroup(model.StreamId).Deleted(model);
                Logger.Debug(LogKey + model);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        [Validate]
        public bool Like(StreamModel model)
        {
            try
            {
                var any = _streamUserMapper.Any(model.StreamId);
                if (!any)
                {
                    Logger.Warn("Stream not exist!" + model);
                    return false;
                }

                Clients.OthersInGroup(model.StreamId).UserLiked(model);
                Logger.Debug(LogKey + model);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        [Validate]
        public bool AddComment(Comment comment)
        {
            try
            {
                var any = _streamUserMapper.Any(comment.StreamId);
                if (!any)
                {
                    _counter.Fail();
                    Logger.Warn("Stream not exist!" + comment);
                    return false;
                }

                if (_eachEventNotify)
                    Clients.OthersInGroup(comment.StreamId).NewComment(comment);

                _comments.Add(comment.StreamId, comment);

                _counter.Success();
                Logger.Debug(LogKey + comment);
                return true;
            }
            catch (Exception e)
            {
                _counter.Fail();
                Logger.Error(e);
                return false;
            }
        }

        [Validate]
        public bool AddToStream(StreamModel model)
        {
            try
            {
                var any = _streamUserMapper.Any(model.StreamId);
                if (!any)
                {
                    Logger.Warn("Stream not exist!" + model);
                    return false;
                }

                foreach (var connection in _connectionMapper.GetConnections(model.User.UserId))
                    Groups.Add(connection, model.StreamId);

                _streamUserMapper.Add(model.StreamId, model.User.UserId);
                model.MembersCount = GetMembersCount(model.StreamId);

                if (_eachEventNotify)
                    Clients.OthersInGroup(model.StreamId).UserJoined(model);

                _joined.Add(model.StreamId, model);

                Logger.Debug(LogKey + model);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        [Validate]
        public bool RemoveFromStream(StreamModel model)
        {
            try
            {
                var any = _streamUserMapper.Any(model.StreamId);
                if (!any)
                {
                    Logger.Warn("Stream not exist!" + model);
                    return false;
                }

                foreach (var connection in _connectionMapper.GetConnections(model.User.UserId))
                    Groups.Remove(connection, model.StreamId);

                _streamUserMapper.Remove(model.StreamId, model.User.UserId);
                model.MembersCount = GetMembersCount(model.StreamId);
                Clients.OthersInGroup(model.StreamId).UserLeft(model);
                _joined.Remove(model.StreamId, model);

                Logger.Debug(LogKey + model);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        [Validate]
        public IEnumerable<string> GetStreamMembers(StreamSimpleModel model)
        {
            try
            {
                var any = _streamUserMapper.Any(model.StreamId);
                if (!any)
                {
                    Logger.Warn("Stream not exist!" + model);
                    return null;
                }

                Logger.Debug(LogKey + model);
                return _streamUserMapper.GetConnections(model.StreamId);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        [Validate]
        public int GetStreamMembersCount(StreamSimpleModel model)
        {
            try
            {
                var any = _streamUserMapper.Any(model.StreamId);
                if (!any)
                {
                    Logger.Warn("Stream not exist!" + model);
                    return 0;
                }

                Logger.Debug(LogKey + model);
                return GetMembersCount(model.StreamId);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return 0;
            }
        }

        [Validate]
        public bool IsStreamExist(StreamSimpleModel model)
        {
            try
            {
                Logger.Debug(LogKey + model);
                return _streamUserMapper.Any(model.StreamId);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        private int GetMembersCount(string streamId)
        {
            Logger.Debug(LogKey + " streamId: " + streamId);
            return _streamUserMapper.GetConnections(streamId).Count;
        }
    }
}