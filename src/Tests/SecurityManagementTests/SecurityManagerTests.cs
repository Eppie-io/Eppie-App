using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Tuvi.Core;
using Tuvi.Core.Backup;
using Tuvi.Core.DataStorage;
using Tuvi.Core.Entities;
using Tuvi.Core.Impl;
using Tuvi.Core.Impl.SecurityManagement;
using Tuvi.Core.Mail;
using TuviPgpLib;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tuvi.Core.Mail.Tests")]
namespace SecurityManagementTests
{
    public class SecurityManagerTests : TestWithStorageBase
    {
        [SetUp]
        public void SetupTest()
        {
            DeleteStorage();
        }

        private static ISecurityManager GetSecurityManager(IDataStorage storage)
        {
            var pgpContentMock = new Mock<ITuviPgpContext>();
            var messageProtectorMock = new Mock<IMessageProtector>();
            var backupProtectorMock = new Mock<IBackupProtector>();

            var manager = SecurityManagerCreator.GetSecurityManager(
                storage,
                pgpContentMock.Object,
                messageProtectorMock.Object,
                backupProtectorMock.Object);
            manager.SetKeyDerivationDetails(new ImplementationDetailsProvider("Test seed", "Test.Package", "backup@test"));

            return manager;
        }

        private IDataStorage GetStorage()
        {
            return GetDataStorage();
        }

        [Test]
        public void KeyStorageNotExist()
        {
            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                Assert.IsTrue(manager.IsNeverStartedAsync().Result);
                Assert.IsNotNull(manager.GetSeedValidator());
                Assert.IsNull(manager.GetSeedQuiz());
            }
        }

        [Test]
        public void KeyNotInitialized()
        {
            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                Assert.DoesNotThrowAsync(() => manager.StartAsync(Password));
                Assert.IsFalse(manager.IsNeverStartedAsync().Result);
                Assert.IsFalse(manager.IsSeedPhraseInitializedAsync().Result);
            }
        }

        [Test]
        public void CreateMasterKey()
        {
            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                string[] seed = manager.CreateSeedPhraseAsync().Result;
                Assert.IsNotNull(seed);
                Assert.GreaterOrEqual(seed.Length, manager.GetRequiredSeedPhraseLength());
                foreach (var word in seed)
                {
                    Assert.IsNotEmpty(word);
                }
                Assert.IsTrue(manager.IsNeverStartedAsync().Result);
                Assert.IsNotNull(manager.GetSeedValidator());
                Assert.IsNotNull(manager.GetSeedQuiz());
            }
        }

        [Test]
        public void MasterKeyInitializedOnStart()
        {
            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                manager.CreateSeedPhraseAsync().Wait();
                manager.StartAsync(Password).Wait();
                Assert.IsFalse(manager.IsNeverStartedAsync().Result);
                Assert.IsTrue(manager.IsSeedPhraseInitializedAsync().Result);
            }
        }

        [Test]
        public void MasterKeyInitializedAfterStart()
        {
            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                manager.StartAsync(Password).Wait();

                Assert.DoesNotThrowAsync(() => manager.CreateSeedPhraseAsync());
                Assert.DoesNotThrowAsync(() => manager.InitializeSeedPhraseAsync());
                Assert.IsTrue(manager.IsSeedPhraseInitializedAsync().Result);
            }
        }

        [Test]
        public void RestoreMasterKey()
        {
            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                var testSeed = TestData.GetTestSeed();
                manager.RestoreSeedPhraseAsync(testSeed).Wait();
                manager.StartAsync(Password).Wait();
                Assert.IsFalse(manager.IsNeverStartedAsync().Result);
                Assert.IsTrue(manager.IsSeedPhraseInitializedAsync().Result);
                Assert.IsNotNull(manager.GetSeedValidator());
            }
        }

        [Test]
        public void ResetSecurityManager()
        {
            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                manager.CreateSeedPhraseAsync().Wait();
                manager.StartAsync(Password).Wait();
                Assert.IsTrue(manager.IsSeedPhraseInitializedAsync().Result);

                manager.ResetAsync().Wait();
            }

            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);

                manager.StartAsync(Password).Wait();
                Assert.IsFalse(manager.IsSeedPhraseInitializedAsync().Result);
            }
        }

        [Test]
        public async Task ChangePassword()
        {
            using (var storage = GetStorage())
            {
                await storage.CreateAsync(Password).ConfigureAwait(true);
                ISecurityManager manager = GetSecurityManager(storage);
                await manager.ChangePasswordAsync(Password, NewPassword, default).ConfigureAwait(true);
            }

            using (var storage = GetStorage())
            {
                ISecurityManager manager = GetSecurityManager(storage);
                Assert.ThrowsAsync<DataBasePasswordException>(() => manager.StartAsync(Password));
                Assert.DoesNotThrowAsync(() => manager.StartAsync(NewPassword));
            }
        }
    }
}
