// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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
using System.Threading.Tasks;
using Eppie.App.Helpers;
using Microsoft.Web.WebView2.Core;
using Tuvi.App.ViewModels;
using Tuvi.Core.Entities;
using Windows.Web.Http;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
#endif

namespace Eppie.App.Views.Dialogs
{
    internal partial class ConnectProtonAddressViewBase : AppDialogViewBase<ConnectProtonAddressViewModel, Account>
    { }

    internal sealed partial class ConnectProtonAddressView : ConnectProtonAddressViewBase
    {
        public ConnectProtonAddressView()
        {
            this.InitializeComponent();

            ViewModel.CloseAction = Close;

            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.Step))
                {
                    UpdateFocus();
                }
            };
        }

        public override void OnOpened()
        {
            base.OnOpened();

            UpdateFocus();

            AddressBox.IsEnabledChanged += (s, e) =>
            {
                UpdateFocus();
            };

            TwoFactorCodeBox.IsEnabledChanged += (s, e) =>
            {
                UpdateFocus();
            };

            MailboxPasswordBox.IsEnabledChanged += (s, e) =>
            {
                UpdateFocus();
            };
        }
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.OnEnterKeyDown();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        private async void OnStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            try
            {
                await UpdateHumanVerifierPage().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                ViewModel.OnError(ex);
            }
        }

        private async void OnNavigationCompleted(WebView2Control sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            try
            {
                if (!args.IsSuccess)
                {
                    throw new InvalidOperationException($"Navigation to {sender.Source} failed with error code {args.WebErrorStatus}");
                }

                await sender.EnsureCoreWebView2Async();
                string script = ScriptLoader.GetProtonCaptchaScript();
                await sender.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                ViewModel.OnError(ex);
            }
        }

        private void OnWebMessageReceived(WebView2Control sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            // On Linux, the message might arrive as a raw text string representing JSON.
            if (!TryGetWebMessageAsString(args, out string message))
            {
                message = args.WebMessageAsJson;
            }

            if (!string.IsNullOrEmpty(message))
            {
                ViewModel.OnHumanVerificationResponse(message);
            }
        }

        private static bool TryGetWebMessageAsString(CoreWebView2WebMessageReceivedEventArgs args, out string message)
        {
            message = null;
            try
            {
                message = args.TryGetWebMessageAsString();
                return !string.IsNullOrEmpty(message);
            }
            catch
            { }

            return false;
        }

        private async Task UpdateHumanVerifierPage()
        {
            if (ViewModel.Step == ProtonConnectionStep.HumanVerifier)
            {
                WebView2Control webView = new WebView2Control()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                WebViewParent.Child = webView;

                await webView.EnsureCoreWebView2Async();
                webView.NavigationCompleted += OnNavigationCompleted;
                webView.WebMessageReceived += OnWebMessageReceived;
                webView.CoreWebView2.Navigate(ViewModel.HumanVerifierUri?.ToString());
            }
            else
            {
                if (WebViewParent.Child is WebView2Control webView)
                {
                    webView.NavigationCompleted -= OnNavigationCompleted;
                    webView.WebMessageReceived -= OnWebMessageReceived;
                }

                WebViewParent.Child = null;
            }
        }

        private void UpdateFocus()
        {
            if (ViewModel.IsProcess)
            {
                return;
            }

            switch (ViewModel.Step)
            {
                case ProtonConnectionStep.Credentials:
                    ScheduleFocus(AddressBox);
                    break;
                case ProtonConnectionStep.HumanVerifier: break;
                case ProtonConnectionStep.TwoFactorCode:
                    ScheduleFocus(TwoFactorCodeBox);
                    break;
                case ProtonConnectionStep.UnlockMailbox:
                    ScheduleFocus(MailboxPasswordBox);
                    break;
                case ProtonConnectionStep.Done:
                    ScheduleFocus(DoneButton);
                    break;
            }
        }

        // TODO: Need a better solution for this.
        // HACK: This is a workaround for focus issues where setting focus immediately doesn't always work.
        // Delay a short time to allow XAML visibility/layout/bindings to settle, then focus if control is visible and enabled.
        private async void ScheduleFocus(Control control)
        {
            if (control is null)
            {
                return;
            }

            await Task.Delay(50).ConfigureAwait(true);

            if (ViewModel.IsProcess)
            {
                return;
            }

            if (control.Visibility == Visibility.Visible && control.IsEnabled)
            {
                control.Focus(FocusState.Programmatic);
            }
        }
    }
}
