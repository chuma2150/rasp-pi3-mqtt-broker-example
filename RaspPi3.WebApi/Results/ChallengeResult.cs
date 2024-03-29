﻿using Microsoft.Owin.Security;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace RaspPi3.WebApi.Results
{
    public class ChallengeResult : HttpUnauthorizedResult, IHttpActionResult
    {
        // Used for XSRF protection when adding external logins
        public const string XsrfKey = "XsrfId";

        public ChallengeResult(string loginProvider, ApiController controller)
        {
            LoginProvider = loginProvider;
            Request = controller.Request;
        }
        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }
        public string RedirectUri { get; }
        public string UserId { get; }
        public string LoginProvider { get; }
        public HttpRequestMessage Request { get; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            Request.GetOwinContext().Authentication.Challenge(LoginProvider);

            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = Request
            };
            return Task.FromResult(response);
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
            if (UserId != null)
            {
                properties.Dictionary[XsrfKey] = UserId;
            }
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
}
