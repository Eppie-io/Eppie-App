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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tuvi.App.ViewModels.Services;

namespace Tuvi.App.ViewModels
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private bool _isRestartMessageVisible;
        public bool IsRestartMessageVisible
        {
            get { return _isRestartMessageVisible; }
            set { SetProperty(ref _isRestartMessageVisible, value); }
        }

        private string _restartMessage = string.Empty;
        public string RestartMessage
        {
            get { return _restartMessage; }
            set { SetProperty(ref _restartMessage, value); }
        }

        public IReadOnlyList<string> ManifestLanguages => LocalizationService.ManifestLanguages;

        public ICommand ChangeMasterPasswordCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(PasswordPageViewModel), PasswordActions.ChangePassword));

        public ICommand ExportBackupCommand => new AsyncRelayCommand<IFileOperationProvider>(ExportBackupToFileAsync);

        public ICommand ImportBackupCommand => new AsyncRelayCommand<IFileOperationProvider>(ImportBackupFromFileAsync);

        public ICommand WipeApplicationDataCommand => new AsyncRelayCommand(WipeApplicationDataAsync);

        public ICommand OpenLogFolderCommand => new AsyncRelayCommand(OpenLogFolderAsync);

        //TODO: Issue #840 - Add logic
        public ICommand ConfirmSeedPhraseCommand => new RelayCommand(() => OnError(new NotImplementedException()));

        //TODO: Issue #840 - Add logic
        public ICommand ImportEncryptionKeyCommand => new RelayCommand(() => OnError(new NotImplementedException()));

        private bool _isLaunchPasswordEnabled = true;
        //TODO: Issue #840 - Add logic
        public bool IsLaunchPasswordEnabled
        {
            get => _isLaunchPasswordEnabled;
            set
            {
                if (value != _isLaunchPasswordEnabled)
                {
                    OnError(new NotImplementedException());
                    OnPropertyChanged();
                }
            }
        }

        private int _themeSelectedIndex;
        public int ThemeSelectedIndex
        {
            get => _themeSelectedIndex;
            set
            {
                if (value != _themeSelectedIndex)
                {
                    SetProperty(ref _themeSelectedIndex, value);
                    ApplyTheme(value);
                }
            }
        }

        override public void OnNavigatedTo(object data)
        {
            base.OnNavigatedTo(data);

            InitializeTheme();

            if (LocalSettingsService != null)
            {
                LocalSettingsService.SettingChanged += LocalSettingsService_SettingChanged;
            }
        }

        override public void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();

            if (LocalSettingsService != null)
            {
                LocalSettingsService.SettingChanged -= LocalSettingsService_SettingChanged;
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

        public void ChangeLanguage(string language)
        {
            if (LocalSettingsService.Language != language)
            {
                LocalSettingsService.Language = language;

                ShowRestartMessage();
            }
        }

        private void ShowRestartMessage()
        {
            var message = GetLocalizedString("RestartApplication");
            IsRestartMessageVisible = true;

            var brandName = BrandService.GetName();
            RestartMessage = string.Format(message, brandName);
        }

        public IReadOnlyList<LogLevel> LogLevels { get; } = Enum.GetValues(typeof(LogLevel))
                                                             .Cast<LogLevel>()
                                                             .ToList();

        public LogLevel SelectedLogLevel
        {
            get => LocalSettingsService.LogLevel;
            set
            {
                if (LocalSettingsService.LogLevel != value)
                {
                    LocalSettingsService.LogLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private Task OpenLogFolderAsync()
        {
            return LauncherService.OpenFolderAsync(LocalSettingsService.LogFolderPath);
        }

        public string LogFolder => LocalSettingsService.LogFolderPath;

        private void LocalSettingsService_SettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (args.Name == nameof(LocalSettingsService.LogLevel))
            {
                OnPropertyChanged(nameof(SelectedLogLevel));
            }
        }

        private void InitializeTheme()
        {
            if (LocalSettingsService != null)
            {
                _themeSelectedIndex = (int)LocalSettingsService.Theme;
                OnPropertyChanged(nameof(ThemeSelectedIndex));
            }
        }

        private void ApplyTheme(int selectedIndex)
        {
            if (LocalSettingsService != null)
            {
                if (!Enum.IsDefined(typeof(AppTheme), selectedIndex))
                {
                    return;
                }

                AppTheme theme = (AppTheme)selectedIndex;

                if (LocalSettingsService.Theme != theme)
                {
                    LocalSettingsService.Theme = theme;
                }
            }
        }

    }
}
