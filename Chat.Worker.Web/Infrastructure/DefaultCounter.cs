using System;
using System.IO;
using System.Timers;
using Chat.Abstract;
using NLog;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// DefaultCounter
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class DefaultCounter : ICounter
    {
        private object _obj = new object();
        private int _count;
        private int _fail;

        private readonly Timer _timer;
        private readonly string _path;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DefaultCounter(int intervalInSec, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception(nameof(path));

            _path = path;

            _timer = new Timer(1000 * intervalInSec);
            _timer.Elapsed += Flush;
            _timer.Start();
        }

        private void Flush(object sender, ElapsedEventArgs e)
        {
            lock (_obj)
            {
                try
                {
                    File.WriteAllText(_path, $" - success.count {_count}\r\n - fail.count {_fail}\r\n - time {DateTime.Now:yyyy-MM-dd HH.mm}");
                    _count = 0;
                    _fail = 0;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }
        
        public void Success()
        {
            lock (_obj)
            {
                _count++;
            }
        }

        public void Fail()
        {
            lock (_obj)
            {
                _fail++;
            }
        }
    }
}