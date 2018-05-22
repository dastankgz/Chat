using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat.Abstract;
using Chat.Abstract.Model;
using Chat.Worker.Web.Infrastructure;
using Chat.Worker.Web.Utils;
using Microsoft.AspNet.SignalR;
using NLog;

namespace Chat.Worker.Web.Hubs
{
    /// <summary>
    /// WorkerHub
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class WorkerHub : Hub<IClientApi>, IWorkerApi
    {
        private readonly IConnectionMapper<string> _mapper;
        private readonly IBackendApi _backend;
        private readonly ICounter _counter;
        
        private string CurUserId => Context.Request.Environment[Param.UserId] as string;

        private WorkerContext WorkerContext =>
            WorkerContext.Build(Context.Request.Environment[Param.UserToken] as string);

        private IList<string> CurUserConnections => _mapper.GetConnections(CurUserId);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string LogKey => $"[UserId: {CurUserId}; ConId: {Context.ConnectionId}] ";

        public WorkerHub(IConnectionMapper<string> mapper, IBackendApi backend, ICounter counter)
        {
            _mapper = mapper;
            _backend = backend;
            _counter = counter;
        }

        #region Connect/Disconnect

        private void InitChatsAndInform(bool isOnline)
        {
            var resp = _backend.GetChats(WorkerContext, CurUserId);

            if (resp.Success && resp.Body != null)
            {
                foreach (var chat in resp.Body)
                {
                    if (string.IsNullOrWhiteSpace(chat.ChatId))
                    {
                        Logger.Warn("chat.ChatId==null");
                        continue;
                    }

                    if (!isOnline)
                        Clients.Group(chat.ChatId).UserOnline(CurUserId);

                    Groups.Add(Context.ConnectionId, chat.ChatId);
                }
            }
        }

        private void InformOffline()
        {
            var resp = _backend.GetChats(WorkerContext, CurUserId);
            
            if (resp.Success && resp.Body != null)
            {
                foreach (var chat in resp.Body)
                {
                    if (string.IsNullOrWhiteSpace(chat.ChatId))
                    {
                        Logger.Warn("chat.ChatId==null");
                        continue;
                    }
                    Clients.Group(chat.ChatId).UserOffline(CurUserId);
                }
            }
        }

        public override Task OnConnected()
        {
            try
            {
                var isOnline = _mapper.Any(CurUserId);
                _mapper.Add(CurUserId, Context.ConnectionId);

                InitChatsAndInform(isOnline);

                if (Config.Instance.InformBackendOnlineOffline)
                    Task.Run(() => _backend.SetOnline(WorkerContext, CurUserId));

                Logger.Debug(LogKey);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                _mapper.Remove(CurUserId, Context.ConnectionId);

                if (!_mapper.Any(CurUserId))
                {
                    InformOffline();

                    if (Config.Instance.InformBackendOnlineOffline)
                        Task.Run(() => _backend.SetOffline(WorkerContext, CurUserId));
                }
                Logger.Debug(LogKey);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Logger.Debug(LogKey);
            return base.OnReconnected();
        }
        
        #endregion

        public Message SendMessage(Message message)
        {
            try
            {
                message.CreateDate = DateTime.Now;
                var result = _backend.SaveMessage(WorkerContext, message);

                if (result.Success)
                {
                    Clients.OthersInGroup(message.ChatId).NewMessage(message);

                    _counter.Success();
                    Logger.Debug(LogKey + message);
                    return message;
                }

                _counter.Fail();
                Logger.Warn(result + LogKey + message);
            }
            catch (Exception e) 
            {
                _counter.Fail();
                Logger.Error(e, LogKey + message);
            }

            return null;
        }

        public bool MessageDelivered(SimpleMessage message)
        {
            try
            {
                message.UserId = CurUserId;
                var result = _backend.MessageDelivered(WorkerContext, message);

                if (result.Success)
                {
                    Clients.OthersInGroup(message.ChatId).MessageDelivered(message);
                    Logger.Debug(LogKey + message);
                    return true;
                }
                Logger.Warn(result + LogKey + message);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return false;
        }

        public bool MessageViewed(SimpleMessage message)
        {
            try
            {
                message.UserId = CurUserId;
                var result = _backend.MessageViewed(WorkerContext, message);

                if (result.Success)
                {
                    Clients.OthersInGroup(message.ChatId).MessageViewed(message);
                    Logger.Debug(LogKey + message);
                    return true;
                }

                Logger.Warn(result + LogKey + message);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return false;
        }

        public bool JoinChat(JoinToChatModel model)
        {
            try
            {
                model.UserId = CurUserId;
                var resp = _backend.AddToChat(WorkerContext, model);
                if (resp.Success)
                {
                    AddToChat(model.UserIdToJoin, model.ChatId);
                    Clients.OthersInGroup(model.ChatId).UserJoinedToChat(model);
                    Logger.Debug(LogKey + model);
                    return true;
                }

                Logger.Warn(resp + LogKey + model);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return false;
        }

        public bool LeaveChat(RemoveFromChatModel model)
        {
            try
            {
                model.UserId = CurUserId;
                var resp = _backend.RemoveFromChat(WorkerContext, model);
                if (resp.Success)
                {
                    RemoveFromChat(model.UserIdToRemove, model.ChatId);
                    Clients.Group(model.ChatId).UserLeftChat(model);

                    var conns = _mapper.GetConnections(model.UserIdToRemove);
                    Clients.Clients(conns).UserLeftChat(model);
                    Logger.Debug(LogKey + model);

                    return true;
                }
                Logger.Warn(resp + LogKey + model);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return false;
        }

        public string CreateChat(CreateChatModel model)
        {
            try
            {
                var response = _backend.CreateChat(WorkerContext, model);
                if (response.Success)
                {
                    var chatId = response.Body;
                    foreach (var user in model.Users)
                        AddToChat(user, chatId);

                    Logger.Debug(LogKey + model);
                    return chatId;
                }

                Logger.Warn(response + LogKey + model);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public bool IsUserOnline(string userId)
        {
            try
            {
                Logger.Debug(LogKey + "userId: " + userId);
                return _mapper.Any(userId);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public string Ping(string value)
        {
            Logger.Debug(LogKey + value);

            return "Ping - " + value;
        }

        private void AddToChat(string userId, string chatId)
        {
            foreach (var connection in _mapper.GetConnections(userId))
            {
                Groups.Add(connection, chatId);
            }
        }

        private void RemoveFromChat(string userId, string chatId)
        {
            foreach (var connection in _mapper.GetConnections(userId))
            {
                try
                {
                    Groups.Remove(connection, chatId);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }
}