using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Services;
using TuviPgpLib.Entities;

namespace Tuvi.App.ViewModels
{
    public class PgpKeysPageViewModel : BaseViewModel
    {
        private List<PgpKeyInfo> pgpKeys = new List<PgpKeyInfo>();
        public List<PgpKeyInfo> PgpKeys
        {
            get { return pgpKeys; }
            private set { SetProperty(ref pgpKeys, value); }
        }

        public ICommand BackCommand => new RelayCommand(() => NavigationService?.GoBack());

        public ICommand PgpKeyDetailsCommand => new RelayCommand<object>(OpenPgpKeyDetails);

        public ICommand LoadFilesCommand => new AsyncRelayCommand<IFileOperationProvider>(ImportPgpKeyFromFileAsync);

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                PgpKeys = (await GetPublicKeysAsync().ConfigureAwait(true)).ToList();
            }
            catch (Exception exception)
            {
                OnError(exception);
            }
        }

        private void OpenPgpKeyDetails(object item)
        {
            if (item is PgpKeyInfo keyInformation)
            {
                NavigationService?.Navigate(nameof(PgpKeyPageViewModel), keyInformation);
            }
        }

        private async Task ImportPgpKeyFromFileAsync(IFileOperationProvider fileProvider)
        {
            try
            {
                var keyFiles = await fileProvider.LoadFilesAsync().ConfigureAwait(true);

                bool isKeyImported = false;
                foreach (var keyFile in keyFiles)
                {
                    if (await TryImportKeyAsync(keyFile.Data, keyFile.Name).ConfigureAwait(true))
                    {
                        isKeyImported = true;
                    }
                }

                if (isKeyImported)
                {
                    PgpKeys = (await GetPublicKeysAsync().ConfigureAwait(true)).ToList();

                    await BackupIfNeededAsync().ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task<bool> TryImportKeyAsync(byte[] keyData, string fileName)
        {
            try
            {
                await ImportKeyAsync(keyData).ConfigureAwait(true);
                return true;
            }
            catch (PublicKeyAlreadyExistException)
            {
                await MessageService.ShowPgpPublicKeyAlreadyExistMessageAsync(fileName).ConfigureAwait(true);
            }
            catch (UnknownPublicKeyAlgorithmException)
            {
                await MessageService.ShowPgpUnknownPublicKeyAlgorithmMessageAsync(fileName).ConfigureAwait(true);
            }
            catch (ImportPublicKeyException exception)
            {
                await MessageService.ShowPgpPublicKeyImportErrorMessageAsync(exception.Message, fileName).ConfigureAwait(true);
            }

            return false;
        }

        private Task<ICollection<PgpKeyInfo>> GetPublicKeysAsync()
        {
            return Task.Run(() => Core.GetSecurityManager().GetPublicPgpKeysInfo());
        }

        private Task ImportKeyAsync(byte[] keyData)
        {
            return Task.Run(() =>
            {
                using (var stream = new MemoryStream(keyData))
                {
                    Core.GetSecurityManager().ImportPgpKeyRingBundle(stream);
                }
            });
        }
    }
}
