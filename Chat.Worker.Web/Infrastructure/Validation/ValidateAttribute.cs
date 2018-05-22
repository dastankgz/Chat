using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.SignalR.Hubs;

namespace Chat.Worker.Web.Infrastructure.Validation
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class ValidateAttribute : Attribute, IValidateHubMethodInvocation
    {
        public IEnumerable<ValidationError> ValidateHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext)
        {
            var errors = new List<ValidationError>();

            for (int i = 0; i < hubIncomingInvokerContext.Args.Count; ++i)
            {
                var arg = hubIncomingInvokerContext.Args[i];
                var paramName = hubIncomingInvokerContext.MethodDescriptor.Parameters[i].Name;
                if (arg == null)
                {
                    var ve = ValidationErrorsFactory.BuildNull(paramName);
                    errors.Add(ve);
                    continue;
                }

                var ctx = new ValidationContext(arg, null, null);
                var results = new List<ValidationResult>();

                bool valid = Validator.TryValidateObject(arg, ctx, results, true);

                if (!valid)
                {
                    ValidationError ve = ValidationErrorsFactory.Build(paramName, arg, results);
                    errors.Add(ve);
                }
            }

            return errors;
        }
    }
}