using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;
using Tuvi.Proton.Client.Exceptions;
using static Tuvi.App.ViewModels.AccountSettingsPageViewModel;

namespace Tuvi.App.ViewModels
{
    public class ProtonAccountSettingsModel : ObservableObject
    {
        /// <summary>
        /// Default interval value (in minutes) for checking new messages
        /// </summary>
        private const int DefaultSynchronizationInterval = 10;

        protected Account CurrentAccount { get; set; }
        public ValidatableProperty<string> Email { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> SynchronizationInterval { get; } = new ValidatableProperty<string>();

        private string _senderName;
        public string SenderName
        {
            get => _senderName;
            set => SetProperty(ref _senderName, value);
        }

        public ValidatableProperty<string> TwoFactorCode { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> Password { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> MailboxPassword { get; } = new ValidatableProperty<string>();


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


        public ProtonAccountSettingsModel()
        {
            Email.SetInitialValue(string.Empty);
            SynchronizationInterval.SetInitialValue(DefaultSynchronizationInterval.ToString());
        }
        protected ProtonAccountSettingsModel(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            CurrentAccount = account;

            if (account.Email != null)
            {
                Email.SetInitialValue(account.Email.Address);
                SenderName = account.Email.Name;
            }

            Password.SetInitialValue(string.Empty);
            TwoFactorCode.SetInitialValue(string.Empty);
            MailboxPassword.SetInitialValue(string.Empty);

            IsBackupAccountSettingsEnabled = account.IsBackupAccountSettingsEnabled;
            IsBackupAccountMessagesEnabled = account.IsBackupAccountMessagesEnabled;
            SynchronizationInterval.SetInitialValue(account.SynchronizationInterval.ToString());
            Email.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Email), args.PropertyName);
            };
            Password.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Password), args.PropertyName);
            };
            TwoFactorCode.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(TwoFactorCode), args.PropertyName);
            };
            MailboxPassword.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(MailboxPassword), args.PropertyName);
            };
            SynchronizationInterval.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SynchronizationInterval), args.PropertyName);
            };
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

        public virtual Account ToAccount()
        {
            CurrentAccount.Email = new EmailAddress(Email.Value, SenderName);

            CurrentAccount.AuthData = new BasicAuthData() { Password = Password.Value };
            CurrentAccount.IsBackupAccountSettingsEnabled = IsBackupAccountSettingsEnabled;
            CurrentAccount.IsBackupAccountMessagesEnabled = IsBackupAccountMessagesEnabled;
            CurrentAccount.SynchronizationInterval = int.TryParse(SynchronizationInterval.Value, out int interval)
                ? interval
                : DefaultSynchronizationInterval;
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
        public class NeedReloginData
        {
            public Account Account { get; set; }
        }

        private ProtonAccountSettingsModel _accountSettingsModel;
        [CustomValidation(typeof(ProtonAccountSettingsPageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(ProtonAccountSettingsPageViewModel), nameof(ValidateEmailIsNotEmpty))]
        [CustomValidation(typeof(ProtonAccountSettingsPageViewModel), nameof(ValidatePasswordIsNotEmpty))]
        [CustomValidation(typeof(ProtonAccountSettingsPageViewModel), nameof(ValidateSynchronizationIntervalIsCorrect))]
        public ProtonAccountSettingsModel AccountSettingsModel
        {
            get { return _accountSettingsModel; }
            set
            {
                if (_accountSettingsModel != null)
                {
                    _accountSettingsModel.PropertyChanged -= OnAccountSettingsModelPropertyChanged;
                }

                SetProperty(ref _accountSettingsModel, value, true);
                OnPropertyChanged(nameof(IsEmailReadonly));

                if (_accountSettingsModel != null)
                {
                    _accountSettingsModel.PropertyChanged += OnAccountSettingsModelPropertyChanged;
                }
            }
        }

        private void OnAccountSettingsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ValidateProperty(AccountSettingsModel, nameof(AccountSettingsModel));
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
                else if (data is NeedReloginData needReloginData)
                {
                    var account = needReloginData.Account;
                    InitModel(ProtonAccountSettingsModel.Create(account), false);

                    AccountSettingsModel.Password.NeedsValidation = true;
                    AccountSettingsModel.Password.Errors.Clear();
                    AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
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
            AccountSettingsModel.Email.NeedsValidation = true;
            AccountSettingsModel.Password.NeedsValidation = true;
            AccountSettingsModel.TwoFactorCode.NeedsValidation = true;
            AccountSettingsModel.MailboxPassword.NeedsValidation = true;

            if ((!IsEmailReadonly && string.IsNullOrEmpty(AccountSettingsModel.Email.Value))
               || string.IsNullOrEmpty(AccountSettingsModel.Password.Value))
            {
                ValidateProperty(AccountSettingsModel, nameof(AccountSettingsModel));
                return;
            }

            IsWaitingResponse = true;
            try
            {
                var (userId, refreshToken, saltedKeyPass) = await Proton.ClientAuth.LoginFullAsync(AccountSettingsModel.Email.Value,
                                                                                                   AccountSettingsModel.Password.Value,
                                                                                                   async (ct) =>
                                                                                                   {
                                                                                                       await DispatcherService.RunAsync(() =>
                                                                                                       {
                                                                                                           if (string.IsNullOrEmpty(AccountSettingsModel.TwoFactorCode.Value))
                                                                                                           {
                                                                                                               AccountSettingsModel.TwoFactorCode.Errors.Clear();
                                                                                                               AccountSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("NeedTwofactorCode"));

                                                                                                               throw new OperationCanceledException();
                                                                                                           }
                                                                                                       });
                                                                                                       return AccountSettingsModel.TwoFactorCode.Value;
                                                                                                   },
                                                                                                   async (ct) =>
                                                                                                   {
                                                                                                       await DispatcherService.RunAsync(() =>
                                                                                                       {
                                                                                                           if (string.IsNullOrEmpty(AccountSettingsModel.MailboxPassword.Value))
                                                                                                           {
                                                                                                               AccountSettingsModel.MailboxPassword.Errors.Clear();
                                                                                                               AccountSettingsModel.MailboxPassword.Errors.Add(GetLocalizedString("NeedMailboxPassword"));

                                                                                                               throw new OperationCanceledException();
                                                                                                           }
                                                                                                       });
                                                                                                       return AccountSettingsModel.MailboxPassword.Value;
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
                AccountSettingsModel.Password.Errors.Clear();
                AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (AuthenticationException)
            {
                AccountSettingsModel.Password.Errors.Clear();
                AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (ProtonSessionRequestException)
            {
                AccountSettingsModel.TwoFactorCode.Errors.Clear();
                AccountSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (OperationCanceledException)
            {
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
                // TODO: TVM-319 
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

        public static ValidationResult ClearValidationErrors(ProtonAccountSettingsModel accountModel, ValidationContext _)
        {
            if (accountModel != null)
            {
                accountModel.Email.Errors.Clear();
                accountModel.SynchronizationInterval.Errors.Clear();
                accountModel.Password.Errors.Clear();
                accountModel.TwoFactorCode.Errors.Clear();
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateEmailIsNotEmpty(ProtonAccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is ProtonAccountSettingsPageViewModel viewModel &&
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

        public static ValidationResult ValidatePasswordIsNotEmpty(ProtonAccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is ProtonAccountSettingsPageViewModel viewModel)
            {
                if (accountModel != null &&
                    accountModel.Password.NeedsValidation &&
                    string.IsNullOrEmpty(accountModel.Password.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    accountModel.Password.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateSynchronizationIntervalIsCorrect(ProtonAccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is ProtonAccountSettingsPageViewModel viewModel)
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
