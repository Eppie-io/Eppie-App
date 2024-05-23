using System;
using System.Net.Http;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Google;
using Finebits.Authorization.OAuth2.Outlook;
using Tuvi.Core;

namespace Tuvi.OAuth2
{
    public static class AuthorizationFactory
    {
        public static AuthorizationProvider GetAuthorizationProvider(AuthorizationConfiguration configuration)
        {
            return GetAuthorizationProvider(configuration, new HttpClient());
        }

        public static AuthorizationProvider GetAuthorizationProvider(AuthorizationConfiguration configuration, HttpClient httpClient)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return AuthorizationProvider.Create(httpClient, configuration.AuthenticationBrokerCreator, configuration.GoogleConfigurationCreator, configuration.OutlookConfigurationCreator);
        }

        public static ITokenRefresher GetTokenRefresher(AuthorizationProvider authProvider)
        {
            if (authProvider is null)
            {
                throw new ArgumentNullException(nameof(authProvider));
            }

            return TokenRefresher.CreateTokenRefresher(authProvider);
        }
    }

    public class AuthorizationConfiguration
    {
        public Func<IAuthenticationBroker> AuthenticationBrokerCreator { get; set; }
        public Func<GoogleConfiguration> GoogleConfigurationCreator { get; set; }
        public Func<OutlookConfiguration> OutlookConfigurationCreator { get; set; }
    }
}
