#if WINDOWS_UWP

using System;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Google;
using Finebits.Authorization.OAuth2.Outlook;
using Tuvi.OAuth2;

namespace Tuvi.App.Shared.Authorization
{
    internal static partial class AuthConfig
    {
        public static AuthorizationConfiguration GetAuthorizationConfiguration()
        {
            return new AuthorizationConfiguration
            {
                AuthenticationBrokerCreator = GetAuthenticationBroker,
                GoogleConfigurationCreator = () => new GoogleConfiguration
                {
                    ClientId = "<ClientId>",
                    RedirectUri = new Uri("<RedirectUri>"),
                    ScopeList = GmailScope
                },
                OutlookConfigurationCreator = () => new OutlookConfiguration
                {
                    ClientId = "<ClientId>",
                    RedirectUri = new Uri("<RedirectUri>"),
                    ScopeList = OutlookScope
                }
            };
        }

        private static IAuthenticationBroker GetAuthenticationBroker()
        {
            return new AuthenticationBroker();
        }
    }
}

#endif
