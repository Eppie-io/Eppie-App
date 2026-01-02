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
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Validation;
using Tuvi.Auth.Proton.Exceptions;
using Tuvi.Core.Entities;
using Tuvi.Proton.Primitive.Messages.Errors;

namespace Tuvi.App.ViewModels
{
    public enum ProtonConnectionStep
    {
        Unknown,
        Credentials,
        HumanVerifier,
        TwoFactorCode,
        UnlockMailbox,
        Done,
        OpenSettings,
    }

    public class ConnectProtonAddressPageViewModel : ProtonAddressSettingsPageViewModel
    {
        private ProtonConnectionStep _step;
        public ProtonConnectionStep Step
        {
            get => _step;
            private set => SetProperty(ref _step, value);
        }

        public ValidatableProperty<string> Email { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> Password { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> TwoFactorCode { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> MailboxPassword { get; } = new ValidatableProperty<string>();

        private bool _isProcess;
        public bool IsProcess
        {
            get => _isProcess;
            private set
            {
                if (SetProperty(ref _isProcess, value))
                {
                    ContinueCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        public IRelayCommand ContinueCommand { get; }
        public IAsyncRelayCommand OpenSettingsCommand => new AsyncRelayCommand(OnOpenSettings);
        public IRelayCommand ClosedCommand => new RelayCommand(OnClosed);
        public IRelayCommand DoneCommand => new RelayCommand(OnDone);

        public Action ClosePopupAction { get; set; }
        private Account AccountData { get; set; }

        private event EventHandler<TwoFactorCodeEventArgs> TwoFactorCodeProvided;
        private event EventHandler<MailboxPasswordEventArgs> MailboxPasswordProvided;

        public ConnectProtonAddressPageViewModel() : base()
        {
            IsProcess = false;
            Step = ProtonConnectionStep.Unknown;

            ContinueCommand = new RelayCommand(OnContinue, CanContinue);

            Email.Errors.CollectionChanged += (s, e) => ContinueCommand.NotifyCanExecuteChanged();
            Password.Errors.CollectionChanged += (s, e) => ContinueCommand.NotifyCanExecuteChanged();
            TwoFactorCode.Errors.CollectionChanged += (s, e) => ContinueCommand.NotifyCanExecuteChanged();
            MailboxPassword.Errors.CollectionChanged += (s, e) => ContinueCommand.NotifyCanExecuteChanged();
        }

        public override void OnNavigatedTo(object data)
        {
            if (data is Account accountData)
            {
                AccountData = accountData;
                Email.SetInitialValue(accountData.Email?.Address);
            }
            ShowStep(ProtonConnectionStep.Credentials);

            base.OnNavigatedTo(data);
        }

        private async void OnContinue()
        {
            try
            {
                IsProcess = true;
                switch (Step)
                {
                    case ProtonConnectionStep.Credentials:
                        if (IsPropertyValid(Email))
                        {
                            await ConnectAccountAsync(AccountData != null).ConfigureAwait(true);
                        }
                        else
                        {
                            IsProcess = false;
                        }
                        break;
                    case ProtonConnectionStep.TwoFactorCode:
                        if (IsPropertyValid(TwoFactorCode))
                        {
                            TwoFactorCodeProvided?.Invoke(this, new TwoFactorCodeEventArgs(false, TwoFactorCode.Value));
                        }
                        else
                        {
                            IsProcess = false;
                        }
                        break;
                    case ProtonConnectionStep.UnlockMailbox:
                        if (IsPropertyValid(MailboxPassword))
                        {
                            MailboxPasswordProvided?.Invoke(this, new MailboxPasswordEventArgs(false, MailboxPassword.Value));
                        }
                        else
                        {
                            IsProcess = false;
                        }
                        break;
                    case ProtonConnectionStep.HumanVerifier:
                        // Todo: Issue #479 add human verification page
                        throw new NotImplementedException();
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex) when ((ex is AuthenticationException) && !IsHumanVerificationException(ex))
            {
                ShowStep(ProtonConnectionStep.Credentials, true);
            }
            catch (Exception ex)
            {
                ForceClosePopup();
                OnError(ex);
            }

            bool IsHumanVerificationException(Exception ex)
            {
                return ex.InnerException is AuthUnsuccessProtonException unsuccess && unsuccess.Response.IsHumanVerificationRequired();
            }
        }

        private void ValidateProperty(ValidatableProperty<string> property)
        {
            DispatcherService?.RunAsync(() =>
            {
                property.Errors.Clear();

                if (!property.NeedsValidation)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(property.Value))
                {
                    var error = GetLocalizedString("FieldIsEmptyNotification");
                    property.Errors.Add(error);
                    return;
                }
            }).ConfigureAwait(false);
        }

        private bool CanContinue()
        {
            if (IsProcess)
            {
                return false;
            }

            switch (Step)
            {
                case ProtonConnectionStep.Credentials:
                    return !(Email.NeedsValidation && Email.HasErrors) && !(Password.NeedsValidation && Password.HasErrors);
                case ProtonConnectionStep.TwoFactorCode:
                    return !(TwoFactorCode.NeedsValidation && TwoFactorCode.HasErrors);
                case ProtonConnectionStep.UnlockMailbox:
                    return !(MailboxPassword.NeedsValidation && MailboxPassword.HasErrors);
                default:
                    return true;
            }
        }

        private bool IsPropertyValid(ValidatableProperty<string> property)
        {
            property.NeedsValidation = true;
            ValidateProperty(property);
            ContinueCommand.NotifyCanExecuteChanged();

            return !property.HasErrors;
        }

        private async Task OnOpenSettings()
        {
            var account = await Core.GetAccountAsync(new EmailAddress(Email.Value)).ConfigureAwait(true);
            NavigateToMailboxSettingsPage(account, false);
            DoneCommand.Execute(null);
        }

        private void OnDone()
        {
            ClosePopupAction?.Invoke();
        }

        private void OnClosed()
        {
            TwoFactorCodeProvided?.Invoke(this, new TwoFactorCodeEventArgs(true, string.Empty));
            MailboxPasswordProvided?.Invoke(this, new MailboxPasswordEventArgs(true, string.Empty));
        }

        private async Task ConnectAccountAsync(bool reconnect)
        {
            bool exists = await IsAccountExistAsync(Email.Value).ConfigureAwait(false);
            if (exists && !reconnect)
            {
                ShowStep(ProtonConnectionStep.OpenSettings);
                return;
            }

            var account = await LoginAsync().ConfigureAwait(false);
            await ProcessAccountAsync(account).ConfigureAwait(false);
            ShowStep(ProtonConnectionStep.Done);
        }

        private async Task<Account> LoginAsync()
        {
            var (userId, refreshToken, saltedKeyPass) = await Proton.ClientAuth.LoginFullAsync
            (
                Email.Value,
                Password.Value,
                ProvideTwoFactorCode,
                ProvideMailboxPassword,
                null, // Todo: Issue #479 add human verification page
                default
            ).ConfigureAwait(true);

            return CreateOrUpdateProtonAccount(userId, refreshToken, saltedKeyPass);
        }

        private Task<(bool completed, string code)> ProvideTwoFactorCode(Exception previousAttemptException, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<(bool, string)>();

            TwoFactorCodeProvided += OnEvent;

            ShowStep(ProtonConnectionStep.TwoFactorCode, previousAttemptException != null);

            return tcs.Task;

            void OnEvent(object sender, TwoFactorCodeEventArgs eventArgs)
            {
                TwoFactorCodeProvided -= OnEvent;
                tcs.SetResult((!eventArgs.IsCanceled, eventArgs.Code ?? string.Empty));
            }
        }

        private Task<(bool completed, string password)> ProvideMailboxPassword(Exception previousAttemptException, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<(bool, string)>();

            MailboxPasswordProvided += OnEvent;

            ShowStep(ProtonConnectionStep.UnlockMailbox, previousAttemptException != null);

            return tcs.Task;

            void OnEvent(object sender, MailboxPasswordEventArgs eventArgs)
            {
                MailboxPasswordProvided -= OnEvent;
                tcs.SetResult((!eventArgs.IsCanceled, eventArgs.Password ?? string.Empty));
            }
        }

        private async void ShowStep(ProtonConnectionStep step, bool error = false)
        {
            try
            {
                await DispatcherService.RunAsync(() =>
                {
                    IsProcess = false;
                    Password.Value = string.Empty;
                    TwoFactorCode.Value = string.Empty;
                    MailboxPassword.Value = string.Empty;

                    Step = step;

                    if (Step == ProtonConnectionStep.OpenSettings)
                    {
                        OpenSettingsCommand.Execute(null);
                    }

                    UpdateAuthenticationErrors(error);

                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ForceClosePopup();
                OnError(ex);
            }
        }

        private void UpdateAuthenticationErrors(bool error)
        {
            if (error)
            {
                var err = GetLocalizedString("AuthenticationError");
                Password.Errors.Add(err);
                TwoFactorCode.Errors.Add(err);
                MailboxPassword.Errors.Add(err);
            }
            else
            {
                Password.Errors.Clear();
                TwoFactorCode.Errors.Clear();
                MailboxPassword.Errors.Clear();
            }
        }

        private async void ForceClosePopup()
        {
            try
            {
                await DispatcherService.RunAsync(() =>
                {
                    ClosePopupAction?.Invoke();
                    TwoFactorCodeProvided?.Invoke(this, new TwoFactorCodeEventArgs(true, string.Empty));
                    MailboxPasswordProvided?.Invoke(this, new MailboxPasswordEventArgs(true, string.Empty));

                }).ConfigureAwait(false);
            }
            catch
            { }
        }

        private Account CreateOrUpdateProtonAccount(string userId, string refreshToken, string saltedKeyPass)
        {
            if (AccountData is null)
            {
                AccountData = Account.Default;
                AccountData.Type = MailBoxType.Proton;
                AccountData.Email = new EmailAddress(Email.Value);
            }

            AccountData.AuthData = new ProtonAuthData()
            {
                UserId = userId,
                RefreshToken = refreshToken,
                SaltedPassword = saltedKeyPass
            };

            return AccountData;
        }

        private Task<bool> IsAccountExistAsync(string email, CancellationToken cancellationToken = default)
        {
            return Core.ExistsAccountWithEmailAddressAsync(new EmailAddress(email), cancellationToken);
        }

        private Task ProcessAccountAsync(Account account, CancellationToken cancellationToken = default)
        {
            return ProcessAccountDataAsync(account, cancellationToken);
        }

        private class TwoFactorCodeEventArgs
        {
            public bool IsCanceled { get; private set; }
            public string Code { get; private set; }

            public TwoFactorCodeEventArgs(bool isCanceled, string code)
            {
                IsCanceled = isCanceled;
                Code = code;
            }
        }

        private class MailboxPasswordEventArgs
        {
            public bool IsCanceled { get; private set; }
            public string Password { get; private set; }

            public MailboxPasswordEventArgs(bool isCanceled, string password)
            {
                IsCanceled = isCanceled;
                Password = password;
            }
        }
    }
}
