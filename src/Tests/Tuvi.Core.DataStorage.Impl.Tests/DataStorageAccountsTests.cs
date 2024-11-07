using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Tuvi.Core.Entities;

namespace Tuvi.Core.DataStorage.Tests
{
    public class DataStorageAccountsTests : TestWithStorageBase
    {
        [SetUp]
        public async Task SetupAsync()
        {
            DeleteStorage();
            TestData.Setup();

            await CreateDataStorageAsync().ConfigureAwait(true);
        }

        [Test]
        public async Task AddAccountInfoToDataStorage()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true))
                {
                    await db.DeleteAccountByEmailAsync(TestData.Account.Email).ConfigureAwait(true);
                }
                Assert.IsFalse(await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true));

                await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                var isAdded = await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true);
                Assert.IsTrue(isAdded);

                var account = await db.GetAccountAsync(TestData.Account.Email).ConfigureAwait(true);
                Assert.AreEqual(account.AuthData.Type, TestData.Account.AuthData.Type);

                Assert.IsTrue(
                    account.AuthData is BasicAuthData basicData &&
                    TestData.Account.AuthData is BasicAuthData data &&
                    basicData.Password?.Equals(data.Password, StringComparison.Ordinal) == true
                    );
            }
        }

        [Test]
        public async Task ExistsAccountWithEmail()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if ((await db.GetAccountsAsync().ConfigureAwait(true)).Count == 0)
                {
                    await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                }

                var accounts = await db.GetAccountsAsync().ConfigureAwait(true);
                bool result = await db.ExistsAccountWithEmailAddressAsync(accounts[0].Email).ConfigureAwait(true);

                Assert.IsTrue(result);
            }
        }

        [Test]
        public async Task AddAccountWithSameEmailAddress()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (!await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true))
                {
                    await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                }

                Assert.ThrowsAsync<AccountAlreadyExistInDatabaseException>(() => db.AddAccountAsync(TestData.Account));
            }
        }

        [Test]
        public async Task DeleteAccountFromDataStorage()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);

                if ((await db.GetAccountsAsync().ConfigureAwait(true)).Count == 0)
                {
                    await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                }

                var accounts = await db.GetAccountsAsync().ConfigureAwait(true);
                Assert.GreaterOrEqual(accounts.Count, 0);

                var accountToDelete = accounts[0];
                await db.DeleteAccountAsync(accountToDelete).ConfigureAwait(true);

                accounts = await db.GetAccountsAsync().ConfigureAwait(true);
                Assert.IsFalse(accounts.Exists(account => account.Email == accountToDelete.Email));
            }
        }

        [Test]
        public async Task DeleteAccountByEmail()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (!await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true))
                {
                    await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                }

                await db.DeleteAccountByEmailAsync(TestData.Account.Email).ConfigureAwait(true);
                Assert.IsFalse(await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true));
            }
        }

        [Test]
        public async Task UpdateAccountInfoInDataStorage()
        {
            string newName = "Test New Name";

            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (!await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true))
                {
                    await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                }

                var account = TestData.Account;
                account.Email = new EmailAddress(account.Email.Address, newName);
                await db.UpdateAccountAsync(account).ConfigureAwait(true);

                var accounts = await db.GetAccountsAsync().ConfigureAwait(true);
                var updatedAccount = accounts.First(x => x.Email == account.Email);

                Assert.AreEqual(newName, updatedAccount.Email.Name, "Account was not updated properly.");

                await db.DeleteAccountAsync(updatedAccount).ConfigureAwait(true);
            }
        }

        [Test]
        public async Task UpdateAccountAuthData()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (!await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true))
                {
                    await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                }

                var account = TestData.Account;
                var newAuth = new BasicAuthData() { Password = "1111" };
                account.AuthData = newAuth;
                await db.UpdateAccountAsync(account).ConfigureAwait(true);

                var accounts = await db.GetAccountsAsync().ConfigureAwait(true);
                var updatedAccount = accounts.First(x => x.Email == account.Email);

                Assert.AreEqual(newAuth, updatedAccount.AuthData, "Account AuthData wasn't updated properly.");

                await db.DeleteAccountAsync(updatedAccount).ConfigureAwait(true);
            }
        }

        [Test]
        public async Task AddAccountWithInvalidLocalCountFolder()
        {
            using var db = GetDataStorage();
            await db.OpenAsync(Password).ConfigureAwait(true);
            var account = TestData.CreateAccountWithFolder();
            account.FoldersStructure[0].LocalCount = 13; // this field shouldn't be serialized or provided from other sources
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accounts = await db.GetAccountsAsync().ConfigureAwait(true);
            Assert.That(accounts[0].FoldersStructure[0].LocalCount == 0);
        }

        [Test]
        public async Task UpdateAccountFolders()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (!await db.ExistsAccountWithEmailAddressAsync(TestData.AccountWithFolder.Email).ConfigureAwait(true))
                {
                    await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
                }

                await db.AddMessageAsync(TestData.AccountWithFolder.Email, TestData.Message).ConfigureAwait(true);

                var account = TestData.AccountWithFolder;
                var newFolders = new List<Folder>
                {
                    new Folder(TestData.Folder, FolderAttributes.Inbox){ UnreadCount = 102, TotalCount=123},
                    new Folder("Trash", FolderAttributes.Trash),
                };

                account.FoldersStructure = newFolders;
                account.DefaultInboxFolder = account.FoldersStructure[0];

                await db.UpdateAccountAsync(account).ConfigureAwait(true);

                var accounts = await db.GetAccountsAsync().ConfigureAwait(true);
                var updatedAccount = accounts.First(x => x.Email.Address == account.Email.Address);

                Assert.AreEqual(newFolders, updatedAccount.FoldersStructure, "Account folders weren't updated properly.");
                Assert.AreEqual(newFolders[0], updatedAccount.DefaultInboxFolder, "Account default folder wasn't updated properly.");
                Assert.That(updatedAccount.FoldersStructure[0].UnreadCount, Is.EqualTo(102));
                Assert.That(updatedAccount.FoldersStructure[0].TotalCount, Is.EqualTo(123));

                var message = await db.GetMessageAsync(TestData.AccountWithFolder.Email, TestData.Message.Folder.FullName, TestData.Message.Id).ConfigureAwait(true);

                Assert.AreEqual(message.Folder, TestData.Message.Folder);

                // check message in the removed folder
                {
                    newFolders.RemoveAt(0);
                    account.FoldersStructure = newFolders;
                    account.DefaultInboxFolder = account.FoldersStructure[0];
                    await db.UpdateAccountAsync(account).ConfigureAwait(true);
                    bool exists = await db.IsMessageExistAsync(TestData.AccountWithFolder.Email, TestData.Message.Folder.FullName, TestData.Message.Id).ConfigureAwait(true);
                    Assert.IsFalse(exists);
                }

                await db.DeleteAccountAsync(updatedAccount).ConfigureAwait(true);
            }
        }

        [Test]
        public async Task UpdateIfAccountNotExist()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true))
                {
                    await db.DeleteAccountByEmailAsync(TestData.Account.Email).ConfigureAwait(true);
                }
                Assert.IsFalse(await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true));

                await db.UpdateAccountAsync(TestData.Account).ConfigureAwait(true);
                Assert.IsFalse(await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true));
            }
        }

        [Test]
        public async Task AddSeveralAccounts()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);
                if (await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true))
                {
                    await db.DeleteAccountByEmailAsync(TestData.Account.Email).ConfigureAwait(true);
                }

                await db.AddAccountAsync(TestData.Account).ConfigureAwait(true);
                Assert.IsTrue(await db.ExistsAccountWithEmailAddressAsync(TestData.Account.Email).ConfigureAwait(true));

                await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
                Assert.IsTrue(await db.ExistsAccountWithEmailAddressAsync(TestData.AccountWithFolder.Email).ConfigureAwait(true));
            }
        }
    }
}
