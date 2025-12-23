using NUnit.Framework;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels.Tests
{
    public class AddressSettingsCancelTests
    {
        [Test]
        public void ProtonSettingsCancelDoesNotMutateAccount()
        {
            // Constants for original values
            const string ProtonEmail = "user@proton.me";
            const string ProtonName = "User";
            const string OriginalFooter = "Old footer";
            const int SyncInterval = 20;
            const string OutgoingServer = "smtp.old.com";
            const int OutgoingPort = 465;
            const string IncomingServer = "imap.old.com";
            const int IncomingPort = 993;
            const string OldPwd = "oldpwd";
            const string IncLogin = "in_old";
            const string IncPwd = "in_pwd";
            const string OutLogin = "out_old";
            const string OutPwd = "out_pwd";

            // Arrange original values
            var account = new Account
            {
                Email = new EmailAddress(ProtonEmail, ProtonName),
                Type = MailBoxType.Proton,
                IsBackupAccountSettingsEnabled = true,
                IsBackupAccountMessagesEnabled = true,
                IsMessageFooterEnabled = false,
                MessageFooter = OriginalFooter,
                SynchronizationInterval = SyncInterval,
                OutgoingServerAddress = OutgoingServer,
                OutgoingServerPort = OutgoingPort,
                IncomingServerAddress = IncomingServer,
                IncomingServerPort = IncomingPort,
                IncomingMailProtocol = MailProtocol.IMAP,
                AuthData = new BasicAuthData
                {
                    Password = OldPwd,
                    IncomingLogin = IncLogin,
                    IncomingPassword = IncPwd,
                    OutgoingLogin = OutLogin,
                    OutgoingPassword = OutPwd
                }
            };

            using (var vm = new ProtonAddressSettingsPageViewModel())
            {
                // Initialize VM with existing account
                vm.OnNavigatedTo(account);

                // Act: change values in the editable model (simulate user edits)
                vm.AddressSettingsModel.Email.Value = "new@proton.me";
                vm.AddressSettingsModel.SenderName.Value = "New User";
                vm.AddressSettingsModel.IsBackupAccountSettingsEnabled = false;
                vm.AddressSettingsModel.IsBackupAccountMessagesEnabled = false;
                vm.AddressSettingsModel.IsMessageFooterEnabled = true;
                vm.AddressSettingsModel.MessageFooter = "New footer";
                vm.AddressSettingsModel.SynchronizationInterval.Value = "15";

                var pmodel = (ProtonAddressSettingsModel)vm.AddressSettingsModel;
                
                // Additional changes to verify no mutation
                var isHybrid = vm.IsHybridAddress;

                // User cancels changes
                vm.CancelSettingsCommand.Execute(null);
            }

            // Assert: original account is not mutated
            Assert.That(account.Email.Address, Is.EqualTo(ProtonEmail));
            Assert.That(account.Email.Name, Is.EqualTo(ProtonName));
            Assert.That(account.Type, Is.EqualTo(MailBoxType.Proton));

            Assert.That(account.IsBackupAccountSettingsEnabled, Is.True);
            Assert.That(account.IsBackupAccountMessagesEnabled, Is.True);
            Assert.That(account.IsMessageFooterEnabled, Is.False);
            Assert.That(account.MessageFooter, Is.EqualTo(OriginalFooter));
            Assert.That(account.SynchronizationInterval, Is.EqualTo(SyncInterval));

            Assert.That(account.OutgoingServerAddress, Is.EqualTo(OutgoingServer));
            Assert.That(account.OutgoingServerPort, Is.EqualTo(OutgoingPort));
            Assert.That(account.IncomingServerAddress, Is.EqualTo(IncomingServer));
            Assert.That(account.IncomingServerPort, Is.EqualTo(IncomingPort));
            Assert.That(account.IncomingMailProtocol, Is.EqualTo(MailProtocol.IMAP));

            Assert.That(account.AuthData, Is.TypeOf<BasicAuthData>());
            var basic = (BasicAuthData)account.AuthData;
            Assert.That(basic.Password, Is.EqualTo(OldPwd));
            Assert.That(basic.IncomingLogin, Is.EqualTo(IncLogin));
            Assert.That(basic.IncomingPassword, Is.EqualTo(IncPwd));
            Assert.That(basic.OutgoingLogin, Is.EqualTo(OutLogin));
            Assert.That(basic.OutgoingPassword, Is.EqualTo(OutPwd));
        }

        [Test]
        public void EmailSettingsCancelDoesNotMutateAccount()
        {
            // Constants for original values
            const string EmailAddr = "user@mail.com";
            const string EmailName = "User";
            const string OriginalFooter = "Footer A";
            const int SyncInterval = 10;
            const string OutgoingServer = "smtp.old.com";
            const int OutgoingPort = 465;
            const string IncomingServer = "imap.old.com";
            const int IncomingPort = 993;
            const string OldPwd = "oldpass";
            const string IncLogin = "in_login";
            const string IncPwd = "in_pass";
            const string OutLogin = "out_login";
            const string OutPwd = "out_pass";

            // Arrange original values
            var account = new Account
            {
                Email = new EmailAddress(EmailAddr, EmailName),
                Type = MailBoxType.Email,
                IsBackupAccountSettingsEnabled = true,
                IsBackupAccountMessagesEnabled = true,
                IsMessageFooterEnabled = true,
                MessageFooter = OriginalFooter,
                SynchronizationInterval = SyncInterval,
                OutgoingServerAddress = OutgoingServer,
                OutgoingServerPort = OutgoingPort,
                IncomingServerAddress = IncomingServer,
                IncomingServerPort = IncomingPort,
                IncomingMailProtocol = MailProtocol.IMAP,
                AuthData = new BasicAuthData
                {
                    Password = OldPwd,
                    IncomingLogin = IncLogin,
                    IncomingPassword = IncPwd,
                    OutgoingLogin = OutLogin,
                    OutgoingPassword = OutPwd
                }
            };

            using (var vm = new EmailAddressSettingsPageViewModel())
            {
                // Initialize VM with existing account
                vm.OnNavigatedTo(account);

                // Act: change values in the editable model (simulate user edits)
                Assert.That(vm.AddressSettingsModel, Is.TypeOf<BasicEmailAddressSettingsModel>());
                var model = (BasicEmailAddressSettingsModel)vm.AddressSettingsModel;

                model.Email.Value = "new@mail.com";
                model.SenderName.Value = "New User";
                model.IsBackupAccountSettingsEnabled = false;
                model.IsBackupAccountMessagesEnabled = false;
                model.IsMessageFooterEnabled = false;
                model.MessageFooter = "Footer B";
                model.SynchronizationInterval.Value = "15";

                model.OutgoingServerAddress.Value = "smtp.new.com";
                model.OutgoingServerPort = 587;
                model.IncomingServerAddress.Value = "pop.new.com";
                model.IncomingServerPort = 995;
                model.IncomingMailProtocol = MailProtocol.POP3;

                model.Password.Value = "newpass";
                model.UseSeparateIncomingCredentials = true;
                model.UseSeparateOutgoingCredentials = true;
                model.IncomingLogin.Value = "in_login2";
                model.IncomingPassword.Value = "in_pass2";
                model.OutgoingLogin.Value = "out_login2";
                model.OutgoingPassword.Value = "out_pass2";

                // Additional changes to verify no mutation
                var isHybrid = vm.IsHybridAddress;

                // User cancels changes
                vm.CancelSettingsCommand.Execute(null);
            }

            // Assert: original account is not mutated
            Assert.That(account.Email.Address, Is.EqualTo(EmailAddr));
            Assert.That(account.Email.Name, Is.EqualTo(EmailName));
            Assert.That(account.Type, Is.EqualTo(MailBoxType.Email));

            Assert.That(account.IsBackupAccountSettingsEnabled, Is.True);
            Assert.That(account.IsBackupAccountMessagesEnabled, Is.True);
            Assert.That(account.IsMessageFooterEnabled, Is.True);
            Assert.That(account.MessageFooter, Is.EqualTo(OriginalFooter));
            Assert.That(account.SynchronizationInterval, Is.EqualTo(SyncInterval));

            Assert.That(account.OutgoingServerAddress, Is.EqualTo(OutgoingServer));
            Assert.That(account.OutgoingServerPort, Is.EqualTo(OutgoingPort));
            Assert.That(account.IncomingServerAddress, Is.EqualTo(IncomingServer));
            Assert.That(account.IncomingServerPort, Is.EqualTo(IncomingPort));
            Assert.That(account.IncomingMailProtocol, Is.EqualTo(MailProtocol.IMAP));

            Assert.That(account.AuthData, Is.TypeOf<BasicAuthData>());
            var basic = (BasicAuthData)account.AuthData;
            Assert.That(basic.Password, Is.EqualTo(OldPwd));
            Assert.That(basic.IncomingLogin, Is.EqualTo(IncLogin));
            Assert.That(basic.IncomingPassword, Is.EqualTo(IncPwd));
            Assert.That(basic.OutgoingLogin, Is.EqualTo(OutLogin));
            Assert.That(basic.OutgoingPassword, Is.EqualTo(OutPwd));
        }

        [Test]
        public void DecentralizedSettingsCancelDoesNotMutateAccount()
        {
            // Constants for original values
            const string OriginalName = "OrigName";
            const string OriginalFooter = "Footer DEC";
            const int SyncInterval = 30;

            // Arrange decentralized Eppie address
            var decEmail = EmailAddress.CreateDecentralizedAddress(NetworkType.Eppie, "owner");
            var originalAddress = decEmail.Address;

            var account = new Account
            {
                Email = new EmailAddress(originalAddress, OriginalName),
                Type = MailBoxType.Dec,
                IsBackupAccountSettingsEnabled = true,
                IsBackupAccountMessagesEnabled = true,
                IsMessageFooterEnabled = false,
                MessageFooter = OriginalFooter,
                SynchronizationInterval = SyncInterval
            };

            using (var vm = new DecentralizedAddressSettingsPageViewModel())
            {
                // Initialize VM with existing decentralized account
                vm.OnNavigatedTo(account);
                var model = vm.AddressSettingsModel;

                // Act: change values in the editable model (simulate user edits)
                model.Email.Value = EmailAddress.CreateDecentralizedAddress(NetworkType.Eppie, "newowner").DisplayAddress;
                model.SenderName.Value = "New Sender";
                model.ClaimedName = "claimed";
                model.SecretKeyWIF = "wif-key";

                model.IsBackupAccountSettingsEnabled = false;
                model.IsBackupAccountMessagesEnabled = false;
                model.IsMessageFooterEnabled = true;
                model.MessageFooter = "Footer NEW";
                model.SynchronizationInterval.Value = "15";

                // Additional changes to verify no mutation
                var isHybrid = vm.IsHybridAddress;

                // User cancels changes
                vm.CancelSettingsCommand.Execute(null);
            }

            // Assert: original account is not mutated
            Assert.That(account.Email.Address, Is.EqualTo(originalAddress));
            Assert.That(account.Email.Name, Is.EqualTo(OriginalName));
            Assert.That(account.Type, Is.EqualTo(MailBoxType.Dec));

            Assert.That(account.IsBackupAccountSettingsEnabled, Is.True);
            Assert.That(account.IsBackupAccountMessagesEnabled, Is.True);
            Assert.That(account.IsMessageFooterEnabled, Is.False);
            Assert.That(account.MessageFooter, Is.EqualTo(OriginalFooter));
            Assert.That(account.SynchronizationInterval, Is.EqualTo(SyncInterval));
        }
    }
}
