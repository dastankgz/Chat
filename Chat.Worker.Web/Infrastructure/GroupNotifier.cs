using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chat.Abstract;
using Chat.Abstract.Model;
using Chat.Worker.Web.Hubs;
using Microsoft.AspNet.SignalR;

namespace Chat.Worker.Web.Infrastructure
{
    public class GroupNotifier
    {
        private readonly MemoryMapper<Comment> _comments;
        private readonly MemoryMapper<StreamModel> _joined;
        private readonly IConnectionMapper<string> _streamUserMapper;
        private readonly IHubContext _context = GlobalHost.ConnectionManager.GetHubContext<LiveStreamWorker>();
        private readonly int _commentPauseInSec;
        private readonly int _joinedPauseInSec;
        private int _startPause = 1000 * 60 * 2;

        public GroupNotifier(MemoryMapper<Comment> comments, MemoryMapper<StreamModel> joined, IConnectionMapper<string> streamUserMapper, int commentPauseInSec, int joinedPauseInSec)
        {
            _comments = comments;
            _joined = joined;
            _streamUserMapper = streamUserMapper;
            _commentPauseInSec = commentPauseInSec;
            _joinedPauseInSec = joinedPauseInSec;

            Task.Factory.StartNew(NotifyComment);
            Task.Factory.StartNew(NotifyJoined);
        }

        private void NotifyComment()
        {
            Thread.Sleep(_startPause);
            while (true)
            {
                foreach (var streamId in _comments.GetKeys())
                {
                    var model = _comments.GetNextAndClear(streamId);
                    if (model.TotalSize > 0)
                        _context.Clients.Group(streamId).NewComments(model.Items.ToArray());
                }

                Thread.Sleep(1000*_commentPauseInSec);
            }
        }

        private void NotifyJoined()
        {
            Thread.Sleep(_startPause);
            while (true)
            {
                foreach (var streamId in _joined.GetKeys())
                {
                    var model = _joined.GetNextAndClear(streamId);
                    if (model.TotalSize > 0)
                    {
                        var joined = new UsersJoinedModel
                        {
                            StreamId = streamId,
                            AllMembersCount = _streamUserMapper.GetConnections(streamId).Count,
                            MembersCount = model.TotalSize,
                            Users = model.Items.Select(x => x.User).ToArray()
                        };
                        _context.Clients.Group(streamId).UsersJoined(joined);
                    }
                }

                Thread.Sleep(1000*_joinedPauseInSec);
            }
        }
    }
}