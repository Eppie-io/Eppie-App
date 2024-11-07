using NUnit.Framework;
using Tuvi.Core.Entities;

namespace Tuvi.Core.DataStorage.Tests
{
    // These tests are synchronous.
    // Storage acts like a state machine.
    // Use separate storage files for each test if parallel execution is needed.
    public class PasswordProtectedStorageTests : TestWithStorageBase
    {
        [SetUp]
        public void SetupTest()
        {
            DeleteStorage();
            TestData.Setup();
        }

        [Test]
        public void StorageNotExist()
        {
            using (var storage = GetDataStorage())
            {
                Assert.ThrowsAsync<DataBaseNotCreatedException>(() => storage.OpenAsync(Password));
                Assert.DoesNotThrowAsync(() => storage.CreateAsync(Password));
            }
        }

        [Test]
        public void StorageExist()
        {
            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.CreateAsync(Password));
                Assert.ThrowsAsync<DataBaseAlreadyExistsException>(() => storage.CreateAsync(Password));
                Assert.DoesNotThrowAsync(() => storage.OpenAsync(Password));
            }
        }

        [Test]
        public void OpenAndSetPassword()
        {
            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.CreateAsync(Password));
                Assert.DoesNotThrowAsync(() => storage.OpenAsync(Password));
            }
        }

        [Test]
        public void OpenWithCorrectPassword()
        {
            OpenAndSetPassword();

            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.OpenAsync(Password));
            }
        }

        [Test]
        public void OpenWithIncorrectPassword()
        {
            OpenAndSetPassword();

            using (var storage = GetDataStorage())
            {
                Assert.ThrowsAsync<DataBasePasswordException>(() => storage.OpenAsync(IncorrectPassword));
            }
        }

        [Test]
        public void ChangePassword()
        {
            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.CreateAsync(Password));
            }


            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.ChangePasswordAsync(Password, NewPassword));
            }

            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.OpenAsync(NewPassword));
            }

            using (var storage = GetDataStorage())
            {
                Assert.ThrowsAsync<DataBasePasswordException>(() => storage.OpenAsync(Password));
                Assert.DoesNotThrowAsync(() => storage.OpenAsync(NewPassword));
            }
        }

        [Test]
        public void ResetStorage()
        {
            using (var storage = GetDataStorage())
            {
                storage.CreateAsync(Password).Wait();
                storage.ResetAsync().Wait();
                Assert.IsFalse(DatabaseFileExists());
            }
        }

        [Test]
        public void MultiplePasswordChange()
        {
            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.CreateAsync(Password));
            }

            using (var storage = GetDataStorage())
            {
                Assert.DoesNotThrowAsync(() => storage.ChangePasswordAsync(Password, "newPass1"));
            }

            using (var storage = GetDataStorage())
            {
                Assert.ThrowsAsync<DataBasePasswordException>(() => storage.OpenAsync(Password));
                Assert.DoesNotThrowAsync(() => storage.OpenAsync("newPass1"));
                Assert.DoesNotThrowAsync(() => storage.ChangePasswordAsync("newPass1", "newPass2"));
            }

            using (var storage = GetDataStorage())
            {
                Assert.ThrowsAsync<DataBasePasswordException>(() => storage.OpenAsync(Password));
                Assert.ThrowsAsync<DataBasePasswordException>(() => storage.OpenAsync("newPass1"));
                Assert.DoesNotThrowAsync(() => storage.OpenAsync("newPass2"));
                Assert.DoesNotThrowAsync(() => storage.ChangePasswordAsync("newPass2", "newPass3"));
            }
        }
    }
}
