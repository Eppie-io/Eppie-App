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
            CurrentAccount.Email = new EmailAddress(Email.Value, SenderName);

            CurrentAccount.IsBackupAccountSettingsEnabled = IsBackupAccountSettingsEnabled;
            CurrentAccount.IsBackupAccountMessagesEnabled = IsBackupAccountMessagesEnabled;
            CurrentAccount.SynchronizationInterval = int.TryParse(SynchronizationInterval.Value, out int interval)
                ? interval
                : DefaultSynchronizationInterval;
            CurrentAccount.Type = (int)MailBoxType.Email;

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
