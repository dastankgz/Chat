using System;
using Chat.Abstract;
using Chat.Abstract.Model;
using Chat.Worker.Web.Utils;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NLog;

namespace Chat.Worker.Web.Infrastructure
{
    /// <inheritdoc />
    /// <summary>
    /// TokenAuthorizeAttribute
    /// </summary>
    /// <author email="dastankgz@gmail.com"></author>
    public class TokenAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly IBackendApi _backend;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public TokenAuthorizeAttribute(IBackendApi backendApi)
        {
            _backend = backendApi;
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            try
            {
                var token = request.QueryString.Get("token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    Logger.Trace("Token not exist");
                    return false;
                }

                var context = WorkerContext.Build(token);
                var tokenModel = new TokenModel{Token = token};
                var user = _backend.ValidateToken(context, tokenModel);
                var isValidToken = user != null && user.Success;
                if (isValidToken)
                {
                    var userId = user.Body.UserId;
                    request.Environment[Param.UserId] = userId;
                    request.Environment[Param.UserToken] = token;
                    return true;
                }

                Logger.Trace($"Token [{token}] is not valid");
                return false;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            return true;
        }
    }
}