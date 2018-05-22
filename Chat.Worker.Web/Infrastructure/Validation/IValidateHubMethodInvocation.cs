using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Hubs;

namespace Chat.Worker.Web.Infrastructure.Validation
{
    public interface IValidateHubMethodInvocation
    {
        IEnumerable<ValidationError> ValidateHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext);
    }
}