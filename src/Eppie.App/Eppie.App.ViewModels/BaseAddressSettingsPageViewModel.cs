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
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public abstract class BaseAddressSettingsPageViewModel : BaseViewModel, IDisposable
    {
        /// <summary>
        /// All available external content policy values for UI binding
        /// </summary>
        public ExternalContentPolicy[] ExternalContentPolicyValues { get; } = (ExternalContentPolicy[])Enum.GetValues(typeof(ExternalContentPolicy));

        private bool _isWaitingResponse;
        public bool IsWaitingResponse
        {
            get { return _isWaitingResponse; }
            set
            {
                SetProperty(ref _isWaitingResponse, value);

                OnPropertyChanged(nameof(IsApplyButtonEnabled));
                ApplySettingsCommand.NotifyCanExecuteChanged();

                OnPropertyChanged(nameof(IsRemoveButtonEnabled));
                RemoveAccountCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _isCreatingAccountMode = true;
        public bool IsCreatingAccountMode
        {
            get { return _isCreatingAccountMode; }
            protected set
            {
                SetProperty(ref _isCreatingAccountMode, value);
                OnPropertyChanged(nameof(IsEmailReadonly));
            }
        }

        public bool IsEmailReadonly
        {
            get { return !_isCreatingAccountMode; }
        }

        public bool IsApplyButtonEnabled
        {
            get { return !IsWaitingResponse && !HasErrors; }
        }

        private bool _isHybridAddress;
        public bool IsHybridAddress
        {
            get
            {
                CheckLinkedHybridAccountExists();
                return _isHybridAddress;
            }

            protected set
            {
                SetProperty(ref _isHybridAddress, value);
                OnPropertyChanged(nameof(IsHybridAddress));
                OnPropertyChanged(nameof(ShowHybridAddressButton));
            }
        }

        private async void CheckLinkedHybridAccountExists()
        {
            try
            {
                var email = AddressSettingsModelBase?.OriginalEmail;
                if (email != null)
                {
                    var deckey = await Core.GetSecurityManager().GetEmailPublicKeyStringAsync(email).ConfigureAwait(true);
                    var hybridAddress = email.MakeHybrid(deckey);

                    var account = await Core.GetAccountAsync(hybridAddress).ConfigureAwait(true);

                    IsHybridAddress = account != null;
                }
            }
            catch (AccountIsNotExistInDatabaseException)
            {
                // Account not found, do nothing
            }
            catch (Exception e)
            {
                OnError(e);
                return;
            }
        }

        protected abstract BaseAddressSettingsModel AddressSettingsModelBase { get; }

        public bool ShowHybridAddressButton
        {
            get
            {
                //TODO: Disabled for now
                _ = !IsHybridAddress && !IsCreatingAccountMode;
                //return !IsHybridAddress && !IsCreatingAccountMode;
                return false;
            }
        }

        public bool IsRemoveButtonEnabled => !IsWaitingResponse;

        public IRelayCommand ApplySettingsCommand { get; }

        public IRelayCommand RemoveAccountCommand { get; }

        public ICommand CancelSettingsCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public IRelayCommand CreateHybridAddressCommand { get; }

        protected BaseAddressSettingsPageViewModel()
        {
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAndGoBackAsync, () => IsApplyButtonEnabled);
            RemoveAccountCommand = new AsyncRelayCommand(RemoveAccountAndGoBackAsync, () => IsRemoveButtonEnabled);
            CreateHybridAddressCommand = new AsyncRelayCommand(CreateHybridAddressAsync, () => !IsHybridAddress);

            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        protected async Task ProcessAccountDataAsync(Account account, CancellationToken cancellationToken = default)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

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

        protected void NavigateFromCurrentPage()
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

        protected virtual async Task ApplySettingsAndGoBackAsync()
        {
            if (!IsValid())
            {
                return;
            }

            IsWaitingResponse = true;
            try
            {
                var accountData = ApplySettingsToAccount();
                var result = await ApplySettingsAsync(accountData).ConfigureAwait(true);
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

        private async Task<bool> ApplySettingsAsync(Account accountData)
        {
            Cts = new CancellationTokenSource();
            bool result = await CheckEmailAccountAsync(accountData, Cts.Token).ConfigureAwait(true);
            if (result)
            {
                try
                {
                    await ProcessAccountDataAsync(accountData, Cts.Token).ConfigureAwait(true);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }

            return false;
        }

        protected virtual Task<bool> CheckEmailAccountAsync(Account accountData, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        private void DoCancel()
        {
            // TODO: TVM-347 Suspend mailbox until relogin
            // if there was a relogin, and the user did not update the data,
            // then we need to suspend work with this email until the data is updated

            if (_isWaitingResponse)
            {
                OnCancel();
            }
            else
            {
                GoBack();
            }
        }

        protected virtual void OnCancel()
        {
            CancelAsyncOperation();
        }

        protected void GoBack()
        {
            NavigationService?.GoBack();
        }

        protected CancellationTokenSource Cts { get; set; }

        private void CancelAsyncOperation()
        {
            Cts?.Cancel();
        }

        private async Task RemoveAccountAndGoBackAsync()
        {
            try
            {
                IsWaitingResponse = true;

                bool isConfirmed = await MessageService.ShowRemoveAccountDialogAsync().ConfigureAwait(true);

                if (isConfirmed)
                {
                    var account = ApplySettingsToAccount();
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

        private async Task CreateHybridAddressAsync()
        {
            await Core.CreateHybridAccountAsync(ApplySettingsToAccount()).ConfigureAwait(true);

            GoBack();
        }

        /// <summary>
        /// Applies the current page settings to an <see cref="Account"/> instance.
        /// </summary>
        /// <remarks>
        /// This method mutates the underlying account data. Invoke it only from <c>ApplySettings*</c> methods.
        /// Calling it from elsewhere may lead to unintended side effects.
        /// </remarks>
        /// <returns>
        /// The account with the applied settings. The base implementation returns <see langword="null"/>.
        /// </returns>
        protected virtual Account ApplySettingsToAccount()
        {
            return null;
        }

        public static ValidationResult ValidateEmailIsNotEmpty(BaseAddressSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is BaseAddressSettingsPageViewModel viewModel &&
                !viewModel.IsEmailReadonly)
            {
                if (accountModel != null &&
                    accountModel.Email.NeedsValidation &&
                    string.IsNullOrEmpty(accountModel.Email.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    accountModel.Email.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateSynchronizationIntervalIsCorrect(BaseAddressSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is BaseAddressSettingsPageViewModel viewModel)
            {
                if (accountModel != null &&
                    accountModel.SynchronizationInterval.NeedsValidation)
                {
                    if (!int.TryParse(accountModel.SynchronizationInterval.Value, out int interval))
                    {
                        var error = viewModel.GetLocalizedString("ValueIsNotNumberNotification");
                        accountModel.SynchronizationInterval.Errors.Add(error);
                        return new ValidationResult(error);
                    }
                    else if (interval < 0)
                    {
                        var error = viewModel.GetLocalizedString("ValueCantBeNegativeNotification");
                        accountModel.SynchronizationInterval.Errors.Add(error);
                        return new ValidationResult(error);
                    }
                }
            }

            return ValidationResult.Success;
        }

        protected virtual bool IsValid()
        {
            return true;
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
                Cts?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
