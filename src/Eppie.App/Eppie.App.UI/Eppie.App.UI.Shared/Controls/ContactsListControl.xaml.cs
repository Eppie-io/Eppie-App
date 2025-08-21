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

using Tuvi.App.ViewModels;
using Eppie.App.UI.Resources;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
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
                var dialog = new ContentDialog
                {
                    Title = StringProvider.GetString("RenameContactDialogTitle"),
                    PrimaryButtonText = StringProvider.GetString("RenameContactDialogPrimaryButtonText"),
                    CloseButtonText = StringProvider.GetString("RenameContactDialogCloseButtonText")
                };

                var stackPanel = new StackPanel { Spacing = 8 };
                var textBox = new TextBox { Header = StringProvider.GetString("RenameContactDialogTextBoxHeader"), Text = contactItem.DisplayName, AcceptsReturn = false };
                stackPanel.Children.Add(textBox);
                dialog.Content = stackPanel;

                textBox.KeyDown += (s, e) =>
                {
                    if (e.Key == Windows.System.VirtualKey.Enter)
                    {
                        dialog.Hide();

                        RenameCommand(contactItem, textBox);
                    }
                    else if (e.Key == Windows.System.VirtualKey.Escape)
                    {
                        dialog.Hide();
                    }
                };

                dialog.PrimaryButtonClick += (s, e) =>
                {
                    RenameCommand(contactItem, textBox);
                };

                _ = dialog.ShowAsync();
            }

            void RenameCommand(ContactItem contact, TextBox textBox)
            {
                contactItem.FullName = textBox.Text;
                ContactsModel?.RenameContactCommand?.Execute(contactItem);
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
