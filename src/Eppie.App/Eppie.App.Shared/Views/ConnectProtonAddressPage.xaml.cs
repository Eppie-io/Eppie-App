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
                    AddressBox.Focus(FocusState.Programmatic);
                    break;
                case ProtonConnectionStep.HumanVerifier: break;
                case ProtonConnectionStep.TwoFactorCode:
                    TwoFactorCodeBox.Focus(FocusState.Programmatic);
                    break;
                case ProtonConnectionStep.UnlockMailbox:
                    MailboxPasswordBox.Focus(FocusState.Programmatic);
                    break;
                case ProtonConnectionStep.Done:
                    DoneButton.Focus(FocusState.Programmatic);
                    break;
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
