using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Tuvi.App.Shared.Extensions;
using Tuvi.App.Shared.Helpers;
using Tuvi.App.ViewModels;

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

    public sealed partial class MainPage : MainPageBase
    {
        public ICommand ShowAllMessagesCommand => new RelayCommand(ShowAllMessages);

        public ICommand MailBoxItemClickCommand => new RelayCommand<MailBoxItem>((mailBoxItem) => contentFrame.Navigate(typeof(FolderMessagesPage), mailBoxItem));

        public ICommand ContactItemClickCommand => new RelayCommand<ContactItem>((contactItem) => contentFrame.Navigate(typeof(ContactMessagesPage), contactItem));

        public ICommand ChangeContactPictureCommand => new AsyncRelayCommand<ContactItem>(ChangeContactPictureAsync);

        public ICommand ShowAboutPageCommand => new RelayCommand(ToggleAboutPane);

        public ICommand OpenIdentityManagerCommand => new RelayCommand(ToggleIdentityManagerPane);

        public ICommand ClosePaneCommand => new RelayCommand(ClosePane);

        public MainPage()
        {
            this.InitializeComponent();

            ViewModel.InitializeNavPanelTabModel(ContactItemClickCommand, ChangeContactPictureCommand, MailBoxItemClickCommand);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                ShowAllMessagesCommand.Execute(this);
            }
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // When returning to the main page, clicking on the Settings menu did not work because it was already selected
                NavigationMenu.SelectedItem = null;
                ToggleSettingsPane();
            }
            else if (args.InvokedItemContainer is DependencyObject dependencyObject)
            {
                AttachedCommands.GetClickCommand(dependencyObject)?.Execute(null);
            }
        }

        private bool _isAboutOpen;
        private void ToggleAboutPane()
        {
            if (splitView.IsPaneOpen && _isAboutOpen)
            {
                ClosePane();
            }
            else
            {
                splitView.IsPaneOpen = true;
                paneFrame.Navigate(typeof(AboutPage));
                _isAboutOpen = true;
                _isSettingsOpen = false;
                _isIdentityManagerOpen = false;
            }
        }

        private bool _isSettingsOpen;
        private void ToggleSettingsPane()
        {
            if (splitView.IsPaneOpen && _isSettingsOpen)
            {
                ClosePane();
            }
            else
            {
                splitView.IsPaneOpen = true;
                paneFrame.Navigate(typeof(SettingsPage));
                _isSettingsOpen = true;
                _isAboutOpen = false;
                _isIdentityManagerOpen = false;
            }
        }

        private bool _isIdentityManagerOpen;
        private void ToggleIdentityManagerPane()
        {
            if (splitView.IsPaneOpen && _isIdentityManagerOpen)
            {
                ClosePane();
            }
            else
            {
                splitView.IsPaneOpen = true;
                paneFrame.Navigate(typeof(IdentityManagerPage));
                _isIdentityManagerOpen = true;
                _isSettingsOpen = false;
                _isAboutOpen = false;
            }
        }

        private void ClosePane()
        {
            splitView.IsPaneOpen = false;
            _isAboutOpen = false;
            _isSettingsOpen = false;
            _isIdentityManagerOpen = false;
        }

        private void ShowAllMessages()
        {
            ViewModel.OnShowAllMessages();
            contentFrame.Navigate(typeof(AllMessagesPage));
        }

        private async Task ChangeContactPictureAsync(ContactItem contactItem)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".bmp");

                Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
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
    }
}