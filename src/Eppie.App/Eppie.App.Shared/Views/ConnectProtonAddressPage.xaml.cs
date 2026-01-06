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
using Eppie.App.UI.Controls;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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

        public static Visibility HideOnStep(ProtonConnectionStep target, ProtonConnectionStep step)
        {
            return target != step ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility ShowOnStep(ProtonConnectionStep target, ProtonConnectionStep step)
        {
            return target == step ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
