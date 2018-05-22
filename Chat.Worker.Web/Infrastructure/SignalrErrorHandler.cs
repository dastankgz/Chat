using Microsoft.AspNet.SignalR.Hubs;
using NLog;

namespace Chat.Worker.Web.Infrastructure
{
    /// <inheritdoc />
    /// <summary>
    /// SignalrErrorHandler
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class SignalrErrorHandler : HubPipelineModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void OnIncomingError(ExceptionContext exceptionContext,
            IHubIncomingInvokerContext invokerContext)
        {
            Logger.Error(exceptionContext.Error, exceptionContext.Error.Message);

            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }
}