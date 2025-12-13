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

#if !WINDOWS_UWP

using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;

namespace Tuvi.App.Shared.Authorization
{
    internal class AuthenticationBroker : IAuthenticationBroker
    {
        private IAuthenticationBroker Broker { get; }

        public AuthenticationBroker(IAuthenticationBroker broker)
        {
            if (broker is null)
            {
                throw new ArgumentNullException(nameof(broker));
            }

            Broker = broker;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(Uri requestUri, Uri callbackUri, CancellationToken cancellationToken = default)
        {
            var dialogCts = new CancellationTokenSource();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, dialogCts.Token))
            {
                Action closeAction = null;
                try
                {
                    var app = Application.Current as Eppie.App.Shared.App;
                    var xamlRoot = app.XamlRoot;

                    var loader = Eppie.App.UI.Resources.StringProvider.GetInstance();

                    closeAction = await Common.UITools.ShowAuthenticationDialogAsync(
                        loader.GetString("AuthenticationTitle"),
                        loader.GetString("AuthenticationContenet"),
                        loader.GetString("MessageButtonCancel"),
                        xamlRoot,
                        () => dialogCts.Cancel()
                    ).ConfigureAwait(true);

                    var result = await Broker.AuthenticateAsync(requestUri, callbackUri, linkedCts.Token).ConfigureAwait(true);

                    return result;
                }
                finally
                {
                    closeAction?.Invoke();
                    dialogCts.Dispose();
                }
            }
        }
    }
}

#endif
