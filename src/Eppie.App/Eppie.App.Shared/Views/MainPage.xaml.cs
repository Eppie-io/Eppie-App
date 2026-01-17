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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Eppie.App.UI.Extensions;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Messages;
using Tuvi.App.ViewModels.Services;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml.Navigation;
#endif

namespace Eppie.App.Views
{
    internal partial class MainPageBase : BasePage<MainPageViewModel, BaseViewModel>
    {
    }

    internal sealed partial class MainPage : MainPageBase, IErrorHandler
    {
        public ICommand ShowAllMessagesCommand => new RelayCommand(ShowAllMessages);

        public ICommand MailBoxItemClickCommand => new RelayCommand<MailBoxItem>(MailBoxItemClick);

        public ICommand MailBoxItemDropCommand => new RelayCommand<MailBoxItem>(MailBoxItemDropMessages, IsDropMessagesAllowed);

        public ICommand ShowAboutPageCommand => new RelayCommand(ShowAboutPage);

        public ICommand OpenAddressManagerCommand => new RelayCommand(ShowAddressManagerPane);

        public ICommand OpenContactsPanelCommand => new RelayCommand(ToggleContactsPanelPane);

        public ICommand OpenMailboxesPanelCommand => new RelayCommand(ToggleMailboxesPanelPane);

        public ICommand OpenAIAgentsPanelCommand => new RelayCommand(ToggleAIAgentsPane);

        public ICommand ClosePaneCommand => new RelayCommand(ClosePane);

        public MainPage()
        {
            this.InitializeComponent();

            ViewModel.InitializeMailboxModel(MailBoxItemClickCommand, MailBoxItemDropCommand);

            NavigationMenu.PaneOpened += OnNavigationPaneToggled;
            NavigationMenu.PaneClosed += OnNavigationPaneToggled;

            WeakReferenceMessenger.Default.Register<ContactSelectedMessage>(this, OnContactSelected);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                ShowAllMessagesCommand.Execute(this);

                var app = App.Current as App;
                var settings = app?.LocalSettingsService;

                if (settings != null)
                {
                    NavigationMenu.IsPaneOpen = settings.IsNavigationPaneOpen;
                }

                if (settings != null && settings.LastSidePane != SidePaneKind.None)
                {
                    OpenPane(settings.LastSidePane);
                }

                OpenAddressManagerPaneIfNeeded();
            }
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ShowAppSettings();
            }
            else if (args.InvokedItemContainer is DependencyObject dependencyObject)
            {
                AttachedCommands.GetClickCommand(dependencyObject)?.Execute(null);
            }
        }

        private void OnNavigationPaneToggled(NavigationView sender, object args)
        {
            var app = Eppie.App.App.Current as Eppie.App.App;
            var settings = app?.LocalSettingsService;
            if (settings != null)
            {
                settings.IsNavigationPaneOpen = sender.IsPaneOpen;
            }
        }

        private void ShowAboutPage()
        {
            contentFrame.Navigate(typeof(AboutPage));
        }

        private SidePaneKind _openedPane = SidePaneKind.None;

        private void ToggleAIAgentsPane()
        {
            if (splitView.IsPaneOpen && _openedPane == SidePaneKind.AIAgentsPanel)
            {
                ClosePane();
            }
            else
            {
                OpenPane(SidePaneKind.AIAgentsPanel);
            }
        }

        private void ShowAddressManagerPane()
        {
            contentFrame.Navigate(typeof(AddressManagerPage));
        }

        private void ToggleContactsPanelPane()
        {
            if (splitView.IsPaneOpen && _openedPane == SidePaneKind.ContactsPanel)
            {
                ClosePane();
            }
            else
            {
                OpenPane(SidePaneKind.ContactsPanel);
            }
        }

        private void ToggleMailboxesPanelPane()
        {
            if (splitView.IsPaneOpen && _openedPane == SidePaneKind.MailboxesPanel)
            {
                ClosePane();
            }
            else
            {
                OpenPane(SidePaneKind.MailboxesPanel);
            }
        }

        private void OpenPane(SidePaneKind kind)
        {
            splitView.IsPaneOpen = true;
            _openedPane = kind;

            switch (kind)
            {
                case SidePaneKind.AIAgentsPanel:
                    paneFrame.Navigate(typeof(AIAgentsManagerPage));
                    break;
                case SidePaneKind.ContactsPanel:
                    paneFrame.Navigate(typeof(ContactsPanelPage));
                    break;
                case SidePaneKind.MailboxesPanel:
                    paneFrame.Navigate(typeof(MailboxesPanelPage), ViewModel.MailBoxesModel);
                    ViewModel.UpdateAccountsList();
                    break;
                default:
                    break;
            }

            SavePaneState();
        }

        private async void OpenAddressManagerPaneIfNeeded()
        {
            try
            {
                if (await ViewModel.IsAccountListEmptyAsync())
                {
                    ShowAddressManagerPane();
                }
            }
            catch (Exception ex)
            {
                ViewModel.OnError(ex);
            }
        }

        private void ClosePane()
        {
            splitView.IsPaneOpen = false;
            _openedPane = SidePaneKind.None;
            SavePaneState();
        }

        private void SavePaneState()
        {
            var app = App.Current as App;
            var settings = app?.LocalSettingsService;
            if (settings != null)
            {
                settings.LastSidePane = _openedPane;
            }
        }

        private void ShowAllMessages()
        {
            ViewModel.OnShowAllMessages();
            contentFrame.Navigate(typeof(AllMessagesPage), new AllMessagesPageViewModel.NavigationData() { ErrorHandler = this });
        }

        private void ShowAppSettings()
        {
            contentFrame.Navigate(typeof(AppSettingsPage), new AllMessagesPageViewModel.NavigationData() { ErrorHandler = this });
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Event handler is referenced from XAML and must be an instance method.")]
        private void OnElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
        {
            var infoBar = args.Element as InfoBar;
            if (infoBar != null)
            {
                infoBar.IsOpen = true;
            }
        }

        private void MailBoxItemDropMessages(MailBoxItem item)
        {
            ViewModel.MailBoxItemDropMessages(item);
        }

        private bool IsDropMessagesAllowed(MailBoxItem item)
        {
            return ViewModel.MailBoxItemIsDropMessagesAllowed(item);
        }

        public void SetMessageService(IMessageService messageService)
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception ex, bool silent = false)
        {
            if (!silent)
            {
                ViewModel.OnError(ex);
            }
        }

        private void MailBoxItemClick(MailBoxItem mailBoxItem)
        {
            WeakReferenceMessenger.Default.Send(new ClearSelectedContactMessage());
            contentFrame.Navigate(typeof(FolderMessagesPage), new FolderMessagesPageViewModel.NavigationData() { MailBoxItem = mailBoxItem, ErrorHandler = this });
        }

        private void OnContactSelected(object recipient, ContactSelectedMessage message)
        {
            ViewModel.MailBoxesModel.SelectedItem = null;
            contentFrame.Navigate(typeof(ContactMessagesPage), new ContactMessagesPageViewModel.NavigationData() { ContactItem = message.Value, ErrorHandler = this });
        }

        public override void HandleBack()
        {
            // Do nothing, disable back navigation on main page
        }
    }
}
