using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;
using Tuvi.Core.Entities;
using Tuvi.OAuth2;

namespace Tuvi.App.ViewModels
{
    public class AccountSettingsPageViewModel : BaseAccountSettingsPageViewModel
    {
        public class NeedReloginData
        {
            public Account Account { get; set; }
        }

        public AuthorizationProvider AuthProvider { get; private set; }

        public void SetAuthProvider(AuthorizationProvider authProvider)
        {
            AuthProvider = authProvider;
        }

        private AccountSettingsModel _accountSettingsModel;
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ValidateOutgoingServerAddressIsNotEmpty))]
        [CustomValidation(typeof(AccountSettingsPageViewModel), nameof(ValidateIncomingServerAddressIsNotEmpty))]
        [CustomValidation(typeof(BaseAccountSettingsPageViewModel), nameof(ValidateEmailIsNotEmpty))]
        [CustomValidation(typeof(BaseAccountSettingsPageViewModel), nameof(ValidateSynchronizationIntervalIsCorrect))]
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

        public AccountSettingsPageViewModel() : base()
        {
            CreateHybridAddress = new AsyncRelayCommand(CreateHybridAddressAsync, () => !IsHybridAddress);
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
            AccountSettingsModel = accountSettingsModel;

            accountSettingsModel.Email.NeedsValidation = !isCreatingMode;
            accountSettingsModel.OutgoingServerAddress.NeedsValidation = !isCreatingMode;
            accountSettingsModel.IncomingServerAddress.NeedsValidation = !isCreatingMode;
        }

        protected async override Task ApplySettingsAndGoBackAsync()
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
                return;
            }

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

        protected override Account AccountSettingsModelToAccount()
        {
            return AccountSettingsModel.ToAccount();
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

        public static ValidationResult ClearValidationErrors(AccountSettingsModel accountModel, ValidationContext _)
        {
            if (accountModel != null)
            {
                accountModel.Email.Errors.Clear();
                accountModel.SynchronizationInterval.Errors.Clear();

                if (accountModel is BasicAccountSettingsModel basicAccountModel)
                {
                    basicAccountModel.Password.Errors.Clear();
                }
                accountModel.OutgoingServerAddress.Errors.Clear();
                accountModel.IncomingServerAddress.Errors.Clear();
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

        public IRelayCommand CreateHybridAddress { get; }

        public bool IsBasicAccount
        {
            get { return AccountSettingsModel is BasicAccountSettingsModel; }
        }

        public bool ShowHybridAddressButton
        {
            get
            {
                return !IsHybridAddress && !IsCreatingAccountMode;
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
                var authClient = AuthProvider.CreateAuthorizationClient(mailService);
                Token token = string.IsNullOrEmpty(userId) ? await authClient.LoginAsync().ConfigureAwait(true)
                                                           : await authClient.LoginAsync(userId).ConfigureAwait(true);

                if (authClient is IProfileReader profileReader)
                {
                    IUserProfile profile = await profileReader.ReadProfileAsync(token).ConfigureAwait(true);

                    if (IsUserCorrect(profile, userId))
                    {
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
        }

        private static bool IsUserCorrect(IUserProfile profile, string userId)
        {
            return string.IsNullOrEmpty(userId) || string.Equals(profile?.Email, userId, StringComparison.OrdinalIgnoreCase);
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
    }
}
