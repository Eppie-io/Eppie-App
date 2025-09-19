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
    public class ProtonAddressSettingsModel : BaseAddressSettingsModel
    {
        public ValidatableProperty<string> TwoFactorCode { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> Password { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> MailboxPassword { get; } = new ValidatableProperty<string>();

        public ProtonAddressSettingsModel() : base()
        {
        }

        protected ProtonAddressSettingsModel(Account account) : base(account)
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
            if (Email.Value is null)
            {
                return null;
            }

            CurrentAccount.Email = new EmailAddress(Email.Value, SenderName.Value);

            CurrentAccount.AuthData = new BasicAuthData() { Password = Password.Value };
            CurrentAccount.IsBackupAccountSettingsEnabled = IsBackupAccountSettingsEnabled;
            CurrentAccount.IsBackupAccountMessagesEnabled = IsBackupAccountMessagesEnabled;
            CurrentAccount.SynchronizationInterval = int.TryParse(SynchronizationInterval.Value, out int interval)
                ? interval
                : DefaultSynchronizationInterval;
            CurrentAccount.Type = MailBoxType.Proton;

            return CurrentAccount;
        }

        public static ProtonAddressSettingsModel Create(Account account)
        {
            return new ProtonAddressSettingsModel(account);
        }
    }
}
