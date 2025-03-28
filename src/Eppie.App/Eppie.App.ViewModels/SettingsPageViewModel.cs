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

        public ICommand PgpKeysCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(PgpKeysPageViewModel)));

        public ICommand ChangeMasterPasswordCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(PasswordPageViewModel), PasswordActions.ChangePassword));

        public ICommand ConfirmSeedPhraseCommand => new RelayCommand(() => throw new NotImplementedException());

        public ICommand ExportBackupCommand => new AsyncRelayCommand<IFileOperationProvider>(ExportBackupToFileAsync);

        public ICommand ImportBackupCommand => new AsyncRelayCommand<IFileOperationProvider>(ImportBackupFromFileAsync);

        public ICommand WipeApplicationDataCommand => new AsyncRelayCommand(WipeApplicationDataAsync);

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
    }
}
