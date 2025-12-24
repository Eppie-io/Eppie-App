using NUnit.Framework;
using System.Globalization;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels.Tests
{
    public class AddressSettingsApplyTests
    {
        // Common new values used across tests
        private const string NewEmail = "new@mail.com";
        private const string NewUserName = "New User";
        private const bool NewBackupSettingsEnabled = true;
        private const bool NewBackupMessagesEnabled = true;
        private const string NewFooter = "new footer";
        private const bool NewFooterEnabled = true;
        private const int NewSyncInterval = 15;
        private const string NewOutgoingServer = "smtp.new.com";
        private const int NewOutgoingPort = 587;
        private const string NewIncomingServer = "pop.new.com";
        private const int NewIncomingPort = 995;
        private const MailProtocol NewIncomingProtocol = MailProtocol.POP3;
        private const string NewPassword = "newpass";
        private const string NewIncomingLogin = "in_login";
        private const string NewIncomingPassword = "in_pass";
        private const string NewOutgoingLogin = "out_login";
        private const string NewOutgoingPassword = "out_pass";

        private const string OauthOutgoingServer = "smtp.gmail.com";
        private const string OauthIncomingServer = "imap.gmail.com";
        private const string OauthRefreshToken = "refTok";
        private const string OauthAssistantId = "Gmail";

        private const string DecClaimedName = "coolname";

        [Test]
        public void BasicEmailModelAppliesCurrentAccountFields()
        {
            // Arrange: existing account with some initial values
            var account = new Account
            {
                Id = 42,
                GroupId = 7,
                Email = new EmailAddress("old@mail.com", "Old User"),
                IsBackupAccountSettingsEnabled = false,
                IsBackupAccountMessagesEnabled = false,
                MessageFooter = "old footer",
                IsMessageFooterEnabled = false,
                SynchronizationInterval = 5,
                OutgoingServerAddress = "smtp.old.com",
                OutgoingServerPort = 465,
                IncomingServerAddress = "imap.old.com",
                IncomingServerPort = 993,
                IncomingMailProtocol = MailProtocol.IMAP,
                AuthData = new BasicAuthData { Password = "oldpass" }
            };

            using (var vm = new EmailAddressSettingsPageViewModel())
            {
                // Initialize VM with existing account
                vm.OnNavigatedTo(account);

                // Work with the underlying Basic model
                Assert.That(vm.AddressSettingsModel, Is.TypeOf<BasicEmailAddressSettingsModel>());
                var model = (BasicEmailAddressSettingsModel)vm.AddressSettingsModel;

                model.OutgoingServerPort = NewOutgoingPort;
                model.IncomingServerPort = NewIncomingPort;
                model.IncomingMailProtocol = NewIncomingProtocol;
                model.UseSeparateIncomingCredentials = true;
                model.UseSeparateOutgoingCredentials = true;

                // Update editable fields in the model
                model.Email.Value = NewEmail;
                model.SenderName.Value = NewUserName;
                model.IsBackupAccountSettingsEnabled = NewBackupSettingsEnabled;
                model.IsBackupAccountMessagesEnabled = NewBackupMessagesEnabled;
                model.MessageFooter = NewFooter;
                model.IsMessageFooterEnabled = NewFooterEnabled;
                model.SynchronizationInterval.Value = NewSyncInterval.ToString(CultureInfo.InvariantCulture);
                model.OutgoingServerAddress.Value = NewOutgoingServer;
                model.IncomingServerAddress.Value = NewIncomingServer;
                model.Password.Value = NewPassword;
                model.IncomingLogin.Value = NewIncomingLogin;
                model.IncomingPassword.Value = NewIncomingPassword;
                model.OutgoingLogin.Value = NewOutgoingLogin;
                model.OutgoingPassword.Value = NewOutgoingPassword;

                // Act: use page command
                vm.ApplySettingsCommand.Execute(null);
                // Wait briefly for async command body to run
                SpinWait.SpinUntil(() => !vm.IsWaitingResponse, TimeSpan.FromMilliseconds(200));
            }

            // Assert: all fields applied on original account
            Assert.That(account.Email.Address, Is.EqualTo(NewEmail));
            Assert.That(account.Email.Name, Is.EqualTo(NewUserName));
            Assert.That(account.IsBackupAccountSettingsEnabled, Is.EqualTo(NewBackupSettingsEnabled));
            Assert.That(account.IsBackupAccountMessagesEnabled, Is.EqualTo(NewBackupMessagesEnabled));
            Assert.That(account.MessageFooter, Is.EqualTo(NewFooter));
            Assert.That(account.IsMessageFooterEnabled, Is.EqualTo(NewFooterEnabled));
            Assert.That(account.SynchronizationInterval, Is.EqualTo(NewSyncInterval));
            Assert.That(account.OutgoingServerAddress, Is.EqualTo(NewOutgoingServer));
            Assert.That(account.OutgoingServerPort, Is.EqualTo(NewOutgoingPort));
            Assert.That(account.IncomingServerAddress, Is.EqualTo(NewIncomingServer));
            Assert.That(account.IncomingServerPort, Is.EqualTo(NewIncomingPort));
            Assert.That(account.IncomingMailProtocol, Is.EqualTo(NewIncomingProtocol));

            Assert.That(account.AuthData, Is.TypeOf<BasicAuthData>());
            var basic = (BasicAuthData)account.AuthData;
            Assert.That(basic.Password, Is.EqualTo(NewPassword));
            Assert.That(basic.IncomingLogin, Is.EqualTo(NewIncomingLogin));
            Assert.That(basic.IncomingPassword, Is.EqualTo(NewIncomingPassword));
            Assert.That(basic.OutgoingLogin, Is.EqualTo(NewOutgoingLogin));
            Assert.That(basic.OutgoingPassword, Is.EqualTo(NewOutgoingPassword));

            // Immutables preserved
            Assert.That(account.Id, Is.EqualTo(42));
            Assert.That(account.GroupId, Is.EqualTo(7));
        }

        [Test]
        public void OAuth2EmailModelAppliesOAuth2AuthData()
        {
            // Arrange
            var account = new Account
            {
                Email = new EmailAddress("user@gmail.com", "User"),
                AuthData = new OAuth2Data()
            };

            using (var vm = new EmailAddressSettingsPageViewModel())
            {
                vm.OnNavigatedTo(account);
                Assert.That(vm.AddressSettingsModel, Is.TypeOf<OAuth2EmailAddressSettingsModel>());
                var model = (OAuth2EmailAddressSettingsModel)vm.AddressSettingsModel;

                model.OutgoingServerAddress.Value = OauthOutgoingServer;
                model.IncomingServerAddress.Value = OauthIncomingServer;

                model.RefreshToken = OauthRefreshToken;
                model.AuthAssistantId = OauthAssistantId;

                // Act
                vm.ApplySettingsCommand.Execute(null);
                SpinWait.SpinUntil(() => !vm.IsWaitingResponse, TimeSpan.FromMilliseconds(200));
            }

            // Assert
            Assert.That(account.AuthData, Is.TypeOf<OAuth2Data>());
            var oauth = (OAuth2Data)account.AuthData;
            Assert.That(oauth.RefreshToken, Is.EqualTo(OauthRefreshToken));
            Assert.That(oauth.AuthAssistantId, Is.EqualTo(OauthAssistantId));
        }

        [Test]
        public void ProtonModelAppliesType()
        {
            // Arrange
            var account = new Account
            {
                Email = new EmailAddress("user@proton.me", "User")
            };

            using (var vm = new ProtonAddressSettingsPageViewModel())
            {
                vm.OnNavigatedTo(account);
                var model = vm.AddressSettingsModel;

                // Act
                vm.ApplySettingsCommand.Execute(null);
                SpinWait.SpinUntil(() => !vm.IsWaitingResponse, TimeSpan.FromMilliseconds(200));
            }

            // Assert
            Assert.That(account.Type, Is.EqualTo(MailBoxType.Proton));
        }

        [Test]
        public void DecentralizedModelAppliesClaimedNameAndType()
        {
            // Arrange: Eppie decentralized address
            var decEmail = EmailAddress.CreateDecentralizedAddress(NetworkType.Eppie, "owner");
            var account = new Account
            {
                Email = decEmail,
                Type = MailBoxType.Email // will be overridden
            };

            using (var vm = new DecentralizedAddressSettingsPageViewModel())
            {
                vm.OnNavigatedTo(account);
                var model = vm.AddressSettingsModel;

                model.SenderName.Value = "ignored"; // will be overridden by ClaimedName when visible
                model.ClaimedName = DecClaimedName;

                // Act
                vm.ApplySettingsCommand.Execute(null);
                SpinWait.SpinUntil(() => !vm.IsWaitingResponse, TimeSpan.FromMilliseconds(200));
            }

            // Assert
            Assert.That(account.Type, Is.EqualTo(MailBoxType.Dec));
            Assert.That(account.Email.Name, Is.EqualTo(DecClaimedName));
        }
    }
}
