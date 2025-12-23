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
        private const string IncorrectEmailExceptionText = "Email is incorrect (It is empty or already exists).";

        private ProtonConnectionStep _step;
        public ProtonConnectionStep Step
        {
            get => _step;
            private set => SetProperty(ref _step, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _twoFactorCode;
        public string TwoFactorCode
        {
            get => _twoFactorCode;
            set => SetProperty(ref _twoFactorCode, value);
        }

        private string _mailboxPassword;
        public string MailboxPassword
        {
            get => _mailboxPassword;
            set => SetProperty(ref _mailboxPassword, value);
        }

        private bool _isProcess;
        public bool IsProcess
        {
            get => _isProcess;
            private set => SetProperty(ref _isProcess, value);
        }

        public IRelayCommand ContinueCommand => new RelayCommand(OnContinue);
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
        }

        public override void OnNavigatedTo(object data)
        {
            if (data is Account accountData)
            {
                AccountData = accountData;
                Email = accountData.Email?.Address;
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
                        await ConnectAccountAsync(AccountData != null).ConfigureAwait(true);
                        break;
                    case ProtonConnectionStep.TwoFactorCode:
                        TwoFactorCodeProvided?.Invoke(this, new TwoFactorCodeEventArgs(false, TwoFactorCode));
                        break;
                    case ProtonConnectionStep.UnlockMailbox:
                        MailboxPasswordProvided?.Invoke(this, new MailboxPasswordEventArgs(false, MailboxPassword));
                        break;
                    case ProtonConnectionStep.HumanVerifier:
                        // Todo: Issue #479 add human verification page
                        throw new NotImplementedException();
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex) when ((IsEmailAddressException(ex) || ex is AuthenticationException) && !IsHumanVerificationException(ex))
            {
                ShowStep(ProtonConnectionStep.Credentials, true);
            }
            catch (Exception ex)
            {
                FroceClosePopup();
                OnError(ex);
            }

            bool IsHumanVerificationException(Exception ex)
            {
                return ex.InnerException is AuthUnsuccessProtonException unsuccess && unsuccess.Response.IsHumanVerificationRequired();
            }

            bool IsEmailAddressException(Exception ex)
            {
                return ex is InvalidOperationException && ex.Message == IncorrectEmailExceptionText;
            }
        }

        private async Task OnOpenSettings()
        {
            var account = await Core.GetAccountAsync(new EmailAddress(Email)).ConfigureAwait(true);
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
            bool incorrect = string.IsNullOrWhiteSpace(Email);
            if (incorrect)
            {
                // ToDo: maybe use more specific exception
                throw new InvalidOperationException(IncorrectEmailExceptionText);
            }

            bool exists = await IsAccountExistAsync(Email).ConfigureAwait(false);
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
                    Email,
                    Password,
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
                    Password = string.Empty;
                    TwoFactorCode = string.Empty;
                    MailboxPassword = string.Empty;

                    Step = step;

                    if (Step == ProtonConnectionStep.OpenSettings)
                    {
                        OpenSettingsCommand.Execute(null);
                    }

                    // Todo: show wrong input note in UI

                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                FroceClosePopup();
                OnError(ex);
            }
        }

        private async void FroceClosePopup()
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
                AccountData.Email = new EmailAddress(Email);
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
            _cts = new CancellationTokenSource();
            return ProcessAccountDataAsync(account, _cts.Token);
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
