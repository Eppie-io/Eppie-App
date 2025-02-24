using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Services;

namespace Tuvi.App.ViewModels
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private bool _showRestartMessage;
        public bool ShowRestartMessage
        {
            get { return _showRestartMessage; }
            set { SetProperty(ref _showRestartMessage, value); }
        }

        private string _restartMessage = string.Empty;
        public string RestartMessage
        {
            get { return _restartMessage; }
            set { SetProperty(ref _restartMessage, value); }
        }

        private bool _isEnableAIButtonVisible;
        public bool IsEnableAIButtonVisible
        {
            get { return _isEnableAIButtonVisible; }
            set { SetProperty(ref _isEnableAIButtonVisible, value); }
        }

        private bool _isDisableAIButtonVisible;
        public bool IsDisableAIButtonVisible
        {
            get { return _isDisableAIButtonVisible; }
            set { SetProperty(ref _isDisableAIButtonVisible, value); }
        }

        private bool _isAIProgressRingVisible;
        public bool IsAIProgressRingVisible
        {
            get { return _isAIProgressRingVisible; }
            set { SetProperty(ref _isAIProgressRingVisible, value); }
        }

        public ICommand PgpKeysCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(PgpKeysPageViewModel)));

        public ICommand ChangeMasterPasswordCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(PasswordPageViewModel), PasswordActions.ChangePassword));

        public ICommand ConfirmSeedPhraseCommand => new RelayCommand(() => throw new NotImplementedException());

        public ICommand ExportBackupCommand => new AsyncRelayCommand<IFileOperationProvider>(ExportBackupToFileAsync);

        public ICommand ImportBackupCommand => new AsyncRelayCommand<IFileOperationProvider>(ImportBackupFromFileAsync);

        public ICommand WipeApplicationDataCommand => new AsyncRelayCommand(WipeApplicationDataAsync);

        public ICommand EnableAICommand => new AsyncRelayCommand(EnableAIAsync);

        public ICommand DisableAICommand => new AsyncRelayCommand(DisableAIAsync);

        override public async void OnNavigatedTo(object data)
        {
            try
            {
                base.OnNavigatedTo(data);
                await ToggleAIButtons();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task WipeApplicationDataAsync()
        {
            try
            {
                bool isConfirmed = await MessageService.ShowWipeAllDataDialogAsync().ConfigureAwait(true);

                if (isConfirmed)
                {
                    await Core.ResetApplicationAsync().ConfigureAwait(true);
                    NavigationService?.Navigate(nameof(WelcomePageViewModel));
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task ExportBackupToFileAsync(IFileOperationProvider fileOperations)
        {
            try
            {
                using (var buffer = new MemoryStream())
                {
                    await Core.GetBackupManager().CreateBackupAsync(buffer).ConfigureAwait(true);

                    byte[] fileContent = buffer.ToArray();
                    string defaultFileName = GetDefaultBackupFileName();

                    await fileOperations.SaveToFileAsync(fileContent, defaultFileName).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task ImportBackupFromFileAsync(IFileOperationProvider fileProvider)
        {
            try
            {
                var files = await fileProvider.LoadFilesAsync().ConfigureAwait(true);

                var backupFile = files?.FirstOrDefault();

                if (backupFile != null)
                {
                    using (var buffer = new MemoryStream(backupFile.Data))
                    {
                        await Core.GetBackupManager().RestoreBackupAsync(buffer).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private string GetDefaultBackupFileName()
        {
            string appName = BrandService.GetName();
            return $"{appName} backup {DateTime.Now:D}.bak";
        }

        public void ChangeLanguage(string language, string message)
        {
            if (LocalSettingsService.Language != language)
            {
                LocalSettingsService.Language = language;

                ShowRestartMessage = true;

                var brandName = BrandService.GetName();
                RestartMessage = string.Format(message, brandName);
            }
        }

        public async Task DisableAIAsync()
        {
            ShowProgressRing();
            try
            {
                await AIService.DeleteModelAsync();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                HideProgressRing();
                await ToggleAIButtons();
            }
        }

        public async Task EnableAIAsync()
        {
            ShowProgressRing();
            try
            {
                await AIService.ImportModelAsync();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                HideProgressRing();
                await ToggleAIButtons();
            }
        }

        private void ShowProgressRing()
        {
            IsAIProgressRingVisible = true;

            IsEnableAIButtonVisible = false;
            IsDisableAIButtonVisible = false;
        }

        private void HideProgressRing()
        {
            IsAIProgressRingVisible = false;
        }

        private async Task ToggleAIButtons()
        {
            if (await AIService.IsEnabledAsync())
            {
                IsEnableAIButtonVisible = false;
                IsDisableAIButtonVisible = true;
            }
            else
            {
                IsEnableAIButtonVisible = true;
                IsDisableAIButtonVisible = false;
            }
        }
    }
}
