using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using TuviPgpLib.Entities;

namespace Tuvi.App.ViewModels
{
    public class PgpKeyPageViewModel : BaseViewModel
    {
        private PgpKeyInfo key;

        private string userIdentity;
        public string UserIdentity
        {
            get { return userIdentity; }
            set { SetProperty(ref userIdentity, value); }
        }

        private string fingerprint;
        public string Fingerprint
        {
            get { return fingerprint; }
            set { SetProperty(ref fingerprint, value); }
        }

        private string keyId;
        public string KeyId
        {
            get { return keyId; }
            set { SetProperty(ref keyId, value); }
        }

        private string algorithm;
        public string Algorithm
        {
            get { return algorithm; }
            set { SetProperty(ref algorithm, value); }
        }

        private string created;
        public string Created
        {
            get { return created; }
            set { SetProperty(ref created, value); }
        }

        private string expires;
        public string Expires
        {
            get { return expires; }
            set { SetProperty(ref expires, value); }
        }

        public ICommand BackCommand => new RelayCommand(() => NavigationService?.GoBack());

        public ICommand CopyPgpKeyCommand => new AsyncRelayCommand<IClipboardProvider>(CopyKeyToClipboardAsync);

        public ICommand ExportPgpKeyCommand => new AsyncRelayCommand<IFileOperationProvider>(ExportKeyToFileAsync);

        public ICommand SendPgpKeyCommand => new AsyncRelayCommand(SendKeyByEmailAsync);

        public override void OnNavigatedTo(object data)
        {
            if (data is PgpKeyInfo key)
            {
                this.key = key;
                SetKeyInformation();
            }
        }

        private void SetKeyInformation()
        {
            UserIdentity = key.UserIdentity;
            Fingerprint = key.Fingerprint;
            KeyId = key.KeyId.ToString("X2", CultureInfo.CurrentCulture);
            Algorithm = $"{key.Algorithm} {key.BitStrength} bit";
            Created = key.CreationTime.ToString("F", CultureInfo.CurrentCulture);
            if (key.IsNeverExpires())
            {
                Expires = GetLocalizedString("PgpKeyExpiresNever");
            }
            else
            {
                TimeSpan expiresIn = TimeSpan.FromSeconds(key.ValidSeconds);
                DateTime expiresOn = key.CreationTime.Add(expiresIn);
                Expires = expiresOn.ToString("F", CultureInfo.CurrentCulture);
            }
        }

        private async Task CopyKeyToClipboardAsync(IClipboardProvider clipboard)
        {
            try
            {
                string armoredKey = await GetArmoredKeyTextAsync().ConfigureAwait(true);
                clipboard.SetClipboardContent(armoredKey);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task ExportKeyToFileAsync(IFileOperationProvider fileOperations)
        {
            try
            {
                byte[] fileContent = await GetArmoredKeyBytesAsync().ConfigureAwait(true);
                string defaultFileName = GetDefaultKeyFileName();

                await fileOperations.SaveToFileAsync(fileContent, defaultFileName).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task SendKeyByEmailAsync()
        {
            try
            {
                byte[] fileContent = await GetArmoredKeyBytesAsync().ConfigureAwait(true);
                string defaultFileName = GetDefaultKeyFileName();
                string defaultMessageSubject = GetLocalizedString("PgpPublicKeyShareMessageSubject");

                var shareKeyMessageData = new SharePublicKeyMessageData(key.UserIdentity, fileContent, defaultFileName, defaultMessageSubject);
                NavigationService?.Navigate(nameof(NewMessagePageViewModel), shareKeyMessageData);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task<string> GetArmoredKeyTextAsync()
        {
            using (var keyDataStream = new MemoryStream())
            {
                await Core.GetSecurityManager().ExportPgpKeyRingAsync(key.KeyId, keyDataStream).ConfigureAwait(true);
                keyDataStream.Position = 0;

                using (var textReader = new StreamReader(keyDataStream))
                {
                    string armoredKeyText = textReader.ReadToEnd();
                    return armoredKeyText;
                }
            }
        }

        private async Task<byte[]> GetArmoredKeyBytesAsync()
        {
            using (var keyDataStream = new MemoryStream())
            {
                await Core.GetSecurityManager().ExportPgpKeyRingAsync(key.KeyId, keyDataStream).ConfigureAwait(true);
                return keyDataStream.ToArray();
            }
        }

        private string GetDefaultKeyFileName()
        {
            return $"{key.Fingerprint}_public.asc";
        }
    }
}
