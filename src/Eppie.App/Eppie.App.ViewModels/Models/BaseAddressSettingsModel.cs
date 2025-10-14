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
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;
using TuviPgpLib.Entities;

namespace Tuvi.App.ViewModels
{
    public abstract class BaseAddressSettingsModel : ObservableObject
    {
        /// <summary>
        /// Default interval value (in minutes) for checking new messages
        /// </summary>
        protected const int DefaultSynchronizationInterval = 10;

        private Func<Tuvi.Core.ITuviMail> CoreProvider { get; set; }
        private Tuvi.Core.ITuviMail Core => CoreProvider != null ? CoreProvider() : null;

        private Services.INavigationService NavigationService { get; set; }

        protected Account CurrentAccount { get; set; }
        public ValidatableProperty<string> Email { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> SynchronizationInterval { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> SenderName { get; } = new ValidatableProperty<string>();

        private bool _isBackupAccountSettingsEnabled = true;
        public bool IsBackupAccountSettingsEnabled
        {
            get { return _isBackupAccountSettingsEnabled; }
            set
            {
                SetProperty(ref _isBackupAccountSettingsEnabled, value);

                if (!IsBackupAccountSettingsEnabled)
                {
                    IsBackupAccountMessagesEnabled = false;
                }
            }
        }

        private bool _isBackupAccountMessagesEnabled = true;
        public bool IsBackupAccountMessagesEnabled
        {
            get { return _isBackupAccountMessagesEnabled; }
            set { SetProperty(ref _isBackupAccountMessagesEnabled, value); }
        }

        private PgpKeyPageViewModel _pgpKeyModel;
        public PgpKeyPageViewModel PgpKeyModel
        {
            get { return _pgpKeyModel; }
            set { SetProperty(ref _pgpKeyModel, value); }
        }

        private string _messageFooter;
        public string MessageFooter
        {
            get { return _messageFooter; }
            set { SetProperty(ref _messageFooter, value); }
        }

        private bool _isMessageFooterEnabled;
        public bool IsMessageFooterEnabled
        {
            get { return _isMessageFooterEnabled; }
            set { SetProperty(ref _isMessageFooterEnabled, value); }
        }

        protected BaseAddressSettingsModel()
        {
            Email.SetInitialValue(string.Empty);
            SenderName.SetInitialValue(string.Empty);
            SynchronizationInterval.SetInitialValue(DefaultSynchronizationInterval.ToString());

            Email.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Email), args.PropertyName);
            };
            SenderName.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SenderName), args.PropertyName);
            };
            SynchronizationInterval.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SynchronizationInterval), args.PropertyName);
            };
        }

        protected BaseAddressSettingsModel(Account account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            CurrentAccount = account;

            if (account.Email != null)
            {
                Email.SetInitialValue(account.Email.Address);
                SenderName.SetInitialValue(account.Email.Name);
            }

            IsBackupAccountSettingsEnabled = account.IsBackupAccountSettingsEnabled;
            IsBackupAccountMessagesEnabled = account.IsBackupAccountMessagesEnabled;
            SynchronizationInterval.SetInitialValue(account.SynchronizationInterval.ToString());

            Email.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Email), args.PropertyName);
            };
            SenderName.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SenderName), args.PropertyName);
            };
            SynchronizationInterval.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SynchronizationInterval), args.PropertyName);
            };
        }

        public async Task InitModelAsync(Func<Tuvi.Core.ITuviMail> provider, Services.INavigationService navigationService)
        {
            CoreProvider = provider;
            NavigationService = navigationService;
            await InitializePgpKeyInfoAsync(CurrentAccount).ConfigureAwait(true);
        }

        private async Task InitializePgpKeyInfoAsync(Account account)
        {
            var email = account?.Email?.Address;
            if (string.IsNullOrEmpty(email) || Core is null)
            {
                return;
            }

            var keys = await Task.Run(() => Core.GetSecurityManager().GetPublicPgpKeysInfo()).ConfigureAwait(true);
            var matched = keys.Where(k => !string.IsNullOrEmpty(k.UserIdentity) && k.UserIdentity.IndexOf(email, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            var key = matched.FirstOrDefault(k => k.IsEncryptionKey) ?? matched.FirstOrDefault();

            SetPgpKeyInfo(key);
        }

        private void SetPgpKeyInfo(PgpKeyInfo keyInfo)
        {
            if (keyInfo is null)
            {
                PgpKeyModel = null;
                return;
            }

            if (PgpKeyModel is null)
            {
                PgpKeyModel = new PgpKeyPageViewModel();
                PgpKeyModel.SetCoreProvider(CoreProvider);
                PgpKeyModel.SetNavigationService(NavigationService);
            }

            PgpKeyModel.SetKey(keyInfo);
        }

        /// <summary>
        /// Handles ValidatableProperty changes
        /// </summary>
        /// <typeparam name="T">The type of ValidatableProperty</typeparam>
        /// <param name="validatablePropertyName">The name of the ValidatableProperty</param>
        /// <param name="propertyName">The name of the property that was changed inside the ValidatableProperty</param>
        protected void OnValidatablePropertyChanged<T>(string validatablePropertyName, string propertyName)
        {
            if (propertyName == nameof(ValidatableProperty<T>.Value) ||
                propertyName == nameof(ValidatableProperty<T>.NeedsValidation))
            {
                OnPropertyChanged(validatablePropertyName);
            }
        }
    }
}
