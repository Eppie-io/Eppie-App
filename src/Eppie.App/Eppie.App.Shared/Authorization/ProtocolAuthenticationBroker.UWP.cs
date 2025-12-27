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
using Windows.System;
using Eppie.App.UI.Common;

namespace Eppie.App.Authorization
{
    /// <summary>
    /// Authentication broker that uses protocol activation to receive OAuth callbacks from system browser
    /// </summary>
    internal class ProtocolAuthenticationBroker : IAuthenticationBroker
    {
        private static TaskCompletionSource<AuthenticationResult> _authCompletionSource;

        public async Task<AuthenticationResult> AuthenticateAsync(
            Uri requestUri,
            Uri callbackUri,
            CancellationToken cancellationToken = default)
        {
            if (requestUri is null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var dialogCts = new CancellationTokenSource();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, dialogCts.Token))
            {
                Action closeAction = null;
                try
                {
                    _authCompletionSource = new TaskCompletionSource<AuthenticationResult>();

                    var loader = Eppie.App.UI.Resources.StringProvider.GetInstance();

                    closeAction = await UITools.ShowAuthenticationDialogAsync(
                        loader.GetString("AuthenticationTitle"),
                        loader.GetString("AuthenticationContenet"),
                        loader.GetString("MessageButtonCancel"),
                        null,
                        () => dialogCts.Cancel()
                    ).ConfigureAwait(true);

                    bool launched = await Launcher.LaunchUriAsync(requestUri);

                    if (!launched)
                    {
                        throw new InvalidOperationException("Failed to launch browser for authentication");
                    }

                    using (linkedCts.Token.Register(() =>
                    {
                        _authCompletionSource?.TrySetCanceled();
                    }))
                    {
                        var result = await _authCompletionSource.Task.ConfigureAwait(true);

                        return result;
                    }
                }
                finally
                {
                    closeAction?.Invoke();
                    dialogCts.Dispose();
                    _authCompletionSource = null;
                }
            }
        }

        /// <summary>
        /// Called by App.OnActivated when protocol activation occurs
        /// </summary>
        public static void CompleteAuthentication(Uri responseUri)
        {
            if (_authCompletionSource is null)
            {
                // No active authentication session
                return;
            }

            try
            {
                var result = new AuthenticationResult(
                    Tuvi.OAuth2.Toolkit.ParseQueryString(responseUri)
                );
                _authCompletionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                _authCompletionSource.TrySetException(ex);
            }
        }

        /// <summary>
        /// Called when user cancels authentication
        /// </summary>
        public static void CancelAuthentication()
        {
            _authCompletionSource?.TrySetCanceled();
        }
    }
}

#endif
