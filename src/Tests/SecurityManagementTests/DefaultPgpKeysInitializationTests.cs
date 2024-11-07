using Tuvi.Core.Backup;
using Tuvi.Core.DataStorage;
using Tuvi.Core.Entities;
using Moq;
using NUnit.Framework;
using Tuvi.Core;
using Tuvi.Core.Impl;
using Tuvi.Core.Impl.SecurityManagement;
using Tuvi.Core.Mail;
using TuviPgpLib;
using TuviPgpLibImpl;

namespace SecurityManagementTests
{
    public class DefaultPgpKeysInitializationTests : TestWithStorageBase
    {
        private ITuviPgpContext PgpContext;

        [SetUp]
        public void SetupTest()
        {
            DeleteStorage();
        }

        private ISecurityManager GetSecurityManager(IDataStorage storage)
        {
            PgpContext = new TuviPgpContext(storage);
            var messageProtectorMock = new Mock<IMessageProtector>();
            var backupProtectorMock = new Mock<IBackupProtector>();

            var manager = SecurityManagerCreator.GetSecurityManager(
                storage,
                PgpContext,
                messageProtectorMock.Object,
                backupProtectorMock.Object);

            manager.SetKeyDerivationDetails(new ImplementationDetailsProvider("Test seed", "Test.Package", "backup@test"));

            return manager;
        }

        private IDataStorage GetStorage()
        {
            return base.GetDataStorage();
        }

        [Test]
        public void Explicit()
        {
            using (var storage = GetStorage())
            {
                var account = Account.Default;
                account.Email = TestData.GetAccount().GetEmailAddress();

                ISecurityManager manager = GetSecurityManager(storage);
                manager.CreateSeedPhraseAsync().Wait();
                manager.StartAsync(Password).Wait();

                Assert.IsFalse(PgpContext.IsSecretKeyExist(account.Email.ToUserIdentity()));

                manager.CreateDefaultPgpKeys(account);

                Assert.IsTrue(
                    PgpContext.IsSecretKeyExist(account.Email.ToUserIdentity()),
                    "Pgp key has to be created for account.");
            }
        }

        [Test]
        public void OnMasterKeyInitialization()
        {
            using (var storage = GetStorage())
            {
                var account = Account.Default;
                account.AuthData = new BasicAuthData();

                account.Email = TestData.GetAccount().GetEmailAddress();

                ISecurityManager manager = GetSecurityManager(storage);
                manager.StartAsync(Password).Wait();
                storage.AddAccountAsync(account).Wait();

                Assert.IsFalse(PgpContext.IsSecretKeyExist(account.Email.ToUserIdentity()));

                manager.CreateSeedPhraseAsync().Wait();
                manager.InitializeSeedPhraseAsync().Wait();

                Assert.IsTrue(
                    PgpContext.IsSecretKeyExist(account.Email.ToUserIdentity()),
                    "Pgp key has to be created for all existing accounts after master key initialization.");
            }
        }
    }
}
