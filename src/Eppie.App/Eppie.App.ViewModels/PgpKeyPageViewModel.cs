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
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;
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

        public ICommand DeletePgpKeyCommand => new AsyncRelayCommand(DeleteKeyAsync);

        public override void OnNavigatedTo(object data)
        {
            if (data is PgpKeyInfo key)
            {
                SetKey(key);
            }
        }

        public void SetKey(PgpKeyInfo keyInfo)
        {
            if (keyInfo != null)
            {
                key = keyInfo;
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

        private async Task DeleteKeyAsync()
        {
            try
            {
                bool userConfirmed = await MessageService.ShowRemovePgpKeyDialogAsync().ConfigureAwait(true);

                if (userConfirmed)
                {
                    var emailAddress = new EmailAddress(key.UserIdentity);
                    Core.GetSecurityManager().RemovePgpKeys(emailAddress);

                    NavigationService?.GoBack();
                }
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
