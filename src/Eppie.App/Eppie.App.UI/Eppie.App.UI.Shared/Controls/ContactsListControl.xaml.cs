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

using System.Collections;
using System.Windows.Input;
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

        public IEnumerable Contacts
        {
            get { return (IEnumerable)GetValue(ContactsProperty); }
            set { SetValue(ContactsProperty, value); }
        }

        public static readonly DependencyProperty ContactsProperty =
            DependencyProperty.Register(nameof(Contacts), typeof(IEnumerable), typeof(ContactsListControl), new PropertyMetadata(null));

        public ContactItem SelectedContact
        {
            get { return (ContactItem)GetValue(SelectedContactProperty); }
            set { SetValue(SelectedContactProperty, value); }
        }

        public static readonly DependencyProperty SelectedContactProperty =
            DependencyProperty.Register(nameof(SelectedContact), typeof(ContactItem), typeof(ContactsListControl), new PropertyMetadata(null));

        public ICommand ContactClickCommand
        {
            get { return (ICommand)GetValue(ContactClickCommandProperty); }
            set { SetValue(ContactClickCommandProperty, value); }
        }

        public static readonly DependencyProperty ContactClickCommandProperty =
            DependencyProperty.Register(nameof(ContactClickCommand), typeof(ICommand), typeof(ContactsListControl), new PropertyMetadata(null));

        public ICommand RenameContactCommand
        {
            get { return (ICommand)GetValue(RenameContactCommandProperty); }
            set { SetValue(RenameContactCommandProperty, value); }
        }

        public static readonly DependencyProperty RenameContactCommandProperty =
            DependencyProperty.Register(nameof(RenameContactCommand), typeof(ICommand), typeof(ContactsListControl), new PropertyMetadata(null));

        public ICommand ChangeContactAvatarCommand
        {
            get { return (ICommand)GetValue(ChangeContactAvatarCommandProperty); }
            set { SetValue(ChangeContactAvatarCommandProperty, value); }
        }

        public static readonly DependencyProperty ChangeContactAvatarCommandProperty =
            DependencyProperty.Register(nameof(ChangeContactAvatarCommand), typeof(ICommand), typeof(ContactsListControl), new PropertyMetadata(null));

        public ICommand RemoveContactCommand
        {
            get { return (ICommand)GetValue(RemoveContactCommandProperty); }
            set { SetValue(RemoveContactCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveContactCommandProperty =
            DependencyProperty.Register(nameof(RemoveContactCommand), typeof(ICommand), typeof(ContactsListControl), new PropertyMetadata(null));

        public ICommand InviteContactCommand
        {
            get { return (ICommand)GetValue(InviteContactCommandProperty); }
            set { SetValue(InviteContactCommandProperty, value); }
        }

        public static readonly DependencyProperty InviteContactCommandProperty =
            DependencyProperty.Register(nameof(InviteContactCommand), typeof(ICommand), typeof(ContactsListControl), new PropertyMetadata(null));

        public ICommand ComposeEmailCommand
        {
            get { return (ICommand)GetValue(ComposeEmailCommandProperty); }
            set { SetValue(ComposeEmailCommandProperty, value); }
        }

        public static readonly DependencyProperty ComposeEmailCommandProperty =
            DependencyProperty.Register(nameof(ComposeEmailCommand), typeof(ICommand), typeof(ContactsListControl), new PropertyMetadata(null));

        public ContactsListControl()
        {
            this.InitializeComponent();
        }

        private void RenameContactMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                _ = Common.UITools.ShowTextInputDialogAsync(
                        StringProvider.GetString("RenameContactDialogTitle"),
                        StringProvider.GetString("RenameContactDialogPrimaryButtonText"),
                        StringProvider.GetString("RenameContactDialogCloseButtonText"),
                        StringProvider.GetString("RenameContactDialogTextBoxHeader"),
                        contactItem.DisplayName,
                        this.XamlRoot,
                        (newName) =>
                        {
                            contactItem.FullName = newName;
                            RenameContactCommand?.Execute(contactItem);
                        });
            }
        }

        private void ChangeContactAvatarMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                ChangeContactAvatarCommand?.Execute(contactItem);
            }
        }

        private void RemoveContactMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                RemoveContactCommand?.Execute(contactItem);
            }
        }

        private void InviteContactMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                InviteContactCommand?.Execute(contactItem);
            }
        }

        private void ComposeEmailMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                ComposeEmailCommand?.Execute(contactItem);
            }
        }
    }
}
