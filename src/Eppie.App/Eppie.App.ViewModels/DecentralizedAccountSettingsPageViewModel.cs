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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EmailValidation;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class Network
    {
        public string Name { get; }
        public NetworkType NetworkType { get; }

        public Network(string name, NetworkType networkType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            NetworkType = networkType;
        }

        public override string ToString() => Name;
    }

    public class DecentralizedAccountSettingsModel : BaseAccountSettingsModel
    {
        private ObservableCollection<Network> _networkOptions;
        public ObservableCollection<Network> NetworkOptions
        {
            get => _networkOptions;
            private set => SetProperty(ref _networkOptions, value);
        }

        private Network _selectedNetwork;
        public Network SelectedNetwork
        {
            get => _selectedNetwork;
            set
            {
                SetProperty(ref _selectedNetwork, value);
                OnPropertyChanged(nameof(IsSecretKeyVisible));
                OnPropertyChanged(nameof(IsSenderNameVisible));
            }
        }

        private bool _isNetworkLocked;
        public bool IsNetworkLocked
        {
            get => _isNetworkLocked;
            set => SetProperty(ref _isNetworkLocked, value);
        }

        // TODO: Zeroize WIF key after using it
        private string _secretKeyWIF;
        public string SecretKeyWIF
        {
            get => _secretKeyWIF;
            set => SetProperty(ref _secretKeyWIF, value);
        }

        public bool IsSecretKeyVisible
        {
            get => SelectedNetwork.NetworkType == NetworkType.Bitcoin;
        }

        public bool IsSenderNameVisible
        {
            get => SelectedNetwork.NetworkType == NetworkType.Eppie && !CurrentAccount.Email.IsHybrid;
        }

        private string _claimedName;
        public string ClaimedName
        {
            get => _claimedName;
            set => SetProperty(ref _claimedName, value);
        }

        protected DecentralizedAccountSettingsModel(Account account) : base(account)
        {
            _networkOptions = new ObservableCollection<Network>
            {
                new Network("Eppie Testnet", NetworkType.Eppie),
                new Network("Bitcoin Testnet", NetworkType.Bitcoin)
            };

            _selectedNetwork = _networkOptions[0];

            UpdateAccount(account);
        }

        public virtual Account ToAccount()
        {
            if (Email.Value is null)
            {
                return null;
            }

            if (IsSenderNameVisible && !string.IsNullOrEmpty(ClaimedName))
            {
                CurrentAccount.Email = new EmailAddress(CurrentAccount.Email.Address, ClaimedName);
            }

            CurrentAccount.IsBackupAccountSettingsEnabled = true;
            CurrentAccount.IsBackupAccountMessagesEnabled = true;
            CurrentAccount.SynchronizationInterval = int.TryParse(SynchronizationInterval.Value, out int interval)
                ? interval
                : DefaultSynchronizationInterval;
            CurrentAccount.Type = MailBoxType.Dec;

            return CurrentAccount;
        }

        public static DecentralizedAccountSettingsModel Create(Account account)
        {
            return new DecentralizedAccountSettingsModel(account);
        }

        public void UpdateAccount(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            CurrentAccount = new Account
            {
                Id = account.Id,
                Email = account.Email,
                IsBackupAccountSettingsEnabled = account.IsBackupAccountSettingsEnabled,
                IsBackupAccountMessagesEnabled = account.IsBackupAccountMessagesEnabled,
                SynchronizationInterval = account.SynchronizationInterval,
                Type = account.Type,
                DecentralizedAccountIndex = account.DecentralizedAccountIndex
            };

            Email.Value = account.Email.DisplayAddress;
            SenderName.Value = account.Email.Name;
            ClaimedName = account.Email.Name;
            SelectedNetwork = NetworkOptions.FirstOrDefault(x => x.NetworkType == account.Email.Network) ?? NetworkOptions.First();
        }
    }

    public class DecentralizedAccountSettingsPageViewModel : BaseAccountSettingsPageViewModel
    {
        private DecentralizedAccountSettingsModel _accountSettingsModel;
        public DecentralizedAccountSettingsModel AccountSettingsModel
        {
            get => _accountSettingsModel;
            set
            {
                if (_accountSettingsModel != null)
                {
                    _accountSettingsModel.PropertyChanged -= OnAccountSettingsModelPropertyChanged;
                }

                SetProperty(ref _accountSettingsModel, value, true);

                if (_accountSettingsModel != null)
                {
                    _accountSettingsModel.PropertyChanged += OnAccountSettingsModelPropertyChanged;
                }

                ClaimNameCommand?.NotifyCanExecuteChanged();
            }
        }

        public IRelayCommand JoinWaitingListCommand => new RelayCommand(DoJoinWaitingList);

        public IRelayCommand ClaimNameCommand { get; }

        private bool _isClaimingName;
        public bool IsClaimingName
        {
            get => _isClaimingName;
            private set
            {
                SetProperty(ref _isClaimingName, value);
                ClaimNameCommand.NotifyCanExecuteChanged();
            }
        }


        public DecentralizedAccountSettingsPageViewModel()
        {
            ClaimNameCommand = new AsyncRelayCommand(ClaimNameAsync, CanClaimExecute);
            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        private bool CanClaimExecute()
        {
            if (_accountSettingsModel is null || _isClaimingName || IsWaitingResponse || !_accountSettingsModel.IsSenderNameVisible)
            {
                return false;
            }

            var desired = _accountSettingsModel.SenderName.Value;
            if (string.IsNullOrWhiteSpace(desired))
            {
                return false;
            }

            return true;
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                await InitModel(data).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task InitModel(object data)
        {
            if (data is Account accountData)
            {
                IsCreatingAccountMode = false;
                AccountSettingsModel = DecentralizedAccountSettingsModel.Create(accountData);
                AccountSettingsModel.IsNetworkLocked = !IsCreatingAccountMode;

                if (accountData.Email.Network == NetworkType.Bitcoin)
                {
                    AccountSettingsModel.SecretKeyWIF = await Core.GetSecurityManager().GetSecretKeyWIFAsync(accountData).ConfigureAwait(true);
                }
            }
            else
            {
                IsCreatingAccountMode = true;
                accountData = await CreateDecentralizedAccountAsync(NetworkType.Eppie, default).ConfigureAwait(true);
                AccountSettingsModel = DecentralizedAccountSettingsModel.Create(accountData);
                AccountSettingsModel.IsNetworkLocked = !IsCreatingAccountMode;
            }
        }

        public async Task<Account> CreateDecentralizedAccountAsync(NetworkType networkType, CancellationToken cancellationToken)
        {
            var (publicKey, accountIndex) = await Core.GetSecurityManager().GetNextDecAccountPublicKeyAsync(networkType, cancellationToken).ConfigureAwait(true);

            var email = EmailAddress.CreateDecentralizedAddress(networkType, publicKey);

            return new Account()
            {
                Email = email,
                IsBackupAccountSettingsEnabled = true,
                IsBackupAccountMessagesEnabled = true,
                Type = MailBoxType.Dec,
                DecentralizedAccountIndex = accountIndex
            };
        }

        protected override Account AccountSettingsModelToAccount()
        {
            return AccountSettingsModel.ToAccount();
        }

        private async void DoJoinWaitingList()
        {
            try
            {
                await LauncherService.LaunchAsync(new Uri(BrandService.GetHomepage())).ConfigureAwait(true);
                GoBack();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async void OnAccountSettingsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DecentralizedAccountSettingsModel.SenderName))
            {
                ClaimNameCommand?.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(DecentralizedAccountSettingsModel.SelectedNetwork) && !_accountSettingsModel.IsNetworkLocked)
            {
                try
                {
                    _accountSettingsModel.IsNetworkLocked = true;
                    var accountData = await CreateDecentralizedAccountAsync(AccountSettingsModel.SelectedNetwork.NetworkType, default).ConfigureAwait(true);
                    AccountSettingsModel.UpdateAccount(accountData);
                    if (AccountSettingsModel.SelectedNetwork.NetworkType == NetworkType.Bitcoin)
                    {
                        AccountSettingsModel.SecretKeyWIF = await Core.GetSecurityManager().GetSecretKeyWIFAsync(accountData).ConfigureAwait(true);
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                finally
                {
                    _accountSettingsModel.IsNetworkLocked = false;
                    ClaimNameCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private async Task ClaimNameAsync()
        {
            if (AccountSettingsModel == null || !AccountSettingsModel.IsSenderNameVisible)
            {
                return;
            }

            var nameRaw = AccountSettingsModel.SenderName.Value?.Trim();
            AccountSettingsModel.SenderName.Errors.Clear();

            if (!ValidateName(nameRaw, out var normalized, out var errorKey))
            {
                AccountSettingsModel.SenderName.Errors.Add(GetLocalizedString(errorKey));
                return;
            }

            try
            {
                IsClaimingName = true;
                var account = AccountSettingsModel.ToAccount();
                var result = await Core.ClaimDecentralizedNameAsync(normalized, account.Email).ConfigureAwait(true);
                if (result)
                {
                    OnNameClaimSucceededAsync(normalized, account);
                }
                else
                {
                    AccountSettingsModel.SenderName.Errors.Add(GetLocalizedString("NameAlreadyTakenError"));
                }
            }
            catch (Exception ex)
            {
                AccountSettingsModel.SenderName.Errors.Add(GetLocalizedString("ClaimNameFailedError"));
                OnError(ex);
            }
            finally
            {
                IsClaimingName = false;
                ClaimNameCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnNameClaimSucceededAsync(string normalized, Account account)
        {
            AccountSettingsModel.ClaimedName = normalized;
            AccountSettingsModel.SenderName.Value = normalized;
            AccountSettingsModel.Email.Value = EmailAddress.CreateDecentralizedAddress(account.Email.Network, normalized).DisplayAddress;
        }

        private static bool ValidateName(string name, out string normalized, out string errorKey)
        {
            normalized = string.Empty;
            errorKey = string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                errorKey = "NameIsEmptyError";
                return false;
            }

            if (name.IndexOf('+') >= 0)
            {
                errorKey = "InvalidNameError";
                return false;
            }
            name = name.ToLowerInvariant();

            var syntheticEmail = EmailAddress.CreateDecentralizedAddress(NetworkType.Eppie, name);
            if (!EmailValidator.Validate(syntheticEmail.Address, allowTopLevelDomains: true))
            {
                errorKey = "InvalidNameError";
                return false;
            }

            normalized = name;
            return true;
        }
    }
}
