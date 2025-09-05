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

using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class AccountSettingsModel : BaseAccountSettingsModel
    {
        public ValidatableProperty<string> OutgoingServerAddress { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> IncomingServerAddress { get; } = new ValidatableProperty<string>();

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

        public AccountSettingsModel() : base()
        {
            OutgoingServerAddress.SetInitialValue(string.Empty);
            IncomingServerAddress.SetInitialValue(string.Empty);
        }
        protected AccountSettingsModel(Account account) : base(account)
        {
            OutgoingServerAddress.SetInitialValue(account.OutgoingServerAddress);
            IncomingServerAddress.SetInitialValue(account.IncomingServerAddress);
            OutgoingServerPort = account.OutgoingServerPort;
            IncomingServerPort = account.IncomingServerPort;
            IncomingMailProtocol = account.IncomingMailProtocol;

            OutgoingServerAddress.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(OutgoingServerAddress), args.PropertyName);
            };
            IncomingServerAddress.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(IncomingServerAddress), args.PropertyName);
            };
        }

        public virtual Account ToAccount()
        {
            if (Email.Value is null)
            {
                return null;
            }

            CurrentAccount.Email = new EmailAddress(Email.Value, SenderName.Value);

            CurrentAccount.IsBackupAccountSettingsEnabled = IsBackupAccountSettingsEnabled;
            CurrentAccount.IsBackupAccountMessagesEnabled = IsBackupAccountMessagesEnabled;
            CurrentAccount.SynchronizationInterval = int.TryParse(SynchronizationInterval.Value, out int interval)
                ? interval
                : DefaultSynchronizationInterval;
            CurrentAccount.Type = MailBoxType.Email;

            CurrentAccount.OutgoingMailProtocol = MailProtocol.SMTP;
            CurrentAccount.OutgoingServerAddress = OutgoingServerAddress.Value;
            CurrentAccount.OutgoingServerPort = OutgoingServerPort;
            CurrentAccount.IncomingMailProtocol = IncomingMailProtocol;
            CurrentAccount.IncomingServerAddress = IncomingServerAddress.Value;
            CurrentAccount.IncomingServerPort = IncomingServerPort;

            return CurrentAccount;
        }

        public static AccountSettingsModel Create(Account account)
        {
            switch (account?.AuthData)
            {
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

        // Fields for separate SMTP login and password
        private bool _useSeparateOutgoingCredentials;
        public bool UseSeparateOutgoingCredentials
        {
            get => _useSeparateOutgoingCredentials;
            set => SetProperty(ref _useSeparateOutgoingCredentials, value);
        }
        public ValidatableProperty<string> OutgoingLogin { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> OutgoingPassword { get; } = new ValidatableProperty<string>();

        // Fields for separate IMAP/POP3 login and password
        private bool _useSeparateIncomingCredentials;
        public bool UseSeparateIncomingCredentials
        {
            get => _useSeparateIncomingCredentials;
            set => SetProperty(ref _useSeparateIncomingCredentials, value);
        }
        public ValidatableProperty<string> IncomingLogin { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> IncomingPassword { get; } = new ValidatableProperty<string>();

        public BasicAccountSettingsModel(Account account)
            : base(account)
        {
            if (account is null)
            {
                throw new System.ArgumentNullException(nameof(account));
            }

            if (account.AuthData is BasicAuthData basicData)
            {
                Password.SetInitialValue(basicData.Password);

                UseSeparateOutgoingCredentials = !string.IsNullOrEmpty(basicData.OutgoingLogin) || !string.IsNullOrEmpty(basicData.OutgoingPassword);
                if (UseSeparateOutgoingCredentials)
                {
                    OutgoingLogin.SetInitialValue(basicData.OutgoingLogin);
                    OutgoingPassword.SetInitialValue(basicData.OutgoingPassword);
                }
                else
                {
                    OutgoingLogin.SetInitialValue(string.Empty);
                    OutgoingPassword.SetInitialValue(string.Empty);
                }

                UseSeparateIncomingCredentials = !string.IsNullOrEmpty(basicData.IncomingLogin) || !string.IsNullOrEmpty(basicData.IncomingPassword);
                if (UseSeparateIncomingCredentials)
                {
                    IncomingLogin.SetInitialValue(basicData.IncomingLogin);
                    IncomingPassword.SetInitialValue(basicData.IncomingPassword);
                }
                else
                {
                    IncomingLogin.SetInitialValue(string.Empty);
                    IncomingPassword.SetInitialValue(string.Empty);
                }
            }
            else
            {
                Password.SetInitialValue(string.Empty);

                OutgoingLogin.SetInitialValue(string.Empty);
                OutgoingPassword.SetInitialValue(string.Empty);
                UseSeparateOutgoingCredentials = false;

                IncomingLogin.SetInitialValue(string.Empty);
                IncomingPassword.SetInitialValue(string.Empty);
                UseSeparateIncomingCredentials = false;
            }

            Password.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Password), args.PropertyName);
            };
            OutgoingLogin.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(OutgoingLogin), args.PropertyName);
            };
            OutgoingPassword.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(OutgoingPassword), args.PropertyName);
            };
            IncomingLogin.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(IncomingLogin), args.PropertyName);
            };
            IncomingPassword.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(IncomingPassword), args.PropertyName);
            };
            SynchronizationInterval.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SynchronizationInterval), args.PropertyName);
            };
        }

        public override Account ToAccount()
        {
            if (Email.Value is null)
            {
                return null;
            }

            CurrentAccount = base.ToAccount();
            var basicAuth = new BasicAuthData() { Password = Password.Value };

            if (UseSeparateOutgoingCredentials)
            {
                basicAuth.OutgoingLogin = OutgoingLogin.Value;
                basicAuth.OutgoingPassword = OutgoingPassword.Value;
            }

            if (UseSeparateIncomingCredentials)
            {
                basicAuth.IncomingLogin = IncomingLogin.Value;
                basicAuth.IncomingPassword = IncomingPassword.Value;
            }

            CurrentAccount.AuthData = basicAuth;

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
            if (account is null)
            {
                throw new System.ArgumentNullException(nameof(account));
            }

            if (account.AuthData is OAuth2Data oauth2Data)
            {
                RefreshToken = oauth2Data.RefreshToken;
                AuthAssistantId = oauth2Data.AuthAssistantId;
            }
        }

        public override Account ToAccount()
        {
            if (Email.Value is null)
            {
                return null;
            }

            CurrentAccount = base.ToAccount();
            CurrentAccount.AuthData = new OAuth2Data()
            {
                RefreshToken = RefreshToken,
                AuthAssistantId = AuthAssistantId
            };

            return CurrentAccount;
        }
    }
}
