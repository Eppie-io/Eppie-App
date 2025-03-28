// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System;
using System.Net.Http;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Google;
using Finebits.Authorization.OAuth2.Outlook;

namespace Tuvi.OAuth2
{
    public class AuthorizationProvider
    {
        public static AuthorizationProvider Create(HttpClient httpClient, Func<IAuthenticationBroker> brokerCreator, Func<GoogleConfiguration> googleConfigCreator, Func<OutlookConfiguration> outlookConfigCreator)
        {
            return new AuthorizationProvider(httpClient, brokerCreator, googleConfigCreator, outlookConfigCreator);
        }

        private static readonly Func<MailService, string> UnknownAuthClientExceptionString = (client) => $"'{client}' is an unknown authorization client.";

        private readonly HttpClient _httpClient;
        private readonly Func<IAuthenticationBroker> _brokerCreator;
        private readonly Func<GoogleConfiguration> _googleConfigurationCreator;
        private readonly Func<OutlookConfiguration> _outlookConfigurationCreator;

        private AuthorizationProvider(HttpClient httpClient, Func<IAuthenticationBroker> brokerCreator, Func<GoogleConfiguration> googleConfigCreator, Func<OutlookConfiguration> outlookConfigCreator)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (brokerCreator is null)
            {
                throw new ArgumentNullException(nameof(brokerCreator));
            }

            if (googleConfigCreator is null)
            {
                throw new ArgumentNullException(nameof(googleConfigCreator));
            }

            if (outlookConfigCreator is null)
            {
                throw new ArgumentNullException(nameof(outlookConfigCreator));
            }

            _httpClient = httpClient;
            _brokerCreator = brokerCreator;
            _googleConfigurationCreator = googleConfigCreator;
            _outlookConfigurationCreator = outlookConfigCreator;
        }

        public IAuthorizationClient CreateAuthorizationClient(MailService mailService)
        {
            switch (mailService)
            {
                case MailService.Outlook: return CreateOutlookAuthClient();
                case MailService.Gmail: return CreateGoogleAuthClient();
                default:
                    throw new ArgumentException(UnknownAuthClientExceptionString(mailService), nameof(mailService));
            }
        }

        public IRefreshable CreateRefreshTokenClient(MailService mailService)
        {
            switch (mailService)
            {
                case MailService.Outlook: return CreateOutlookAuthClient();
                case MailService.Gmail: return CreateGoogleAuthClient();
                default:
                    throw new ArgumentException(UnknownAuthClientExceptionString(mailService), nameof(mailService));
            }
        }

        private GoogleAuthClient CreateGoogleAuthClient()
        {
            return new GoogleAuthClient(_httpClient, _brokerCreator(), _googleConfigurationCreator());
        }

        private OutlookAuthClient CreateOutlookAuthClient()
        {
            return new OutlookAuthClient(_httpClient, _brokerCreator(), _outlookConfigurationCreator());
        }
    }
}
