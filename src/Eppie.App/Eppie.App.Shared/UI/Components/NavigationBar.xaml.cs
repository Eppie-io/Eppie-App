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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
#endif

namespace Eppie.App.UI.Components
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class NavigationBar : UserControl
    {

        // Todo: Remove it after the new UI of the MainPage is implemented
        public ICommand ComposeMessageCommand
        {
            get { return (ICommand)GetValue(ComposeMessageCommandProperty); }
            set { SetValue(ComposeMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty ComposeMessageCommandProperty =
            DependencyProperty.Register(nameof(ComposeMessageCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));



        public ICommand LeftPaneCommand
        {
            get { return (ICommand)GetValue(LeftPaneCommandProperty); }
            set { SetValue(LeftPaneCommandProperty, value); }
        }

        public static readonly DependencyProperty LeftPaneCommandProperty =
            DependencyProperty.Register(nameof(LeftPaneCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public ICommand InviteCommand
        {
            get { return (ICommand)GetValue(InviteCommandProperty); }
            set { SetValue(InviteCommandProperty, value); }
        }

        public static readonly DependencyProperty InviteCommandProperty =
            DependencyProperty.Register(nameof(InviteCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public ICommand MainPageCommand
        {
            get { return (ICommand)GetValue(MainPageCommandProperty); }
            set { SetValue(MainPageCommandProperty, value); }
        }


        public static readonly DependencyProperty MainPageCommandProperty =
            DependencyProperty.Register(nameof(MainPageCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public ICommand MailboxesCommand
        {
            get { return (ICommand)GetValue(MailboxesCommandProperty); }
            set { SetValue(MailboxesCommandProperty, value); }
        }

        public static readonly DependencyProperty MailboxesCommandProperty =
            DependencyProperty.Register(nameof(MailboxesCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public ICommand ContactsCommand
        {
            get { return (ICommand)GetValue(ContactsCommandProperty); }
            set { SetValue(ContactsCommandProperty, value); }
        }

        public static readonly DependencyProperty ContactsCommandProperty =
            DependencyProperty.Register(nameof(ContactsCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public ICommand AddressManagerCommand
        {
            get { return (ICommand)GetValue(AddressManagerCommandProperty); }
            set { SetValue(AddressManagerCommandProperty, value); }
        }

        public static readonly DependencyProperty AddressManagerCommandProperty =
            DependencyProperty.Register(nameof(AddressManagerCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public Visibility AIAgentSettingsVisibility
        {
            get { return (Visibility)GetValue(AIAgentSettingsVisibilityProperty); }
            set { SetValue(AIAgentSettingsVisibilityProperty, value); }
        }

        public static readonly DependencyProperty AIAgentSettingsVisibilityProperty =
            DependencyProperty.Register(nameof(AIAgentSettingsVisibility), typeof(Visibility), typeof(NavigationBar), new PropertyMetadata(Visibility.Collapsed));


        public ICommand AIAgentSettingsCommand
        {
            get { return (ICommand)GetValue(AIAgentSettingsCommandProperty); }
            set { SetValue(AIAgentSettingsCommandProperty, value); }
        }

        public static readonly DependencyProperty AIAgentSettingsCommandProperty =
            DependencyProperty.Register(nameof(AIAgentSettingsCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public ICommand AboutCommand
        {
            get { return (ICommand)GetValue(AboutCommandProperty); }
            set { SetValue(AboutCommandProperty, value); }
        }

        public static readonly DependencyProperty AboutCommandProperty =
            DependencyProperty.Register(nameof(AboutCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public ICommand AppSettingsCommand
        {
            get { return (ICommand)GetValue(AppSettingsCommandProperty); }
            set { SetValue(AppSettingsCommandProperty, value); }
        }

        public static readonly DependencyProperty AppSettingsCommandProperty =
            DependencyProperty.Register(nameof(AppSettingsCommand), typeof(ICommand), typeof(NavigationBar), new PropertyMetadata(null));


        public FlyoutBase WhatsNewFlyout
        {
            get { return (FlyoutBase)GetValue(WhatsNewFlyoutProperty); }
            set { SetValue(WhatsNewFlyoutProperty, value); }
        }
        public static readonly DependencyProperty WhatsNewFlyoutProperty =
            DependencyProperty.Register(nameof(WhatsNewFlyout), typeof(FlyoutBase), typeof(NavigationBar), new PropertyMetadata(null));


        public Visibility SupportVisibility
        {
            get { return (Visibility)GetValue(SupportVisibilityProperty); }
            set { SetValue(SupportVisibilityProperty, value); }
        }

        public static readonly DependencyProperty SupportVisibilityProperty =
            DependencyProperty.Register(nameof(SupportVisibility), typeof(Visibility), typeof(NavigationBar), new PropertyMetadata(Visibility.Collapsed));


        public FlyoutBase SupportFlyout
        {
            get { return (FlyoutBase)GetValue(SupportFlyoutProperty); }
            set { SetValue(SupportFlyoutProperty, value); }
        }
        public static readonly DependencyProperty SupportFlyoutProperty =
            DependencyProperty.Register(nameof(SupportFlyout), typeof(FlyoutBase), typeof(NavigationBar), new PropertyMetadata(null));

        public NavigationBar()
        {
            this.InitializeComponent();
        }
    }
}
