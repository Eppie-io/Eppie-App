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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Eppie.App.UI.Controls;
using Eppie.App.WebViewHelper;
using Microsoft.Web.WebView2.Core;
using Tuvi.App.ViewModels;
using Windows.Web.Http;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Views
{
    internal partial class ConnectProtonAddressPageBase : BasePage<ConnectProtonAddressPageViewModel, BaseViewModel>
    { }

    internal sealed partial class ConnectProtonAddressPage : ConnectProtonAddressPageBase, IPopupPage
    {
        public event EventHandler ClosePopupRequested;

        public ConnectProtonAddressPage()
        {
            this.InitializeComponent();

            // Todo: Remove this piece of code when MacOS will be fixed.
#if HAS_UNO
            ViewModel.IsMacOS = OperatingSystem.IsMacOS();
#endif

            ViewModel.ClosePopupAction = ClosePopup;

            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.Step))
                {
                    UpdateFocus();
                }
            };
        }

        public void OnCloseClicked()
        {
            ViewModel.ClosedCommand?.Execute(null);
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
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
            if (!args.IsSuccess || args.HttpStatusCode == (int)HttpStatusCode.None)
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
            if (!string.IsNullOrEmpty(args.WebMessageAsJson) &&
                HumanVerificationResponse.TryDeserialize(args.WebMessageAsJson, out HumanVerificationResponse response))
            {
                if (response?.IsCaptcha() is true)
                {
                    ViewModel.HumanVerificationCompletedCommand?.Execute((HumanVerificationResponse.HumanVerificationType, response.Token));
                }
            }
        }

        private async Task UpdateHumanVerifierPage()
        {
            const string blankPage = "about:blank";

            await HumanVerifierWebView.EnsureCoreWebView2Async();
            if (ViewModel.Step == ProtonConnectionStep.HumanVerifier)
            {
                string uri = ViewModel.HumanVerifierUri?.ToString() ?? blankPage;
                HumanVerifierWebView.CoreWebView2.Navigate(uri);
            }
            else
            {
                HumanVerifierWebView.CoreWebView2.Navigate(blankPage);
            }
        }

        private void ClosePopup()
        {
            ClosePopupRequested?.Invoke(this, EventArgs.Empty);
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

    internal class HumanVerificationResponse
    {
        public static readonly string HumanVerificationType = "captcha";
        private static readonly string PostMessageCaptchaTypeKey = "pm_captcha";

        [JsonPropertyName("type")]
        public string PostMessageType { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        public bool IsCaptcha()
        {
            return !string.IsNullOrEmpty(PostMessageType) && PostMessageType == PostMessageCaptchaTypeKey;
        }

        public static bool TryDeserialize(string json, out HumanVerificationResponse response)
        {
            response = null;
            try
            {
#if WINDOWS_UWP
                response = JsonSerializer.Deserialize<HumanVerificationResponse>(json);
#else
                response = JsonSerializer.Deserialize<HumanVerificationResponse>(json, ConnectProtonJsonContext.Default.HumanVerificationResponse);
#endif
                return true;
            }
            catch (ArgumentNullException)
            { }
            catch (JsonException)
            { }
            catch (NotSupportedException)
            { }

            return false;
        }
    }

#if !WINDOWS_UWP
    [JsonSerializable(typeof(HumanVerificationResponse))]
    internal partial class ConnectProtonJsonContext : JsonSerializerContext
    {
    }
#endif
}
