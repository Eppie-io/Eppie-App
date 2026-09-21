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
using Eppie.App.UI.Common;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Messages;
using Tuvi.App.ViewModels.Services;

#if WINDOWS_UWP
using Windows.UI.Xaml.Navigation;
using WinUI = Microsoft.UI.Xaml;
#else
using Microsoft.UI.Xaml.Navigation;
using WinUI = Microsoft.UI.Xaml;
#endif

namespace Eppie.App.Views
{
    internal partial class MainPageBase : BasePage<MainPageViewModel, BaseViewModel>
    {
    }

    internal sealed partial class MainPage : MainPageBase, IErrorHandler
    {
        public ICommand OpenComposeMessageCommand => new RelayCommand(OpenComposeMessage);

        public ICommand ShowAllMessagesCommand => new RelayCommand(ShowAllMessages);

        public ICommand ShowAppSettingsCommand => new RelayCommand(ShowAppSettings);

        public ICommand MailBoxItemClickCommand => new RelayCommand<MailBoxItem>(MailBoxItemClick);

        public ICommand MailBoxItemDropCommand => new RelayCommand<MailBoxItem>(MailBoxItemDropMessages, IsDropMessagesAllowed);

        public ICommand NewFolderCommand => new RelayCommand<MailBoxItem>(NewFolder);

        public ICommand MailboxSettingsCommand => new RelayCommand<MailBoxItem>(OpenMailboxSettings);

        public ICommand RemoveMailboxCommand => new RelayCommand<MailBoxItem>(RemoveMailbox);

        public ICommand RenameFolderCommand => new RelayCommand<MailBoxItem>(RenameFolder);

        public ICommand DeleteFolderCommand => new RelayCommand<MailBoxItem>(DeleteFolder);

        public ICommand ShowAboutPageCommand => new RelayCommand(ShowAboutPage);

        public ICommand ShowAddressManagerCommand => new RelayCommand(ShowAddressManager);

        public ICommand OpenContactsPanelCommand => new RelayCommand(OpenContacts);

        public ICommand OpenMailboxesPanelCommand => new RelayCommand(OpenMailboxes);

        public ICommand OpenAIAgentsPanelCommand => new RelayCommand(OpenAIAgents);

        public ICommand ToggleLeftPaneCommand => new RelayCommand(ToggleLeftPane);

        public ICommand OpenInviteDialogCommand => new RelayCommand(OpenInvitationDialog);

        public ICommand ClosePaneCommand => new RelayCommand(ClosePane);

        public MainPage()
        {
            this.InitializeComponent();

            ViewModel.InitializeMailboxModel(MailBoxItemClickCommand, MailBoxItemDropCommand);
            ViewModel.MailBoxesModel.NewFolderCommand = NewFolderCommand;
            ViewModel.MailBoxesModel.MailboxSettingsCommand = MailboxSettingsCommand;
            ViewModel.MailBoxesModel.RemoveMailboxCommand = RemoveMailboxCommand;
            ViewModel.MailBoxesModel.RenameFolderCommand = RenameFolderCommand;
            ViewModel.MailBoxesModel.DeleteFolderCommand = DeleteFolderCommand;

            WeakReferenceMessenger.Default.Register<ContactSelectedMessage>(this, OnContactSelected);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var app = App.Current as App;

            if (app?.NavigationService is Services.NavigationService navigationService)
            {
                navigationService.SetContentFrame(contentFrame);
            }

            if (e.NavigationMode != NavigationMode.Back)
            {
                ShowAllMessagesCommand.Execute(this);

                // Todo: Restore the state of the left panel from the last session. For now, always open the mailboxes panel.
                // Use LocalSettingsService to restore the last opened panel state and size.
                OpenMailboxes();

                OpenAddressManagerPaneIfNeeded();
            }
        }

        private void OnWhatsNewClose(object sender, EventArgs e)
        {
            WhatsNewFlyout.Hide();
        }

        private void ShowAboutPage()
        {
            ViewModel.ShowAbout();
        }

        private void OpenInvitationDialog()
        {
            ViewModel.OpenInvitationDialog();
        }

        private SidePaneKind _openedPane = SidePaneKind.None;
        private void ToggleLeftPane()
        {
            // Todo: Use LocalSettingsService to store the last opened panel state and size.
            LeftSidePane.IsPaneOpen = !LeftSidePane.IsPaneOpen;
        }


        private void OpenAIAgents()
        {
            splitView.IsPaneOpen = true;
            paneFrame.Navigate(typeof(AIAgentsManagerPage));
        }

        private void ShowAddressManager()
        {
            ViewModel.ShowAddressManager();
        }

        private void OpenContacts()
        {
            // Todo: Create UI component for ContactsPanelPage and add it to the left pane.
            // In Compact mode, the left pane should be hidden when an item is selected,
            // and the right pane should show corresponding content.
            LeftSidePane.IsPaneOpen = true;
            LeftSidePaneFrame.Navigate(typeof(ContactsPanelPage));
        }

        private void OpenMailboxes()
        {
            // Todo: Create UI component for MailboxesPanelPage and add it to the left pane.
            // In Compact mode, the left pane should be hidden when an item is selected,
            // and the right pane should show corresponding content.
            LeftSidePane.IsPaneOpen = true;
            LeftSidePaneFrame.Navigate(typeof(MailboxesPanelPage), ViewModel.MailBoxesModel);
            ViewModel.UpdateAccountsList();
        }

        private async void OpenAddressManagerPaneIfNeeded()
        {
            try
            {
                if (await ViewModel.IsAccountListEmptyAsync())
                {
                    ShowAddressManager();
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


        private void OpenComposeMessage()
        {
            _ = ViewModel.WriteNewMessageAsync();
        }

        private void ShowAllMessages()
        {
            ViewModel.ShowAllMessages(this);
        }

        private void ShowAppSettings()
        {
            ViewModel.ShowAppSettings();
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Event handler is referenced from XAML and must be an instance method.")]
        private void OnElementClearing(WinUI.Controls.ItemsRepeater sender, WinUI.Controls.ItemsRepeaterElementClearingEventArgs args)
        {
            if (args.Element is WinUI.Controls.InfoBar infoBar)
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
            ViewModel.ShowMailBoxMessages(mailBoxItem, this);
        }

        private void OnContactSelected(object recipient, ContactSelectedMessage message)
        {
            ViewModel.ShowContactMessages(message.Value, this);
        }

        private void NewFolder(MailBoxItem mailBoxItem)
        {
            if (mailBoxItem is null || !mailBoxItem.IsRootItem)
            {
                return;
            }

            var stringProvider = UI.Resources.StringProvider.GetInstance();
            _ = UITools.ShowTextInputDialogAsync(
                stringProvider.GetString("NewFolderDialogTitle"),
                stringProvider.GetString("NewFolderDialogPrimaryButtonText"),
                stringProvider.GetString("NewFolderDialogCloseButtonText"),
                stringProvider.GetString("NewFolderDialogTextBoxHeader"),
                string.Empty,
                this.XamlRoot,
                async (folderName) =>
                {
                    if (!string.IsNullOrWhiteSpace(folderName))
                    {
                        try
                        {
                            await ViewModel.CreateFolderAsync(mailBoxItem.Account.Email, folderName).ConfigureAwait(true);
                        }
                        catch (Exception ex)
                        {
                            OnError(ex);
                        }
                    }
                });
        }

        private async void OpenMailboxSettings(MailBoxItem mailBoxItem)
        {
            if (mailBoxItem is null || !mailBoxItem.IsRootItem)
            {
                return;
            }

            try
            {
                var account = await ViewModel.GetAccountAsync(mailBoxItem.Account.Email).ConfigureAwait(true);
                if (account != null)
                {
                    ViewModel.OpenMailboxSettings(account);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async void RemoveMailbox(MailBoxItem mailBoxItem)
        {
            if (mailBoxItem is null || !mailBoxItem.IsRootItem)
            {
                return;
            }

            try
            {
                var account = await ViewModel.GetAccountAsync(mailBoxItem.Account.Email).ConfigureAwait(true);
                if (account != null)
                {
                    await ViewModel.RemoveAccountAsync(account).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void RenameFolder(MailBoxItem mailBoxItem)
        {
            if (mailBoxItem is null || mailBoxItem.IsRootItem || mailBoxItem.Folder is null)
            {
                return;
            }

            var stringProvider = UI.Resources.StringProvider.GetInstance();
            _ = UITools.ShowTextInputDialogAsync(
                stringProvider.GetString("RenameFolderDialogTitle"),
                stringProvider.GetString("RenameFolderDialogPrimaryButtonText"),
                stringProvider.GetString("RenameFolderDialogCloseButtonText"),
                stringProvider.GetString("RenameFolderDialogTextBoxHeader"),
                mailBoxItem.Text,
                this.XamlRoot,
                async (newFolderName) =>
                {
                    if (!string.IsNullOrWhiteSpace(newFolderName) && newFolderName != mailBoxItem.Text)
                    {
                        try
                        {
                            // For now, only support simple folders renaming
                            if (mailBoxItem.Folder.Folders.Count == 1)
                            {
                                await ViewModel.RenameFolderAsync(mailBoxItem.Account.Email, mailBoxItem.Folder.Folders[0], newFolderName).ConfigureAwait(true);

                                ShowAllMessages();
                            }
                            else
                            {
                                throw new NotImplementedException("Only simple folders renaming is supported.");
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError(ex);
                        }
                    }
                });
        }

        private async void DeleteFolder(MailBoxItem mailBoxItem)
        {
            if (mailBoxItem is null || mailBoxItem.IsRootItem || mailBoxItem.Folder is null)
            {
                return;
            }

            try
            {
                // For now, only support simple folders deletion.
                if (mailBoxItem.Folder.Folders.Count == 1)
                {
                    await ViewModel.DeleteFolderAsync(mailBoxItem.Account.Email, mailBoxItem.Folder.Folders[0]).ConfigureAwait(true);

                    ShowAllMessages();
                }
                else
                {
                    throw new NotImplementedException("Only simple folders deletion is supported.");
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public override void HandleBack()
        {
            // Do nothing, disable back navigation on main page
        }
    }
}
