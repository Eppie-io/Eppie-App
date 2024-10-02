using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;
using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;
using Tuvi.OAuth2;

namespace Tuvi.App.ViewModels
{
    public class AccountSettingsModel : ObservableObject
    {
        /// <summary>
        /// Default interval value (in minutes) for checking new messages
        /// </summary>
        private const int DefaultSynchronizationInterval = 10;

        protected Account CurrentAccount { get; set; }
        public ValidatableProperty<string> Email { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> OutgoingServerAddress { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> IncomingServerAddress { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> SynchronizationInterval { get; } = new ValidatableProperty<string>();

        private string _senderName;
        public string SenderName
        {
            get { return _senderName; }
            set { SetProperty(ref _senderName, value); }
        }

        private int _outgoingServerPort = 465; // default outgoing port (SMTP)
        public int OutgoingServerPort
        {
            get { return _outgoingServerPort; }
            set { SetProperty(ref _outgoingServerPort, value); }
        }

        private int _incomingServerPort = 993; // default incoming port (IMAP)
        public int IncomingServerPort
        {
            get { return _incomingServerPort; }
            set { SetProperty(ref _incomingServerPort, value); }
        }

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

        private MailProtocol _incomingMailProtocol = MailProtocol.IMAP;
        public MailProtocol IncomingMailProtocol
        {
            get { return _incomingMailProtocol; }
            set
            {
                if (SetProperty(ref _incomingMailProtocol, value))
                {
                    if (_incomingMailProtocol == MailProtocol.IMAP)
                    {
                        IncomingServerPort = 993; // default incoming port for IMAP
                    }
                    else if (_incomingMailProtocol == MailProtocol.POP3)
                    {
                        IncomingServerPort = 995; // default incoming port for POP3
                    }
                }
            }
        }

        public AccountSettingsModel()
        {
            Email.SetInitialValue(string.Empty);
            OutgoingServerAddress.SetInitialValue(string.Empty);
            IncomingServerAddress.SetInitialValue(string.Empty);
            SynchronizationInterval.SetInitialValue(DefaultSynchronizationInterval.ToString());
        }
        protected AccountSettingsModel(Account account)
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

            OutgoingServerAddress.SetInitialValue(account.OutgoingServerAddress);
            IncomingServerAddress.SetInitialValue(account.IncomingServerAddress);
            OutgoingServerPort = account.OutgoingServerPort;
            IncomingServerPort = account.IncomingServerPort;
            IncomingMailProtocol = account.IncomingMailProtocol;
            IsBackupAccountSettingsEnabled = account.IsBackupAccountSettingsEnabled;
            IsBackupAccountMessagesEnabled = account.IsBackupAccountMessagesEnabled;
            SynchronizationInterval.SetInitialValue(account.SynchronizationInterval.ToString());

            Email.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Email), args.PropertyName);
            };
            OutgoingServerAddress.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(OutgoingServerAddress), args.PropertyName);
            };
            IncomingServerAddress.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(IncomingServerAddress), args.PropertyName);
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

            CurrentAccount.OutgoingMailProtocol = MailProtocol.SMTP;
            CurrentAccount.OutgoingServerAddress = OutgoingServerAddress.Value;
            CurrentAccount.OutgoingServerPort = OutgoingServerPort;
            CurrentAccount.IncomingMailProtocol = IncomingMailProtocol;
            CurrentAccount.IncomingServerAddress = IncomingServerAddress.Value;
            CurrentAccount.IncomingServerPort = IncomingServerPort;
            CurrentAccount.IsBackupAccountSettingsEnabled = IsBackupAccountSettingsEnabled;
            CurrentAccount.IsBackupAccountMessagesEnabled = IsBackupAccountMessagesEnabled;
            CurrentAccount.SynchronizationInterval = int.TryParse(SynchronizationInterval.Value, out int interval)
                ? interval
                : DefaultSynchronizationInterval;
            CurrentAccount.Type = (int)MailBoxType.Email;

            return CurrentAccount;
        }

        public static AccountSettingsModel Create(Account account)
        {
            switch (account?.AuthData)
            {
                case ProtonAuthData authData:
                    return new ProtonAuthAccountSettingsModel(account);
                case OAuth2Data oauth2:
                    return new OAuth2AccountSettingsModel(account);
                case BasicAuthData basic:
                    return new BasicAccountSettingsModel(account);
            }

            return new BasicAccountSettingsModel(account);
        }
    }

    public class BasicAccountSettingsModel : AccountSettingsModel
    {
        public ValidatableProperty<string> Password { get; } = new ValidatableProperty<string>();
        public BasicAccountSettingsModel(Account account)
            : base(account)
        {
            if (account.AuthData is BasicAuthData basicData)
            {
                Password.SetInitialValue(basicData.Password);
            }
            else
            {
                Password.SetInitialValue(string.Empty);
            }

            Password.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Password), args.PropertyName);
            };
            SynchronizationInterval.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SynchronizationInterval), args.PropertyName);
            };
        }

        public override Account ToAccount()
        {
            CurrentAccount = base.ToAccount();
            CurrentAccount.AuthData = new BasicAuthData() { Password = Password.Value };

            return CurrentAccount;
        }
    }

    public class OAuth2AccountSettingsModel : AccountSettingsModel
    {
        public string RefreshToken { get; set; }
        public string AuthAssistantId { get; set; }

        public OAuth2AccountSettingsModel(Account account)
            : base(account)
        {
            if (account.AuthData is OAuth2Data oauth2Data)
            {
                RefreshToken = oauth2Data.RefreshToken;
                AuthAssistantId = oauth2Data.AuthAssistantId;
            }
        }

        public override Account ToAccount()
        {
            CurrentAccount = base.ToAccount();
            CurrentAccount.AuthData = new OAuth2Data()
            {
                RefreshToken = RefreshToken,
                AuthAssistantId = AuthAssistantId
            };

            return CurrentAccount;
        }
    }

    public class ProtonAuthAccountSettingsModel : AccountSettingsModel
    {
        public string RefreshToken { get; set; }
        public string UserId { get; set; }
        public string SaltedPassword { get; set; }

        public ProtonAuthAccountSettingsModel(Account account)
            : base(account)
        {
            if (account.AuthData is ProtonAuthData authData)
            {
                RefreshToken = authData.RefreshToken;
                UserId = authData.UserId;
                SaltedPassword = authData.SaltedPassword;
            }
        }

        public override Account ToAccount()
        {
            CurrentAccount = base.ToAccount();
            CurrentAccount.AuthData = new ProtonAuthData()
            {
                RefreshToken = RefreshToken,
                UserId = UserId,
                SaltedPassword = SaltedPassword
            };

            return CurrentAccount;
        }
    }

    public class AccountSettingsPageViewModel : BaseViewModel, IDisposable
    {
        public AuthorizationProvider AuthProvider { get; private set; }

        public void SetAuthProvider(AuthorizationProvider authProvider)
        {
            AuthProvider = authProvider;
        }

        private AccountSettingsModel _accountSettingsModel;
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ValidateEmailIsNotEmpty))]
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ValidateOutgoingServerAddressIsNotEmpty))]
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ValidateIncomingServerAddressIsNotEmpty))]
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ValidateSynchronizationIntervalIsCorrect))]
        public AccountSettingsModel AccountSettingsModel
        {
            get { return _accountSettingsModel; }
            set
            {
                if (_accountSettingsModel != null)
                {
                    _accountSettingsModel.PropertyChanged -= OnAccountSettingsModelPropertyChanged;
                }

                SetProperty(ref _accountSettingsModel, value, true);
                OnPropertyChanged(nameof(IsBasicAccount));
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

        private Array _incomingProtocolTypes;
        public Array IncomingProtocolTypes
        {
            get
            {
                if (_incomingProtocolTypes == null)
                {
                    _incomingProtocolTypes = Array.CreateInstance(typeof(MailProtocol), 2);
                    _incomingProtocolTypes.SetValue(MailProtocol.IMAP, 0);
                    _incomingProtocolTypes.SetValue(MailProtocol.POP3, 1);
                }

                return _incomingProtocolTypes;
            }
        }

        private bool _inCompactMode;
        public bool InCompactMode
        {
            get { return _inCompactMode; }
            set { SetProperty(ref _inCompactMode, value); }
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

        public bool IsApplyButtonEnabled
        {
            get { return !IsWaitingResponse && !HasErrors; }
        }

        private bool _isSignInButtonVisible;
        public bool IsSignInButtonVisible
        {
            get { return _isSignInButtonVisible; }
            set { SetProperty(ref _isSignInButtonVisible, value); }
        }

        private bool _isSignInButtonEnabled;
        public bool IsSignInButtonEnabled
        {
            get { return _isSignInButtonEnabled; }
            set
            {
                SetProperty(ref _isSignInButtonEnabled, value);
                SignInCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _isCreatingAccountMode = true;
        public bool IsCreatingAccountMode
        {
            get { return _isCreatingAccountMode; }
            private set
            {
                SetProperty(ref _isCreatingAccountMode, value);
                OnPropertyChanged(nameof(IsEmailReadonly));
            }
        }

        public bool IsHybridAddress
        {
            get
            {
                if (AccountSettingsModel.Email.Value != null)
                {
                    return new EmailAddress(AccountSettingsModel.Email.Value).IsHybrid;
                }

                return false;
            }
        }

        public bool ShowHybridAddressButton
        {
            get
            {
                return !IsHybridAddress && !IsCreatingAccountMode;
            }
        }

        public bool IsEmailReadonly
        {
            get { return !(_isCreatingAccountMode && IsBasicAccount); }
        }

        public bool IsBasicAccount
        {
            get { return AccountSettingsModel is BasicAccountSettingsModel; }
        }

        public IRelayCommand ApplySettingsCommand { get; }

        public IRelayCommand SignInCommand { get; }

        public IRelayCommand RemoveAccountCommand { get; }

        public IRelayCommand CreateHybridAddress { get; }

        public bool IsRemoveButtonEnabled => !IsWaitingResponse;

        public ICommand CancelSettingsCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public class NeedReloginData
        {
            public Account Account { get; set; }
        }

        public AccountSettingsPageViewModel()
        {
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAndGoBackAsync, () => IsApplyButtonEnabled);
            SignInCommand = new AsyncRelayCommand(SignInAsync, () => IsSignInButtonEnabled);
            RemoveAccountCommand = new AsyncRelayCommand(RemoveAccountAndGoBackAsync, () => IsRemoveButtonEnabled);
            CreateHybridAddress = new AsyncRelayCommand(CreateHybridAddressAsync, () => !IsHybridAddress);
            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account accountData)
                {
                    InitModel(AccountSettingsModel.Create(accountData), false);
                }
                else if (data is OAuth2.MailService mailService && mailService != OAuth2.MailService.Unknown)
                {
                    InitModel(new OAuth2AccountSettingsModel(DefaultAccountConfig.CreateDefaultOAuth2Account(mailService)), true);

                    await LoginAsync(mailService).ConfigureAwait(true);
                }
                else if (data is NeedReloginData needReloginData)
                {
                    // It looks like the authorization data is out of date, you need to ask the user to authorization again.

                    var account = needReloginData.Account;
                    InitModel(AccountSettingsModel.Create(account), false);

                    if (account.AuthData is OAuth2Data oAuth2Data)
                    {
                        await LoginAsync(GetMailService(oAuth2Data.AuthAssistantId), account.Email.Address).ConfigureAwait(true);
                    }
                    else
                    {
                        NotifyToCheckEmailAndPasswordFields();
                    }
                }
                else
                {
                    InitModel(new BasicAccountSettingsModel(Account.Default), true);
                }

                // TVM-349 ValidationModel reorganization
                // Validate() - hides all errors and only checks for empty lines
                // the same happens when a field is changed, Validate() is called there

                // AccountSettingsModel.Validate();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void InitModel(AccountSettingsModel accountSettingsModel, bool isCreatingMode)
        {
            IsCreatingAccountMode = isCreatingMode;
            accountSettingsModel.Email.NeedsValidation = !isCreatingMode;
            accountSettingsModel.OutgoingServerAddress.NeedsValidation = !isCreatingMode;
            accountSettingsModel.IncomingServerAddress.NeedsValidation = !isCreatingMode;

            AccountSettingsModel = accountSettingsModel;
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

        private async Task SignInAsync()
        {
            try
            {
                AccountSettingsModel.Email.Errors.Clear();

                var account = AccountSettingsModel.ToAccount();
                if (account.AuthData is OAuth2Data oAuth2Data)
                {
                    await LoginAsync(GetMailService(oAuth2Data.AuthAssistantId), account.Email.Address).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        CancellationTokenSource _cts;

        private async Task<bool> ApplyAccountSettingsAsync(Account accountData)
        {
            if ((!IsEmailReadonly && string.IsNullOrEmpty(AccountSettingsModel.Email.Value))
                || string.IsNullOrEmpty(AccountSettingsModel.OutgoingServerAddress.Value)
                || string.IsNullOrEmpty(AccountSettingsModel.IncomingServerAddress.Value))
            {
                // Validator will block Apply button and show notification
                // if Email, Outgoing server address or Incoming server address 
                // fields are empty even if values were not entered
                AccountSettingsModel.Email.NeedsValidation = true;
                AccountSettingsModel.OutgoingServerAddress.NeedsValidation = true;
                AccountSettingsModel.IncomingServerAddress.NeedsValidation = true;
                if (AccountSettingsModel is BasicAccountSettingsModel basicAccountSettingsModel)
                {
                    basicAccountSettingsModel.Password.NeedsValidation = true;
                }
                ValidateProperty(AccountSettingsModel, nameof(AccountSettingsModel));
                return false;
            }

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

        private async Task<bool> CheckEmailAccountAsync(Account accountData, CancellationToken cancellationToken = default)
        {
            bool[] result = await Task<bool[]>.WhenAll<bool>(
                CheckIncomingMailServerAsync(accountData, cancellationToken),
                CheckOutgoingMailServerAsync(accountData, cancellationToken)).ConfigureAwait(true);

            return result[0] && result[1];
        }

        private async Task<bool> CheckIncomingMailServerAsync(Account accountData, CancellationToken cancellationToken = default)
        {
            AccountSettingsModel.IncomingServerAddress.Errors.Clear();

            try
            {
                var credentialsProvider = Core.CredentialsManager.CreateCredentialsProvider(accountData);
                await Core.TestMailServerAsync(accountData.IncomingServerAddress, accountData.IncomingServerPort, accountData.IncomingMailProtocol, credentialsProvider, cancellationToken).ConfigureAwait(true);
                return true;
            }
            catch (AuthenticationException)
            {
                // Either the email and password are incorrect, or IMAP access is disabled.
                // Unambiguous definition fails. With MailKit comes one bug for these two cases.
                // But as an option: if Web authentication was used, then the email and password are correct,
                // and perhaps the problem is disabled IMAP access.
                if (IsBasicAccount)
                {
                    AccountSettingsModel.IncomingServerAddress.Errors.Add(GetLocalizedString("AuthenticationError"));
                    NotifyToCheckEmailAndPasswordFields();
                }
                else
                {
                    AccountSettingsModel.IncomingServerAddress.Errors.Add(GetLocalizedString("AccessDenied"));
                    await MessageService.ShowEnableImapMessageAsync(AccountSettingsModel.Email.Value).ConfigureAwait(true);
                }
            }
            catch (ConnectionException)
            {
                AccountSettingsModel.IncomingServerAddress.Errors.Add(GetLocalizedString("ConnectionError"));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnError(new Exception(AccountSettingsModel.IncomingServerAddress.Value + "\n" + ex.Message));
            }
            return false;
        }

        private void NotifyToCheckEmailAndPasswordFields()
        {
            AccountSettingsModel.Email.Errors.Clear();
            AccountSettingsModel.Email.Errors.Add(GetLocalizedString("CheckEmailNotification"));

            if (AccountSettingsModel is BasicAccountSettingsModel basicAccountModel)
            {
                basicAccountModel.Password.Errors.Clear();
                basicAccountModel.Password.Errors.Add(GetLocalizedString("CheckPasswordNotification"));
            }
        }

        private async Task<bool> CheckOutgoingMailServerAsync(Account accountData, CancellationToken cancellationToken = default)
        {
            AccountSettingsModel.OutgoingServerAddress.Errors.Clear();

            try
            {
                var credentialsProvider = Core.CredentialsManager.CreateCredentialsProvider(accountData);
                await Core.TestMailServerAsync(accountData.OutgoingServerAddress, accountData.OutgoingServerPort, accountData.OutgoingMailProtocol, credentialsProvider, cancellationToken).ConfigureAwait(true);
                return true;
            }
            catch (AuthenticationException)
            {
                // When IMAP is disabled in Yandex mail, for some reason, verification via SMTP also fails
                // If web authentication was used, then the email and password are correct
                if (IsBasicAccount)
                {
                    AccountSettingsModel.OutgoingServerAddress.Errors.Add(GetLocalizedString("AuthenticationError"));
                    NotifyToCheckEmailAndPasswordFields();
                }
            }
            catch (ConnectionException)
            {
                AccountSettingsModel.OutgoingServerAddress.Errors.Add(GetLocalizedString("ConnectionError"));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnError(new Exception(AccountSettingsModel.OutgoingServerAddress.Value + "\n" + ex.Message));
            }
            return false;
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

        public void OnAuthorizationCompleted(IUserProfile userProfile, OAuth2.MailService mailService, Token token)
        {
            if (userProfile is null)
            {
                throw new ArgumentNullException(nameof(userProfile));
            }

            AccountSettingsModel.Email.Value = userProfile.Email;

            if (string.IsNullOrEmpty(AccountSettingsModel.SenderName))
            {
                AccountSettingsModel.SenderName = userProfile.DisplayName;
            }

            if (AccountSettingsModel is OAuth2AccountSettingsModel model)
            {
                model.RefreshToken = token.RefreshToken;
                model.AuthAssistantId = mailService.ToString();
            }
        }

        public void OnAuthenticateCanceled()
        {
            GoBack();
        }

        public void OnAuthorizationError(Exception ex)
        {
            OnError(ex);
            GoBack();
        }

        private async Task CreateHybridAddressAsync()
        {
            await Core.CreateHybridEmailAsync(new EmailAddress(AccountSettingsModel.Email.Value)).ConfigureAwait(true);

            GoBack();
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

        private static OAuth2.MailService GetMailService(string mailServiceName)
        {
            if (Enum.TryParse(mailServiceName, out OAuth2.MailService mailService))
            {
                return mailService;
            }

            throw new ArgumentException($"'{mailServiceName}' email service is not supported.", nameof(mailServiceName));
        }

        private async Task LoginAsync(OAuth2.MailService mailService, string userId = null)
        {
            try
            {
                IsSignInButtonEnabled = false;
                var authClient = AuthProvider.CreateAuthorizationClient(mailService);
                Token token = string.IsNullOrEmpty(userId) ? await authClient.LoginAsync().ConfigureAwait(true)
                                                           : await authClient.LoginAsync(userId).ConfigureAwait(true);

                if (authClient is IProfileReader profileReader)
                {
                    IUserProfile profile = await profileReader.ReadProfileAsync(token).ConfigureAwait(true);

                    if (IsUserCorrect(profile, userId))
                    {
                        IsSignInButtonVisible = false;
                        OnAuthorizationCompleted(profile, mailService, token);
                    }
                    else
                    {
                        var errorFormat = GetLocalizedString("EmailDoesntMatchNotification");
                        var errorNotification = string.Format(errorFormat, profile.Email ?? string.Empty);

                        OnAuthorizationFailed(errorNotification);
                    }
                }
                else
                {
                    throw new InvalidOperationException("User profile cannot be read.");
                }
            }
            // ToDo: Auth-new - WIP
            //catch (global::Auth.Interfaces.Types.Exceptions.AuthCancelException)
            //{
            //    // Authenticate window was closed

            //    OnAuthenticateCanceled();
            //}
            //catch (global::Auth.Interfaces.Types.Exceptions.AuthProtocolAccessDeniedException)
            //{
            //    // AuthProtocolAccessDeniedException it's possibly:
            //    // 1) the user refused to enter a login or password
            //    // 2) the user refused to grant rights

            //    OnAccessDenied();
            //}
            //catch (global::Auth.Interfaces.Types.Exceptions.AuthProtocolErrorException ex)
            //{
            //    OnAuthenticateError(ex);
            //}
            catch (Exception ex)
            {
                OnAuthorizationError(ex);
            }
        }

        private void OnAccessDenied()
        {
            var errorNotification = GetLocalizedString("AccountAccessDeniedNotification");
            OnAuthorizationFailed(errorNotification);
        }

        private void OnAuthorizationFailed(string errorNotification)
        {
            AccountSettingsModel.Email.Errors.Add(errorNotification);

            IsSignInButtonEnabled = true;
            IsSignInButtonVisible = true;
        }

        private static bool IsUserCorrect(IUserProfile profile, string userId)
        {
            return string.IsNullOrEmpty(userId) || string.Equals(profile?.Email, userId, StringComparison.OrdinalIgnoreCase);
        }

        public static ValidationResult ClearValidationErrors(AccountSettingsModel accountModel, ValidationContext _)
        {
            if (accountModel != null)
            {
                accountModel.Email.Errors.Clear();
                accountModel.OutgoingServerAddress.Errors.Clear();
                accountModel.IncomingServerAddress.Errors.Clear();
                accountModel.SynchronizationInterval.Errors.Clear();

                if (accountModel is BasicAccountSettingsModel basicAccountModel)
                {
                    basicAccountModel.Password.Errors.Clear();
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateEmailIsNotEmpty(AccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is AccountSettingsPageViewModel viewModel &&
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

        public static ValidationResult ValidateOutgoingServerAddressIsNotEmpty(AccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is AccountSettingsPageViewModel viewModel)
            {
                if (accountModel != null &&
                    accountModel.OutgoingServerAddress.NeedsValidation &&
                    string.IsNullOrEmpty(accountModel.OutgoingServerAddress.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    accountModel.OutgoingServerAddress.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateIncomingServerAddressIsNotEmpty(AccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is AccountSettingsPageViewModel viewModel)
            {
                if (accountModel != null &&
                    accountModel.IncomingServerAddress.NeedsValidation &&
                    string.IsNullOrEmpty(accountModel.IncomingServerAddress.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    accountModel.IncomingServerAddress.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateSynchronizationIntervalIsCorrect(AccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is AccountSettingsPageViewModel viewModel)
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
    }
}
