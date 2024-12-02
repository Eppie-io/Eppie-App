using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ProtonAccountSettingsModel : BaseAccountSettingsModel
    {
        public ValidatableProperty<string> TwoFactorCode { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> Password { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> MailboxPassword { get; } = new ValidatableProperty<string>();

        public ProtonAccountSettingsModel() : base()
        {
        }

        protected ProtonAccountSettingsModel(Account account) : base(account)
        {
            Password.SetInitialValue(string.Empty);
            TwoFactorCode.SetInitialValue(string.Empty);
            MailboxPassword.SetInitialValue(string.Empty);

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
        }

        public Account ToAccount()
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
}
