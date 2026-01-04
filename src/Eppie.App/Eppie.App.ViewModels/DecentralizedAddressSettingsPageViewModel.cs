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
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EmailValidation;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class DecentralizedAddressSettingsModel : BaseAddressSettingsModel
    {
        private NetworkType _selectedNetwork = NetworkType.Eppie;

        // TODO: Zeroize WIF key after using it
        private string _secretKeyWIF;
        public string SecretKeyWIF
        {
            get => _secretKeyWIF;
            set => SetProperty(ref _secretKeyWIF, value);
        }

        public bool IsSecretKeyVisible
        {
            get => IsSecretKeySupported(_selectedNetwork);
        }

        private static bool IsSecretKeySupported(NetworkType networkType)
        {
            return networkType == NetworkType.Bitcoin || networkType == NetworkType.Ethereum;
        }

        private int _derivationIndex;
        public int DerivationIndex
        {
            get => _derivationIndex;
            set => SetProperty(ref _derivationIndex, value);
        }

        public bool IsDerivationIndexVisible
        {
            get => CurrentAccount != null && !CurrentAccount.Email.IsHybrid;
        }

        public bool IsSenderNameVisible
        {
            get => _selectedNetwork == NetworkType.Eppie && !CurrentAccount.Email.IsHybrid;
        }

        private string _claimedName;
        public string ClaimedName
        {
            get => _claimedName;
            set => SetProperty(ref _claimedName, value);
        }

        public string DisplayAddress => CurrentAccount?.Email?.DisplayAddress;

        protected DecentralizedAddressSettingsModel(Account account) : base(account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            _selectedNetwork = account.Email.Network;

            CurrentAccount = account;

            Email.Value = account.Email.Address;
            SenderName.Value = account.Email.Name;
            ClaimedName = account.Email.Name;

            if (account.DecentralizedAccountIndex >= 0)
            {
                _derivationIndex = account.DecentralizedAccountIndex;
            }
        }

        public override Account ToAccount()
        {
            var account = base.ToAccount();
            if (account is null)
            {
                return null;
            }

            if (IsSenderNameVisible && !string.IsNullOrEmpty(ClaimedName))
            {
                account.Email = new EmailAddress(account.Email.Address, ClaimedName);
            }
            else
            {
                account.Email = new EmailAddress(account.Email.Address, string.Empty);
            }

            account.Type = MailBoxType.Dec;

            return account;
        }

        public static DecentralizedAddressSettingsModel Create(Account account)
        {
            return new DecentralizedAddressSettingsModel(account);
        }
    }

    public class DecentralizedAddressSettingsPageViewModel : BaseAddressSettingsPageViewModel
    {
        protected override BaseAddressSettingsModel AddressSettingsModelBase => AddressSettingsModel;

        private DecentralizedAddressSettingsModel _addressSettingsModel;
        public DecentralizedAddressSettingsModel AddressSettingsModel
        {
            get => _addressSettingsModel;
            set
            {
                SetProperty(ref _addressSettingsModel, value, true);
            }
        }

        public IRelayCommand JoinWaitingListCommand => new RelayCommand(DoJoinWaitingList);

        public IRelayCommand CopySecretKeyCommand { get; }

        private bool CanCopySecretKey(IClipboardProvider clipboard)
        {
            return clipboard != null && AddressSettingsModel != null && !string.IsNullOrEmpty(AddressSettingsModel.SecretKeyWIF);
        }

        private void CopySecretKey(IClipboardProvider clipboard)
        {
            if (clipboard != null && AddressSettingsModel?.SecretKeyWIF != null)
            {
                clipboard.SetClipboardContent(AddressSettingsModel.SecretKeyWIF);
            }
        }

        public DecentralizedAddressSettingsPageViewModel()
        {
            CopySecretKeyCommand = new RelayCommand<IClipboardProvider>(CopySecretKey, CanCopySecretKey);

            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account existing)
                {
                    if (existing.Email.Network != NetworkType.Eppie)
                    {
                        throw new ArgumentException("The provided account is not a Eppie account.");
                    }

                    IsCreatingAccountMode = false;
                    AddressSettingsModel = DecentralizedAddressSettingsModel.Create(existing);
                }
                else
                {
                    IsCreatingAccountMode = true;
                    var account = await CreateDecentralizedAccountAsync(NetworkType.Eppie, CancellationToken.None).ConfigureAwait(true);
                    AddressSettingsModel = DecentralizedAddressSettingsModel.Create(account);
                }
            }
            catch (Exception e)
            {
                OnError(e);
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
                DecentralizedAccountIndex = accountIndex,
                IsMessageFooterEnabled = false
            };
        }

        protected override Account ApplySettingsToAccount()
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

        private async Task<bool> ClaimNameAsync()
        {
            if (AddressSettingsModel is null || !AddressSettingsModel.IsSenderNameVisible || !IsCreatingAccountMode)
            {
                return true;
            }

            var nameRaw = AddressSettingsModel.SenderName.Value?.Trim();

            if (string.IsNullOrWhiteSpace(nameRaw))
            {
                return true;
            }

            AddressSettingsModel.SenderName.Errors.Clear();

            if (!ValidateName(nameRaw, out var normalized, out var errorKey))
            {
                AddressSettingsModel.SenderName.Errors.Add(GetLocalizedString(errorKey));
                return false;
            }

            try
            {
                var account = AddressSettingsModel.ToAccount();
                var result = await Core.ClaimDecentralizedNameAsync(normalized, account.Email).ConfigureAwait(true);
                if (result)
                {
                    OnNameClaimSucceeded(normalized, account);
                    return true;
                }
                else
                {
                    AddressSettingsModel.SenderName.Errors.Add(GetLocalizedString("NameAlreadyTakenError"));
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddressSettingsModel.SenderName.Errors.Add(GetLocalizedString("ClaimNameFailedError"));
                OnError(ex);
                return false;
            }
        }

        protected override async Task ApplySettingsAndGoBackAsync()
        {
            if (!await ClaimNameAsync().ConfigureAwait(true))
            {
                return;
            }

            await base.ApplySettingsAndGoBackAsync().ConfigureAwait(true);
        }

        private void OnNameClaimSucceeded(string normalized, Account account)
        {
            // TODO: Remove hardcoded ".test" suffix after Testnet phase is over
            normalized += ".test";

            AddressSettingsModel.ClaimedName = normalized;
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
