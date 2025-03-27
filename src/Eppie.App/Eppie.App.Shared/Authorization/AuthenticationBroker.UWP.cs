#if WINDOWS_UWP

using System;
using System.Threading;
using System.Threading.Tasks;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;
using Windows.Security.Authentication.Web;

namespace Tuvi.App.Shared.Authorization
{
    internal class AuthenticationBroker : IAuthenticationBroker
    {
        public async Task<AuthenticationResult> AuthenticateAsync(Uri requestUri, Uri callbackUri, CancellationToken cancellationToken = default)
        {
            WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, requestUri, callbackUri);

            if (WebAuthenticationStatus.UserCancel == result.ResponseStatus)
            {
                throw new OperationCanceledException();
            }

            return new AuthenticationResult(OAuth2.Toolkit.ParseQueryString(result.ResponseData));
        }
    }
}

#endif
