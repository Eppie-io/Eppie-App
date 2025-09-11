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
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;
using Tuvi.App.ViewModels.Extensions;
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

        private struct UserProfile
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public bool IsFilled => !string.IsNullOrEmpty(Email);
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

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account accountData)
                {
                    await InitModelAsync(AccountSettingsModel.Create(accountData), false).ConfigureAwait(true);
                }
                else if (data is OAuth2.MailService mailService && mailService != OAuth2.MailService.Unknown)
                {
                    await InitModelAsync(new OAuth2AccountSettingsModel(DefaultAccountConfig.CreateDefaultOAuth2Account(mailService)), true).ConfigureAwait(true);

                    await LoginAsync(mailService).ConfigureAwait(true);
                }
                else if (data is NeedReloginData needReloginData)
                {
                    // It looks like the authorization data is out of date, you need to ask the user to authorization again.

                    var account = needReloginData.Account;
                    await InitModelAsync(AccountSettingsModel.Create(account), false).ConfigureAwait(true);

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
                    await InitModelAsync(new BasicAccountSettingsModel(Account.Default), true).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task InitModelAsync(AccountSettingsModel accountSettingsModel, bool isCreatingMode)
        {
            IsCreatingAccountMode = isCreatingMode;
            AccountSettingsModel = accountSettingsModel;

            AccountSettingsModel.Email.NeedsValidation = !isCreatingMode;
            AccountSettingsModel.OutgoingServerAddress.NeedsValidation = !isCreatingMode;
            AccountSettingsModel.IncomingServerAddress.NeedsValidation = !isCreatingMode;

            await AccountSettingsModel.InitModelAsync(CoreProvider, NavigationService).ConfigureAwait(true);
        }

        protected override bool IsValid()
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
                    basicAccountSettingsModel.IncomingLogin.NeedsValidation = true;
                    basicAccountSettingsModel.IncomingPassword.NeedsValidation = true;
                    basicAccountSettingsModel.OutgoingLogin.NeedsValidation = true;
                    basicAccountSettingsModel.OutgoingPassword.NeedsValidation = true;
                }
                ValidateProperty(AccountSettingsModel, nameof(AccountSettingsModel));
                return false;
            }

            return true;
        }

        protected override Account AccountSettingsModelToAccount()
        {
            return AccountSettingsModel.ToAccount();
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
                    basicAccountModel.IncomingLogin.Errors.Clear();
                    basicAccountModel.IncomingPassword.Errors.Clear();
                    basicAccountModel.OutgoingLogin.Errors.Clear();
                    basicAccountModel.OutgoingPassword.Errors.Clear();
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

        public bool IsBasicAccount
        {
            get { return AccountSettingsModel is BasicAccountSettingsModel; }
        }

        private Array _incomingProtocolTypes;
        public Array IncomingProtocolTypes
        {
            get
            {
                if (_incomingProtocolTypes is null)
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
                AuthCredential credential = string.IsNullOrEmpty(userId) ? await authClient.LoginAsync().ConfigureAwait(true)
                                                                         : await authClient.LoginAsync(userId).ConfigureAwait(true);

                UserProfile profile = await ReadUserProfileAsync(authClient, credential).ConfigureAwait(true);

                if (profile.IsFilled)
                {
                    if (IsUserCorrect(profile, userId))
                    {
                        OnAuthorizationCompleted(profile, mailService, credential);
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

        private static async Task<UserProfile> ReadUserProfileAsync(IAuthorizationClient client, AuthCredential credential)
        {
            string email;
            string name;

            if (client is IProfileReader profileReader)
            {
                IUserProfile profile = await profileReader.ReadProfileAsync(credential).ConfigureAwait(false);
                name = profile.DisplayName;
                email = profile.Email;
            }
            else
            {
                name = credential.ReadName();
                email = credential.ReadEmailAddress();
            }

            return new UserProfile
            {
                Name = name,
                Email = email
            };
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

        private static bool IsUserCorrect(UserProfile profile, string userId)
        {
            return string.IsNullOrEmpty(userId) || string.Equals(profile.Email, userId, StringComparison.OrdinalIgnoreCase);
        }

        private void OnAuthorizationCompleted(UserProfile userProfile, OAuth2.MailService mailService, Credential credential)
        {
            AccountSettingsModel.Email.Value = userProfile.Email;

            if (string.IsNullOrEmpty(AccountSettingsModel.SenderName.Value))
            {
                AccountSettingsModel.SenderName.Value = userProfile.Name;
            }

            if (AccountSettingsModel is OAuth2AccountSettingsModel model)
            {
                model.RefreshToken = credential.RefreshToken;
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

        private async Task<bool> CheckOutgoingMailServerAsync(Account accountData, CancellationToken cancellationToken = default)
        {
            AccountSettingsModel.OutgoingServerAddress.Errors.Clear();

            try
            {
                var credentialsProvider = Core.CredentialsManager.CreateOutgoingCredentialsProvider(accountData);
                await Core.TestMailServerAsync(accountData.OutgoingServerAddress, accountData.OutgoingServerPort, accountData.OutgoingMailProtocol, credentialsProvider, cancellationToken).ConfigureAwait(true);
                return true;
            }
            catch (ConnectionException)
            {
                AccountSettingsModel.OutgoingServerAddress.Errors.Add(GetLocalizedString("ConnectionError"));
            }
            catch (AuthenticationException)
            {
                AccountSettingsModel.OutgoingServerAddress.Errors.Add(GetLocalizedString("AuthenticationError"));
                NotifyToCheckEmailAndPasswordFields();
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

        protected override async Task<bool> CheckEmailAccountAsync(Account accountData, CancellationToken token = default)
        {
            if (accountData is null)
            {
                throw new ArgumentNullException(nameof(accountData));
            }

            bool[] result = await Task<bool[]>.WhenAll<bool>(
                CheckIncomingMailServerAsync(accountData, token),
                CheckOutgoingMailServerAsync(accountData, token)).ConfigureAwait(true);

            return result[0] && result[1];
        }

        private async Task<bool> CheckIncomingMailServerAsync(Account accountData, CancellationToken cancellationToken = default)
        {
            AccountSettingsModel.IncomingServerAddress.Errors.Clear();

            try
            {
                var credentialsProvider = Core.CredentialsManager.CreateIncomingCredentialsProvider(accountData);
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

                if (basicAccountModel.UseSeparateIncomingCredentials)
                {
                    basicAccountModel.IncomingLogin.Errors.Clear();
                    basicAccountModel.IncomingLogin.Errors.Add(GetLocalizedString("CheckEmailNotification"));

                    basicAccountModel.IncomingPassword.Errors.Clear();
                    basicAccountModel.IncomingPassword.Errors.Add(GetLocalizedString("CheckPasswordNotification"));
                }

                if (basicAccountModel.UseSeparateOutgoingCredentials)
                {
                    basicAccountModel.OutgoingLogin.Errors.Clear();
                    basicAccountModel.OutgoingLogin.Errors.Add(GetLocalizedString("CheckEmailNotification"));

                    basicAccountModel.OutgoingPassword.Errors.Clear();
                    basicAccountModel.OutgoingPassword.Errors.Add(GetLocalizedString("CheckPasswordNotification"));
                }
            }
        }
    }
}
