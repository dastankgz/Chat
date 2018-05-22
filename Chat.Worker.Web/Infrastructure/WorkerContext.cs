using Chat.Abstract;

namespace Chat.Worker.Web.Infrastructure
{
    /// <summary>
    /// WorkerContext
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class WorkerContext : IWorkerContext
    {
        public string CurUserToken { get; set; }

        public static WorkerContext Build(string token) => new WorkerContext {CurUserToken = token};
    }
}