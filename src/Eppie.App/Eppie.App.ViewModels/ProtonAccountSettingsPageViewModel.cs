using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ProtonAccountSettingsModel : ObservableObject
    {
        protected Account CurrentAccount { get; set; }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _senderName;
        public string SenderName
        {
            get => _senderName;
            set => SetProperty(ref _senderName, value);
        }

        private string _twoFactorCode;
        public string TwoFactorCode
        {
            get => _twoFactorCode;
            set => SetProperty(ref _twoFactorCode, value);
        }

        public ValidatableProperty<string> Password { get; } = new ValidatableProperty<string>();

        public ValidatableProperty<string> MailboxPassword { get; } = new ValidatableProperty<string>();

        public ProtonAccountSettingsModel()
        {
        }
        protected ProtonAccountSettingsModel(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            Password.SetInitialValue(string.Empty);
            MailboxPassword.SetInitialValue(string.Empty);

            Password.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Password), args.PropertyName);
            };
            MailboxPassword.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(MailboxPassword), args.PropertyName);
            };

            if (account.Email != null)
            {
                Email = account.Email.Address;
                SenderName = account.Email.Name;
            }

            CurrentAccount = account;
        }

        // TODO: avoid copy-paste
        private void OnValidatablePropertyChanged<T>(string validatablePropertyName, string propertyName)
        {
            if (propertyName == nameof(ValidatableProperty<T>.Value) ||
                propertyName == nameof(ValidatableProperty<T>.NeedsValidation))
            {
                OnPropertyChanged(validatablePropertyName);
            }
        }

        public virtual Account ToAccount()
        {
            CurrentAccount.Email = new EmailAddress(Email, SenderName);
            CurrentAccount.AuthData = new BasicAuthData() { Password = Password.Value };
            CurrentAccount.Type = (int)MailBoxType.Proton;
            return CurrentAccount;
        }

        public static ProtonAccountSettingsModel Create(Account account)
        {
            return new ProtonAccountSettingsModel(account);
        }
    }

    public class ProtonAccountSettingsPageViewModel : BaseViewModel, IDisposable
    {
        private ProtonAccountSettingsModel _accountSettingsModel;
        public ProtonAccountSettingsModel AccountSettingsModel
        {
            get => _accountSettingsModel;
            set => SetProperty(ref _accountSettingsModel, value, true);
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

        public bool IsEmailReadonly
        {
            get { return !(_isCreatingAccountMode); }
        }

        public IRelayCommand ApplySettingsCommand { get; }

        public IRelayCommand RemoveAccountCommand { get; }

        public ICommand CancelSettingsCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public ProtonAccountSettingsPageViewModel()
        {
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAndGoBackAsync, () => !IsWaitingResponse);
            RemoveAccountCommand = new AsyncRelayCommand(RemoveAccountAndGoBackAsync, () => !IsWaitingResponse);
            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        public override void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account accountData)
                {
                    InitModel(ProtonAccountSettingsModel.Create(accountData), false);
                }
                else
                {
                    InitModel(ProtonAccountSettingsModel.Create(Account.Default), true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void InitModel(ProtonAccountSettingsModel accountSettingsModel, bool isCreatingMode)
        {
            IsCreatingAccountMode = isCreatingMode;
            AccountSettingsModel = accountSettingsModel;
        }

        private async Task ApplySettingsAndGoBackAsync()
        {
            IsWaitingResponse = true;
            try
            {
                var (userId, refreshToken, saltedKeyPass) = await Proton.ClientAuth.LoginFullAsync(AccountSettingsModel.Email,
                                                                                                   AccountSettingsModel.Password.Value,
                                                                                                   (ct) =>
                                                                                                   {
                                                                                                       return Task.FromResult(AccountSettingsModel.TwoFactorCode);
                                                                                                   },
                                                                                                   (ct) =>
                                                                                                   {
                                                                                                       return Task.FromResult(AccountSettingsModel.MailboxPassword.Value);
                                                                                                   },
                                                                                                   default)
                                                         .ConfigureAwait(true);
                var accountData = AccountSettingsModel.ToAccount();
                accountData.AuthData = new ProtonAuthData()
                {
                    UserId = userId,
                    RefreshToken = refreshToken,
                    SaltedPassword = saltedKeyPass
                };
                var result = await ApplyAccountSettingsAsync(accountData).ConfigureAwait(true);
                if (result)
                {
                    NavigateFromCurrentPage();
                }
            }
            catch (AuthorizationException)
            {
                // TODO
                AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
                AccountSettingsModel.MailboxPassword.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (AuthenticationException)
            {
                // TODO
                AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
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
