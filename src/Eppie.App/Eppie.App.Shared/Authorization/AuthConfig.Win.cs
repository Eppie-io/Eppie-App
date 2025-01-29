#if WINDOWS10_0_19041_0_OR_GREATER

using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.AuthenticationBroker;
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
                    RedirectUri = DesktopAuthenticationBroker.GetLoopbackUri(),
                    ScopeList = GmailScope
                },
                OutlookConfigurationCreator = () => new OutlookConfiguration
                {
                    ClientId = "<ClientId>",
                    RedirectUri = DesktopAuthenticationBroker.GetLoopbackUri(),
                    ScopeList = OutlookScope
                }
            };
        }

        private static IAuthenticationBroker GetAuthenticationBroker()
        {
            if (!DesktopAuthenticationBroker.IsSupported)
            {
                throw new InvalidOperationException("DesktopAuthenticationBroker is not supported");
            }

            return new DesktopAuthenticationBroker(new WebBrowserLauncher());
        }
    }
}

#endif
