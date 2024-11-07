using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using KeyDerivation.Keys;
using KeyDerivationLib;
using NUnit.Framework;
using TuviPgpLib.Entities;

namespace Tuvi.Core.DataStorage.Tests
{
    // These tests are synchronous.
    // Storage acts like a state machine.
    // Use separate storage files for each test if parallel execution is needed.
    public class KeyStorageTests : TestWithStorageBase
    {
        [SetUp]
        public void SetupTest()
        {
            DeleteStorage();
        }

        private static MasterKey GenerateRandomMasterKey()
        {
            MasterKeyFactory factory = new MasterKeyFactory(new TestKeyDerivationDetailsProvider());
            factory.GenerateSeedPhrase();
            return factory.GetMasterKey();
        }

        private static PgpPublicKeyBundle GenerateRandomPgpPublicKeyBundleData()
        {
            var data = new byte[100];

            RandomNumberGenerator.Fill(data);

            return new PgpPublicKeyBundle { Data = data };
        }

        private static PgpSecretKeyBundle GenerateRandomPgpSecretKeyBundleData()
        {
            var rand = new Random();
            var data = new byte[100];

            RandomNumberGenerator.Fill(data);

            return new PgpSecretKeyBundle { Data = data };
        }

        [Test]
        public void MasterKeyNotInitialized()
        {
            using (var storage = GetDataStorage())
            {
                storage.CreateAsync(Password).Wait();
                Assert.IsFalse(storage.IsMasterKeyExistAsync().Result);
            }
        }

        [Test]
        public void MasterKeyInitialized()
        {
            using (var storage = GetDataStorage())
            {
                storage.CreateAsync(Password).Wait();
                storage.InitializeMasterKeyAsync(GenerateRandomMasterKey()).Wait();
                Assert.IsTrue(storage.IsMasterKeyExistAsync().Result);
            }
        }

        [Test]
        public void GetMasterKey()
        {
            using (var storage = GetDataStorage())
            {
                storage.CreateAsync(Password).Wait();
                var masterKey = GenerateRandomMasterKey();
                storage.InitializeMasterKeyAsync(masterKey).Wait();
                Assert.AreEqual(masterKey, storage.GetMasterKeyAsync().Result);
            }
        }

        [Test]
        public void PgpPublicKeysStorage()
        {
            using (var storage = GetDataStorage())
            {
                storage.CreateAsync(Password).Wait();
                storage.InitializeMasterKeyAsync(GenerateRandomMasterKey()).Wait();
                var keys = storage.GetPgpPublicKeysAsync().Result;
                Assert.AreEqual(null, keys, "Key hasn't to exist.");

                var randomKeyData = GenerateRandomPgpPublicKeyBundleData();
                storage.SavePgpPublicKeys(randomKeyData);

                var extracted = storage.GetPgpPublicKeysAsync().Result;
                Assert.AreEqual(randomKeyData, extracted, "Key data wasn't stored properly.");

                randomKeyData = GenerateRandomPgpPublicKeyBundleData();
                storage.SavePgpPublicKeys(randomKeyData);

                extracted = storage.GetPgpPublicKeysAsync().Result;
                Assert.AreEqual(randomKeyData, extracted, "Key data wasn't updated properly.");
            }
        }

        [Test]
        public void PgpSecretKeysStorage()
        {
            using (var storage = GetDataStorage())
            {
                storage.CreateAsync(Password).Wait();
                storage.InitializeMasterKeyAsync(GenerateRandomMasterKey()).Wait();
                Assert.AreEqual(null, storage.GetPgpSecretKeysAsync().Result, "Key hasn't to exist.");

                var randomKeyData = GenerateRandomPgpSecretKeyBundleData();
                storage.SavePgpSecretKeys(randomKeyData);

                var extracted = storage.GetPgpSecretKeysAsync().Result;
                Assert.AreEqual(randomKeyData, extracted, "Key data wasn't stored properly.");

                randomKeyData = GenerateRandomPgpSecretKeyBundleData();
                storage.SavePgpSecretKeys(randomKeyData);

                extracted = storage.GetPgpSecretKeysAsync().Result;
                Assert.AreEqual(randomKeyData, extracted, "Key data wasn't updated properly.");
            }
        }

        [Test]
        public async Task OpenCreateTest()
        {
            DeleteStorage();
            Assert.IsFalse(DatabaseFileExists());
            var storage = await CreateDataStorageAsync().ConfigureAwait(true);
            Assert.DoesNotThrow(() => storage.Dispose());
            Assert.IsTrue(DatabaseFileExists());
            Assert.DoesNotThrowAsync(async () => storage = await OpenDataStorageAsync().ConfigureAwait(true));
            Assert.DoesNotThrow(() => storage.Dispose());
            Assert.IsTrue(DatabaseFileExists());
            Assert.DoesNotThrowAsync(async () => storage = await OpenDataStorageAsync().ConfigureAwait(true));
            Assert.DoesNotThrowAsync(() => storage.ResetAsync());
            Assert.IsFalse(DatabaseFileExists());
            Assert.DoesNotThrow(() => storage.Dispose());
            Assert.DoesNotThrowAsync(async () => storage = await CreateDataStorageAsync().ConfigureAwait(true));
            Assert.IsTrue(DatabaseFileExists());
            Assert.DoesNotThrowAsync(() => storage.ResetAsync());
            Assert.IsFalse(DatabaseFileExists());
            Assert.DoesNotThrow(() => storage.Dispose());
            Assert.IsFalse(DatabaseFileExists());
        }

        [Test]
        public void OpenCreateMultithreadTest()
        {
            for (int j = 0; j < 100; ++j)
            {
                Assert.IsFalse(DatabaseFileExists());
                var tasks = new List<Task>();
                var storage = GetDataStorage();
                Assert.IsFalse(DatabaseFileExists());
                for (int i = 0; i < 100; ++i)
                {
                    tasks.Add(Task.Run(async () =>
                    {
#pragma warning disable CA5394 // Do not use insecure randomness
                        await Task.Delay(Random.Shared.Next(200)).ConfigureAwait(false);
#pragma warning restore CA5394 // Do not use insecure randomness
                        await storage.OpenAsync(Password).ConfigureAwait(false);
                        Assert.IsTrue(DatabaseFileExists());
                    }));
                }
                storage.ResetAsync(); // should wait all connections
                Assert.IsFalse(DatabaseFileExists());
            }

        }
    }
}
