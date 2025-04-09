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
    public class BaseAccountSettingsPageViewModel : BaseViewModel, IDisposable
    {
        protected class NeedAdditionalAuthInfo : Exception
        {
        }

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
            get { return !(_isCreatingAccountMode); }
        }

        public bool IsApplyButtonEnabled
        {
            get { return !IsWaitingResponse && !HasErrors; }
        }

        public bool IsRemoveButtonEnabled => !IsWaitingResponse;

        public IRelayCommand ApplySettingsCommand { get; }

        public IRelayCommand RemoveAccountCommand { get; }

        public ICommand CancelSettingsCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));


        public BaseAccountSettingsPageViewModel()
        {
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAndGoBackAsync, () => IsApplyButtonEnabled);
            RemoveAccountCommand = new AsyncRelayCommand(RemoveAccountAndGoBackAsync, () => IsRemoveButtonEnabled);
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
                var accountData = AccountSettingsModelToAccount();
                var result = await ApplyAccountSettingsAsync(accountData).ConfigureAwait(true);
                if (result)
                {
                    NavigateFromCurrentPage();
                }
            }
            catch (NeedAdditionalAuthInfo)
            {
            }
            catch (AuthorizationException)
            {
                throw new NeedAdditionalAuthInfo();
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

        private async Task<bool> ApplyAccountSettingsAsync(Account accountData)
        {
            _cts = new CancellationTokenSource();
            bool result = await CheckEmailAccountAsync(accountData, _cts.Token).ConfigureAwait(true);
            if (result)
            {
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
                CancelAsyncOperation();
            }
            else
            {
                GoBack();
            }
        }

        protected void GoBack()
        {
            NavigationService?.GoBack();
        }

        protected CancellationTokenSource _cts;

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
                    var account = AccountSettingsModelToAccount();
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

        protected virtual Account AccountSettingsModelToAccount()
        {
            return null;
        }

        public static ValidationResult ValidateEmailIsNotEmpty(BaseAccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is BaseAccountSettingsPageViewModel viewModel &&
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

        public static ValidationResult ValidateSynchronizationIntervalIsCorrect(BaseAccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is BaseAccountSettingsPageViewModel viewModel)
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
                _cts?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
