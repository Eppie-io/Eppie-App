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
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Eppie.App.UI.Tools;
using Microsoft.UI.Xaml.Controls;
using Tuvi.App.Shared.Extensions;
using Tuvi.App.Shared.Helpers;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using Windows.Storage;
using Windows.Storage.Pickers;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml.Navigation;
#endif

namespace Tuvi.App.Shared.Views
{
    public partial class MainPageBase : BasePage<MainPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class MainPage : MainPageBase, IErrorHandler
    {
        public ICommand ShowAllMessagesCommand => new RelayCommand(ShowAllMessages);

        public ICommand MailBoxItemClickCommand => new RelayCommand<MailBoxItem>(MailBoxItemClick);

        public ICommand MailBoxItemDropCommand => new RelayCommand<MailBoxItem>(MailBoxItemDropMessages, IsDropMessagesAllowed);

        public ICommand ContactItemClickCommand => new RelayCommand<ContactItem>(ContactItemClick);

        public ICommand RenameContactCommand => new AsyncRelayCommand<ContactItem>(RenameContactAsync);

        public ICommand ChangeContactPictureCommand => new AsyncRelayCommand<ContactItem>(ChangeContactPictureAsync);

        public ICommand ShowAboutPageCommand => new RelayCommand(ShowAboutPage);

        public ICommand OpenAddressManagerCommand => new RelayCommand(ShowAddressManagerPane);

        public ICommand OpenContactsPanelCommand => new RelayCommand(ToggleContactsPanelPane);

        public ICommand OpenMailboxesPanelCommand => new RelayCommand(ToggleMailboxesPanelPane);

        public ICommand OpenAIAgentsPanelCommand => new RelayCommand(ToggleAIAgentsPane);

        public ICommand ClosePaneCommand => new RelayCommand(ClosePane);

        public MainPage()
        {
            this.InitializeComponent();

            ViewModel.InitializeModels(ContactItemClickCommand, RenameContactCommand, ChangeContactPictureCommand, MailBoxItemClickCommand, MailBoxItemDropCommand);

            NavigationMenu.PaneOpened += OnNavigationPaneToggled;
            NavigationMenu.PaneClosed += OnNavigationPaneToggled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                ShowAllMessagesCommand.Execute(this);

                var app = Eppie.App.Shared.App.Current as Eppie.App.Shared.App;
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
            var app = Eppie.App.Shared.App.Current as Eppie.App.Shared.App;
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
                    paneFrame.Navigate(typeof(ContactsPanelPage), ViewModel.ContactsModel);
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
            var app = Eppie.App.Shared.App.Current as Eppie.App.Shared.App;
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

        private async Task RenameContactAsync(ContactItem contactItem)
        {
            await ViewModel.RenameContactAsync(contactItem).ConfigureAwait(true);
        }

        private async Task ChangeContactPictureAsync(ContactItem contactItem)
        {
            try
            {
                FileOpenPicker fileOpenPicker = FileOpenPickerBuilder.CreateBuilder(Eppie.App.Shared.App.MainWindow)
                                                                     .Configure((picker) =>
                                                                     {
                                                                         picker.ViewMode = PickerViewMode.Thumbnail;
                                                                         picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                                                                         picker.FileTypeFilter.Add(".jpg");
                                                                         picker.FileTypeFilter.Add(".jpeg");
                                                                         picker.FileTypeFilter.Add(".png");
                                                                         picker.FileTypeFilter.Add(".bmp");
                                                                     })
                                                                     .Build();

                StorageFile file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    var bitmapBytes = await BitmapTools.GetThumbnailPixelDataAsync(file, (uint)ContactItem.DefaultAvatarSize, (uint)ContactItem.DefaultAvatarSize).ConfigureAwait(true);
                    if (bitmapBytes != null)
                    {
                        await ViewModel.SetContactAvatarAsync(contactItem, bitmapBytes, ContactItem.DefaultAvatarSize, ContactItem.DefaultAvatarSize).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception e)
            {
                ViewModel.OnError(e);
            }
        }

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
            return ViewModel.MailBoxItem_IsDropMessagesAllowed(item);
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
            ViewModel.ContactsModel.SelectedContact = null;
            contentFrame.Navigate(typeof(FolderMessagesPage), new FolderMessagesPageViewModel.NavigationData() { MailBoxItem = mailBoxItem, ErrorHandler = this });
        }

        private void ContactItemClick(ContactItem contactItem)
        {
            ViewModel.MailBoxesModel.SelectedItem = null;
            contentFrame.Navigate(typeof(ContactMessagesPage), new ContactMessagesPageViewModel.NavigationData() { ContactItem = contactItem, ErrorHandler = this });
        }

        public override void HandleBack()
        {
            // Do nothing, disable back navigation on main page
        }

    }
}
