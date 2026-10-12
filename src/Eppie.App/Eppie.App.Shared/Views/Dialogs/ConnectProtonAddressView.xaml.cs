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
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
#endif

namespace Eppie.App.Views.Dialogs
{
    internal partial class ConnectProtonAddressViewBase : AppDialogViewBase<ConnectProtonAddressViewModel, Account>
    { }

    internal sealed partial class ConnectProtonAddressView : ConnectProtonAddressViewBase
    {
        private const string BlankPage = "about:blank";

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

        private async void OnNavigationCompleted(Microsoft.UI.Xaml.Controls.WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            bool isAboutBlank = string.Equals(sender.Source?.AbsoluteUri, BlankPage, StringComparison.OrdinalIgnoreCase);
            if (isAboutBlank || !args.IsSuccess || args.HttpStatusCode == (int)HttpStatusCode.None)
            {
                return;
            }

            try
            {
                await HumanVerifierWebView.EnsureCoreWebView2Async();
                string script = ScriptLoader.GetProtonCaptchaScript();
                await HumanVerifierWebView.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                ViewModel.OnError(ex);
            }
        }

        private void OnWebMessageReceived(Microsoft.UI.Xaml.Controls.WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.WebMessageAsJson))
            {
                ViewModel.OnHumanVerificationResponse(args.WebMessageAsJson);
            }
        }

        private async Task UpdateHumanVerifierPage()
        {
            await HumanVerifierWebView.EnsureCoreWebView2Async();
            if (ViewModel.Step == ProtonConnectionStep.HumanVerifier)
            {
                string uri = ViewModel.HumanVerifierUri?.ToString() ?? BlankPage;
                HumanVerifierWebView.CoreWebView2.Navigate(uri);
            }
            else
            {
                HumanVerifierWebView.CoreWebView2.Navigate(BlankPage);
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
