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
using Eppie.App.UI.Controls;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
#endif

namespace Eppie.App.Views
{
    internal partial class InvitationPageBase : BasePage<InvitationPageViewModel, BaseViewModel>
    { }

    internal sealed partial class InvitationPage : InvitationPageBase, IPopupPage
    {
        public event EventHandler ClosePopupRequested;

        public InvitationPage()
        {
            this.InitializeComponent();

            ViewModel.ClosePopupAction = () => ClosePopupRequested?.Invoke(this, EventArgs.Empty);
        }

        public void OnCloseClicked()
        { }


        private void OnRecipientRemoved(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is ContactItem item)
            {
                ViewModel?.OnRecipientRemoved(item);
            }
        }

        private void OnContactQueryChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel?.OnContactQueryChanged(sender.Text);
            }
        }

        private void OnContactQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel?.OnContactQuerySubmitted(args.ChosenSuggestion as ContactItem, args.QueryText);
            ViewModel?.OnContactQueryChanged(string.Empty);
            sender.Text = string.Empty;
        }
    }

    public class EppieAddressesDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DropDownTemplate { get; set; }
        public DataTemplate SelectedTemplate { get; set; }
        public DataTemplate CreateAddressTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is CreateEppieAddressItem) return CreateAddressTemplate;

            if (item is AddressItem)
            {
                if (IsDropDownItem(container))
                {
                    return DropDownTemplate;
                }
                else
                {
                    if (item is AddressItem) return SelectedTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }

        private static bool IsDropDownItem(DependencyObject container)
        {
            var current = container;

            while (current != null)
            {
                if (current is ComboBoxItem)
                {
                    return true;
                }

                if (current is ComboBox)
                {
                    return false;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
