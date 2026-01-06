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

using Tuvi.App.ViewModels;
using Eppie.App.UI.Resources;
using System.Diagnostics.CodeAnalysis;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class ContactsListControl : UserControl
    {
        private static readonly StringProvider StringProvider = StringProvider.GetInstance();

        public ContactsModel ContactsModel
        {
            get { return (ContactsModel)GetValue(ContactsModelProperty); }
            set { SetValue(ContactsModelProperty, value); }
        }
        public static readonly DependencyProperty ContactsModelProperty =
            DependencyProperty.Register(nameof(ContactsModel), typeof(ContactsModel), typeof(ContactsListControl), new PropertyMetadata(null));

        public ContactsListControl()
        {
            this.InitializeComponent();
        }

        private void RenameContactMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                _ = Common.UITools.ShowRenameContactDialogAsync(
                        StringProvider.GetString("RenameContactDialogTitle"),
                        StringProvider.GetString("RenameContactDialogPrimaryButtonText"),
                        StringProvider.GetString("RenameContactDialogCloseButtonText"),
                        StringProvider.GetString("RenameContactDialogTextBoxHeader"),
                        contactItem.DisplayName,
                        this.XamlRoot,
                        (newName) =>
                        {
                            contactItem.FullName = newName;
                            ContactsModel?.RenameContactCommand?.Execute(contactItem);
                        });
            }
        }

        private void ChangeContactAvatarMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                ContactsModel?.ChangeContactAvatarCommand?.Execute(contactItem);
            }
        }

        private void RemoveContactMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                ContactsModel?.RemoveContactCommand?.Execute(contactItem);
            }
        }
    }
}
