using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using NLog;

namespace Chat.Worker.Web.Infrastructure.Validation
{
    public class ValidationModule : HubPipelineModule
    {
        private readonly ConcurrentDictionary<MethodDescriptor, IEnumerable<IValidateHubMethodInvocation>> _methodInvocationCache = new ConcurrentDictionary<MethodDescriptor, IEnumerable<IValidateHubMethodInvocation>>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
        {
            return base.BuildIncoming(context =>
            {
                var methodLevelValidator = _methodInvocationCache.GetOrAdd(context.MethodDescriptor,
                    methodDescriptor => methodDescriptor.Attributes.OfType<IValidateHubMethodInvocation>()).FirstOrDefault();

                // no validator... keep going on with the rest of the pipeline
                if (methodLevelValidator == null)
                    return invoke(context);

                var validationErrors = methodLevelValidator.ValidateHubMethodInvocation(context);
                // no errors... keep going on with the rest of the pipeline
                if (!validationErrors.Any())
                    return invoke(context);

                string errorsInJson = JsonConvert.SerializeObject(validationErrors);

                Logger.Warn(errorsInJson);
                // Send error back to the client
                return FromError<object>(new ValidationException($"ValidationError|{errorsInJson}."));
            });
        }

        private static Task<T> FromError<T>(Exception e)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(e);
            return tcs.Task;
        }
    }
}