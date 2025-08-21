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
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        public DecentralizedAccountSettingsModel()
        {
        }

        protected DecentralizedAccountSettingsModel(Account account) : base(account)
        {
        }

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
            }
        }

        private bool _isNetworkLocked;
        public bool IsNetworkLocked
        {
            get => _isNetworkLocked;
            set => SetProperty(ref _isNetworkLocked, value);
        }

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

        public DecentralizedAccountSettingsModel()
        {
            _networkOptions = new ObservableCollection<Network>
            {
                new Network("Eppie Testnet", NetworkType.Eppie),
                new Network("Bitcoin Testnet", NetworkType.Bitcoin)
            };

            _selectedNetwork = _networkOptions[0];
        }

        protected DecentralizedAccountSettingsModel(Account account) : this()
        {
            UpdateAccount(account);
        }

        public virtual Account ToAccount()
        {
            if (Email.Value is null)
            {
                return null;
            }
            
            CurrentAccount.Email = new EmailAddress(Email.Value, SenderName);

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
            CurrentAccount = account ?? throw new ArgumentNullException(nameof(account));
            Email = account.Email.Address;
            SenderName = account.Email.Name;

            SelectedNetwork = NetworkOptions.FirstOrDefault(x => x.NetworkType == account.Email.Network) ?? NetworkOptions.First();
        }
    }

    public class DecentralizedAccountSettingsPageViewModel : BaseViewModel, IDisposable
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
            }
        }

        private bool _isWaitingResponse;
        public bool IsWaitingResponse
        {
            get { return _isWaitingResponse; }
            set
            {
                SetProperty(ref _isWaitingResponse, value);
                ApplySettingsCommand.NotifyCanExecuteChanged();
                RemoveAccountCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _isCreatingAccountMode = true;
        public bool IsCreatingAccountMode
        {
            get { return _isCreatingAccountMode; }
            private set { SetProperty(ref _isCreatingAccountMode, value); }
        }

        public IRelayCommand ApplySettingsCommand { get; }

        public IRelayCommand JoinWaitingListCommand => new RelayCommand(DoJoinWaitingList);

        public IRelayCommand RemoveAccountCommand { get; }

        public ICommand CancelSettingsCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public DecentralizedAccountSettingsPageViewModel()
        {
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAndGoBackAsync, () => !IsWaitingResponse);
            RemoveAccountCommand = new AsyncRelayCommand(RemoveAccountAndGoBackAsync, () => !IsWaitingResponse);
            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
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
                    AccountSettingsModel.SecretKeyWIF = Core.GetSecurityManager().GetSecretKeyWIF(accountData);
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

            var email = EmailAddress.CreateDecentralizedAddress(networkType, publicKey, $"#{accountIndex}");

            return new Account()
            {
                Email = email,
                IsBackupAccountSettingsEnabled = true,
                IsBackupAccountMessagesEnabled = true,
                Type = MailBoxType.Dec,
                DecentralizedAccountIndex = accountIndex
            };
        }

        private async Task ApplySettingsAndGoBackAsync()
        {
            IsWaitingResponse = true;
            try
            {
                var accountData = AccountSettingsModel.ToAccount();
                var result = await ApplyAccountSettingsAsync(accountData).ConfigureAwait(true);
                if (result)
                {
                    NavigateFromCurrentPage();
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                IsWaitingResponse = false;
            }
        }

        CancellationTokenSource _cts;

        private async Task<bool> ApplyAccountSettingsAsync(Account accountData)
        {
            _cts = new CancellationTokenSource();
            try
            {
                await ProcessAccountDataAsync(accountData, _cts.Token).ConfigureAwait(true);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private async Task ProcessAccountDataAsync(Account account, CancellationToken cancellationToken = default)
        {
            bool existAccount = await Core.ExistsAccountWithEmailAddressAsync(account.Email, cancellationToken).ConfigureAwait(true);

            if (!existAccount)
            {
                await Core.AddAccountAsync(account, cancellationToken).ConfigureAwait(true);
            }
            else
            {
                await Core.UpdateAccountAsync(account, cancellationToken).ConfigureAwait(true);
            }

            await BackupIfNeededAsync().ConfigureAwait(true);
        }

        private void NavigateFromCurrentPage()
        {
            if (IsCreatingAccountMode)
            {
                NavigationService?.GoBackToOrNavigate(nameof(MainPageViewModel));
            }
            else
            {
                NavigationService?.GoBackOrNavigate(nameof(MainPageViewModel));
            }
        }

        private async void DoJoinWaitingList()
        {
            try
            {
                await LauncherService.LaunchAsync(new Uri(BrandService.GetHomepage())).ConfigureAwait(true);
                DoCancel();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void DoCancel()
        {
            if (_isWaitingResponse)
            {
                CancelAsyncOperation();
            }
            else
            {
                GoBack();
            }
        }

        private void GoBack()
        {
            NavigationService?.GoBack();
        }

        private void CancelAsyncOperation()
        {
            _cts?.Cancel();
        }

        private async Task RemoveAccountAndGoBackAsync()
        {
            try
            {
                IsWaitingResponse = true;

                bool isConfirmed = await MessageService.ShowRemoveAccountDialogAsync().ConfigureAwait(true);

                if (isConfirmed)
                {
                    var account = AccountSettingsModel.ToAccount();
                    await Core.DeleteAccountAsync(account).ConfigureAwait(true);

                    await BackupIfNeededAsync().ConfigureAwait(true);

                    GoBack();
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                IsWaitingResponse = false;
            }
        }

        private async void OnAccountSettingsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DecentralizedAccountSettingsModel.SelectedNetwork) &&
                !_accountSettingsModel.IsNetworkLocked)
            {
                try
                {
                    _accountSettingsModel.IsNetworkLocked = true;
                    var accountData = await CreateDecentralizedAccountAsync(AccountSettingsModel.SelectedNetwork.NetworkType, default).ConfigureAwait(true);
                    AccountSettingsModel.UpdateAccount(accountData);
                    AccountSettingsModel.SecretKeyWIF = AccountSettingsModel.SelectedNetwork.NetworkType == NetworkType.Bitcoin ? Core.GetSecurityManager().GetSecretKeyWIF(accountData) : string.Empty;
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                finally
                {
                    _accountSettingsModel.IsNetworkLocked = false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _cts?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
