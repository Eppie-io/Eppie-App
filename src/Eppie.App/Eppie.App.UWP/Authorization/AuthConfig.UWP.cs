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

#if WINDOWS_UWP

using System;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Google;
using Finebits.Authorization.OAuth2.Outlook;
using Tuvi.OAuth2;

namespace Eppie.App.Authorization
{
    internal static partial class AuthConfig
    {
        public static readonly string UriScheme = "<UriScheme>";

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
            return new ProtocolAuthenticationBroker();
        }
    }
}

#endif
