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

    public class DecentralizedAddressSettingsModel : BaseAddressSettingsModel
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

        protected DecentralizedAddressSettingsModel(Account account) : base(account)
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

        public static DecentralizedAddressSettingsModel Create(Account account)
        {
            return new DecentralizedAddressSettingsModel(account);
        }

        public void UpdateAccount(Account account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            CurrentAccount = account;

            Email.Value = account.Email.DisplayAddress;
            SenderName.Value = account.Email.Name;
            ClaimedName = account.Email.Name;
            SelectedNetwork = NetworkOptions.FirstOrDefault(x => x.NetworkType == account.Email.Network) ?? NetworkOptions.First();
        }
    }

    public class DecentralizedAddressSettingsPageViewModel : BaseAddressSettingsPageViewModel
    {
        private DecentralizedAddressSettingsModel _addressSettingsModel;
        public DecentralizedAddressSettingsModel AddressSettingsModel
        {
            get => _addressSettingsModel;
            set
            {
                if (_addressSettingsModel != null)
                {
                    _addressSettingsModel.PropertyChanged -= OnAddressSettingsModelPropertyChanged;
                }

                SetProperty(ref _addressSettingsModel, value, true);

                if (_addressSettingsModel != null)
                {
                    _addressSettingsModel.PropertyChanged += OnAddressSettingsModelPropertyChanged;
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


        public DecentralizedAddressSettingsPageViewModel()
        {
            ClaimNameCommand = new AsyncRelayCommand(ClaimNameAsync, CanClaimExecute);
            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        private bool CanClaimExecute()
        {
            if (_addressSettingsModel is null || _isClaimingName || IsWaitingResponse || !_addressSettingsModel.IsSenderNameVisible)
            {
                return false;
            }

            var desired = _addressSettingsModel.SenderName.Value;
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
                AddressSettingsModel = DecentralizedAddressSettingsModel.Create(accountData);
                AddressSettingsModel.IsNetworkLocked = !IsCreatingAccountMode;

                if (accountData.Email.Network == NetworkType.Bitcoin)
                {
                    AddressSettingsModel.SecretKeyWIF = await Core.GetSecurityManager().GetSecretKeyWIFAsync(accountData).ConfigureAwait(true);
                }
            }
            else
            {
                IsCreatingAccountMode = true;
                accountData = await CreateDecentralizedAccountAsync(NetworkType.Eppie, default).ConfigureAwait(true);
                AddressSettingsModel = DecentralizedAddressSettingsModel.Create(accountData);
                AddressSettingsModel.IsNetworkLocked = !IsCreatingAccountMode;
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

        protected override Account AddressSettingsModelToAccount()
        {
            return AddressSettingsModel.ToAccount();
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

        private async void OnAddressSettingsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DecentralizedAddressSettingsModel.SenderName))
            {
                ClaimNameCommand?.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(DecentralizedAddressSettingsModel.SelectedNetwork) && !_addressSettingsModel.IsNetworkLocked)
            {
                try
                {
                    _addressSettingsModel.IsNetworkLocked = true;
                    var accountData = await CreateDecentralizedAccountAsync(AddressSettingsModel.SelectedNetwork.NetworkType, default).ConfigureAwait(true);
                    AddressSettingsModel.UpdateAccount(accountData);
                    if (AddressSettingsModel.SelectedNetwork.NetworkType == NetworkType.Bitcoin)
                    {
                        AddressSettingsModel.SecretKeyWIF = await Core.GetSecurityManager().GetSecretKeyWIFAsync(accountData).ConfigureAwait(true);
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                finally
                {
                    _addressSettingsModel.IsNetworkLocked = false;
                    ClaimNameCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private async Task ClaimNameAsync()
        {
            if (AddressSettingsModel is null || !AddressSettingsModel.IsSenderNameVisible)
            {
                return;
            }

            var nameRaw = AddressSettingsModel.SenderName.Value?.Trim();
            AddressSettingsModel.SenderName.Errors.Clear();

            if (!ValidateName(nameRaw, out var normalized, out var errorKey))
            {
                AddressSettingsModel.SenderName.Errors.Add(GetLocalizedString(errorKey));
                return;
            }

            try
            {
                IsClaimingName = true;
                var account = AddressSettingsModel.ToAccount();
                var result = await Core.ClaimDecentralizedNameAsync(normalized, account.Email).ConfigureAwait(true);
                if (result)
                {
                    OnNameClaimSucceededAsync(normalized, account);
                }
                else
                {
                    AddressSettingsModel.SenderName.Errors.Add(GetLocalizedString("NameAlreadyTakenError"));
                }
            }
            catch (Exception ex)
            {
                AddressSettingsModel.SenderName.Errors.Add(GetLocalizedString("ClaimNameFailedError"));
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
            AddressSettingsModel.ClaimedName = normalized;
            AddressSettingsModel.SenderName.Value = normalized;
            AddressSettingsModel.Email.Value = EmailAddress.CreateDecentralizedAddress(account.Email.Network, normalized).DisplayAddress;
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
