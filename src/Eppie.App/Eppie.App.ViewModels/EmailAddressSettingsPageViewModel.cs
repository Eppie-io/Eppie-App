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
    public class EmailAddressSettingsPageViewModel : BaseAddressSettingsPageViewModel
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

        private bool _isAdvancedSettingsModeActive;
        public bool IsAdvancedSettingsModeActive
        {
            get => _isAdvancedSettingsModeActive;
            set => SetProperty(ref _isAdvancedSettingsModeActive, value);
        }

        private bool _shouldAutoExpandOutgoingServer;
        public bool ShouldAutoExpandOutgoingServer
        {
            get => _shouldAutoExpandOutgoingServer;
            set => SetProperty(ref _shouldAutoExpandOutgoingServer, value);
        }

        private bool _shouldAutoExpandIncomingServer;
        public bool ShouldAutoExpandIncomingServer
        {
            get => _shouldAutoExpandIncomingServer;
            set => SetProperty(ref _shouldAutoExpandIncomingServer, value);
        }

        public AuthorizationProvider AuthProvider { get; private set; }

        public void SetAuthProvider(AuthorizationProvider authProvider)
        {
            AuthProvider = authProvider;
        }

        private EmailAddressSettingsModel _addressSettingsModel;
        [CustomValidation(typeof(EmailAddressSettingsPageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(EmailAddressSettingsPageViewModel), nameof(ValidateOutgoingServerAddressIsNotEmpty))]
        [CustomValidation(typeof(EmailAddressSettingsPageViewModel), nameof(ValidateIncomingServerAddressIsNotEmpty))]
        [CustomValidation(typeof(BaseAddressSettingsPageViewModel), nameof(ValidateEmailIsNotEmpty))]
        [CustomValidation(typeof(BaseAddressSettingsPageViewModel), nameof(ValidateSynchronizationIntervalIsCorrect))]
        public EmailAddressSettingsModel AddressSettingsModel
        {
            get { return _addressSettingsModel; }
            set
            {
                if (_addressSettingsModel != null)
                {
                    _addressSettingsModel.PropertyChanged -= OnAddressSettingsModelPropertyChanged;
                }

                SetProperty(ref _addressSettingsModel, value, true);
                OnPropertyChanged(nameof(IsBasicAaddress));
                OnPropertyChanged(nameof(IsEmailReadonly));
                OnPropertyChanged(nameof(ShouldAutoExpandOutgoingServer));
                OnPropertyChanged(nameof(ShouldAutoExpandIncomingServer));

                if (_addressSettingsModel != null)
                {
                    _addressSettingsModel.PropertyChanged += OnAddressSettingsModelPropertyChanged;
                }
            }
        }

        protected override BaseAddressSettingsModel AddressSettingsModelBase => AddressSettingsModel;

        private void OnAddressSettingsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ValidateProperty(AddressSettingsModel, nameof(AddressSettingsModel));
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account accountData)
                {
                    await InitModelAsync(EmailAddressSettingsModel.Create(accountData), false).ConfigureAwait(true);
                }
                else if (data is OAuth2.MailService mailService && mailService != OAuth2.MailService.Unknown)
                {
                    await InitModelAsync(new OAuth2EmailAddressSettingsModel(DefaultAccountConfig.CreateDefaultOAuth2Account(mailService)), true).ConfigureAwait(true);

                    await LoginAsync(mailService).ConfigureAwait(true);
                }
                else if (data is NeedReloginData needReloginData)
                {
                    // It looks like the authorization data is out of date, you need to ask the user to authorization again.

                    var account = needReloginData.Account;
                    await InitModelAsync(EmailAddressSettingsModel.Create(account), false).ConfigureAwait(true);

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
                    await InitModelAsync(new BasicEmailAddressSettingsModel(Account.Default), true).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task InitModelAsync(EmailAddressSettingsModel addressSettingsModel, bool isCreatingMode)
        {
            IsCreatingAccountMode = isCreatingMode;
            AddressSettingsModel = addressSettingsModel;

            AddressSettingsModel.Email.NeedsValidation = !isCreatingMode;
            AddressSettingsModel.OutgoingServerAddress.NeedsValidation = !isCreatingMode;
            AddressSettingsModel.IncomingServerAddress.NeedsValidation = !isCreatingMode;

            await AddressSettingsModel.InitModelAsync(CoreProvider, NavigationService).ConfigureAwait(true);

            IsAdvancedSettingsModeActive = isCreatingMode && addressSettingsModel is BasicEmailAddressSettingsModel;
            ShouldAutoExpandOutgoingServer = ShouldAutoExpandIncomingServer = IsAdvancedSettingsModeActive;
        }

        protected override bool IsValid()
        {
            if ((!IsEmailReadonly && string.IsNullOrEmpty(AddressSettingsModel.Email.Value))
                || string.IsNullOrEmpty(AddressSettingsModel.OutgoingServerAddress.Value)
                || string.IsNullOrEmpty(AddressSettingsModel.IncomingServerAddress.Value))
            {
                // Validator will block Apply button and show notification
                // if Email, Outgoing server address or Incoming server address 
                // fields are empty even if values were not entered
                AddressSettingsModel.Email.NeedsValidation = true;
                AddressSettingsModel.OutgoingServerAddress.NeedsValidation = true;
                AddressSettingsModel.IncomingServerAddress.NeedsValidation = true;
                if (AddressSettingsModel is BasicEmailAddressSettingsModel basicAddressSettingsModel)
                {
                    basicAddressSettingsModel.Password.NeedsValidation = true;
                    basicAddressSettingsModel.IncomingLogin.NeedsValidation = true;
                    basicAddressSettingsModel.IncomingPassword.NeedsValidation = true;
                    basicAddressSettingsModel.OutgoingLogin.NeedsValidation = true;
                    basicAddressSettingsModel.OutgoingPassword.NeedsValidation = true;
                }
                ValidateProperty(AddressSettingsModel, nameof(AddressSettingsModel));
                return false;
            }

            return true;
        }

        protected override Account ApplySettingsToAccount()
        {
            return AddressSettingsModel.ToAccount();
        }

        public static ValidationResult ClearValidationErrors(EmailAddressSettingsModel addressSettingsModel, ValidationContext _)
        {
            if (addressSettingsModel != null)
            {
                addressSettingsModel.Email.Errors.Clear();
                addressSettingsModel.SynchronizationInterval.Errors.Clear();

                if (addressSettingsModel is BasicEmailAddressSettingsModel basicAddressSettingsModel)
                {
                    basicAddressSettingsModel.Password.Errors.Clear();
                    basicAddressSettingsModel.IncomingLogin.Errors.Clear();
                    basicAddressSettingsModel.IncomingPassword.Errors.Clear();
                    basicAddressSettingsModel.OutgoingLogin.Errors.Clear();
                    basicAddressSettingsModel.OutgoingPassword.Errors.Clear();
                }
                addressSettingsModel.OutgoingServerAddress.Errors.Clear();
                addressSettingsModel.IncomingServerAddress.Errors.Clear();
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateIncomingServerAddressIsNotEmpty(EmailAddressSettingsModel addressSettingsModel, ValidationContext context)
        {
            if (context?.ObjectInstance is EmailAddressSettingsPageViewModel viewModel)
            {
                if (addressSettingsModel != null &&
                    addressSettingsModel.IncomingServerAddress.NeedsValidation &&
                    string.IsNullOrEmpty(addressSettingsModel.IncomingServerAddress.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    addressSettingsModel.IncomingServerAddress.Errors.Add(error);

                    if (!viewModel.IsAdvancedSettingsModeActive)
                    {
                        viewModel.IsAdvancedSettingsModeActive = true;
                    }
                    viewModel.ShouldAutoExpandIncomingServer = true;

                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateOutgoingServerAddressIsNotEmpty(EmailAddressSettingsModel addressSettingsModel, ValidationContext context)
        {
            if (context?.ObjectInstance is EmailAddressSettingsPageViewModel viewModel)
            {
                if (addressSettingsModel != null &&
                    addressSettingsModel.OutgoingServerAddress.NeedsValidation &&
                    string.IsNullOrEmpty(addressSettingsModel.OutgoingServerAddress.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    addressSettingsModel.OutgoingServerAddress.Errors.Add(error);

                    if (!viewModel.IsAdvancedSettingsModeActive)
                    {
                        viewModel.IsAdvancedSettingsModeActive = true;
                    }
                    viewModel.ShouldAutoExpandOutgoingServer = true;

                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public bool IsBasicAaddress
        {
            get { return AddressSettingsModel is BasicEmailAddressSettingsModel; }
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
                        var errorNotification = string.Format(System.Globalization.CultureInfo.InvariantCulture, errorFormat, profile.Email ?? string.Empty);

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
            AddressSettingsModel.Email.Errors.Add(errorNotification);
        }

        private static bool IsUserCorrect(UserProfile profile, string userId)
        {
            return string.IsNullOrEmpty(userId) || string.Equals(profile.Email, userId, StringComparison.OrdinalIgnoreCase);
        }

        private void OnAuthorizationCompleted(UserProfile userProfile, OAuth2.MailService mailService, Credential credential)
        {
            AddressSettingsModel.Email.Value = userProfile.Email;

            if (string.IsNullOrEmpty(AddressSettingsModel.SenderName.Value))
            {
                AddressSettingsModel.SenderName.Value = userProfile.Name;
            }

            if (AddressSettingsModel is OAuth2EmailAddressSettingsModel model)
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
            AddressSettingsModel.OutgoingServerAddress.Errors.Clear();

            try
            {
                var credentialsProvider = Core.CredentialsManager.CreateOutgoingCredentialsProvider(accountData);
                await Core.TestMailServerAsync(accountData.OutgoingServerAddress, accountData.OutgoingServerPort, accountData.OutgoingMailProtocol, credentialsProvider, cancellationToken).ConfigureAwait(true);
                return true;
            }
            catch (ConnectionException)
            {
                AddressSettingsModel.OutgoingServerAddress.Errors.Add(GetLocalizedString("ConnectionError"));
            }
            catch (AuthenticationException)
            {
                AddressSettingsModel.OutgoingServerAddress.Errors.Add(GetLocalizedString("AuthenticationError"));
                NotifyToCheckEmailAndPasswordFields();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnError(new InvalidOperationException(AddressSettingsModel.OutgoingServerAddress.Value + "\n" + ex.Message, ex));
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
            AddressSettingsModel.IncomingServerAddress.Errors.Clear();

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
                if (IsBasicAaddress)
                {
                    AddressSettingsModel.IncomingServerAddress.Errors.Add(GetLocalizedString("AuthenticationError"));
                    NotifyToCheckEmailAndPasswordFields();
                }
                else
                {
                    AddressSettingsModel.IncomingServerAddress.Errors.Add(GetLocalizedString("AccessDenied"));
                    await MessageService.ShowEnableImapMessageAsync(AddressSettingsModel.Email.Value).ConfigureAwait(true);
                }
            }
            catch (ConnectionException)
            {
                AddressSettingsModel.IncomingServerAddress.Errors.Add(GetLocalizedString("ConnectionError"));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnError(new InvalidOperationException(AddressSettingsModel.IncomingServerAddress.Value + "\n" + ex.Message, ex));
            }
            return false;
        }

        private void NotifyToCheckEmailAndPasswordFields()
        {
            AddressSettingsModel.Email.Errors.Clear();
            AddressSettingsModel.Email.Errors.Add(GetLocalizedString("CheckEmailNotification"));

            if (AddressSettingsModel is BasicEmailAddressSettingsModel basicAddressModel)
            {
                basicAddressModel.Password.Errors.Clear();
                basicAddressModel.Password.Errors.Add(GetLocalizedString("CheckPasswordNotification"));

                if (basicAddressModel.UseSeparateIncomingCredentials)
                {
                    basicAddressModel.IncomingLogin.Errors.Clear();
                    basicAddressModel.IncomingLogin.Errors.Add(GetLocalizedString("CheckEmailNotification"));

                    basicAddressModel.IncomingPassword.Errors.Clear();
                    basicAddressModel.IncomingPassword.Errors.Add(GetLocalizedString("CheckPasswordNotification"));
                }

                if (basicAddressModel.UseSeparateOutgoingCredentials)
                {
                    basicAddressModel.OutgoingLogin.Errors.Clear();
                    basicAddressModel.OutgoingLogin.Errors.Add(GetLocalizedString("CheckEmailNotification"));

                    basicAddressModel.OutgoingPassword.Errors.Clear();
                    basicAddressModel.OutgoingPassword.Errors.Add(GetLocalizedString("CheckPasswordNotification"));
                }
            }
        }
    }
}
