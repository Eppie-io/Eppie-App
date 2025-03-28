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
