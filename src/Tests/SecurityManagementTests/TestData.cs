using System;
using System.Collections.Generic;
using KeyDerivation;
using KeyDerivation.Keys;
using KeyDerivationLib;
using MimeKit;
using Moq;
using Tuvi.Core.Entities;
using TuviPgpLib;
using TuviPgpLib.Entities;

namespace SecurityManagementTests
{
    public class MockPgpKeyStorage
    {
        private PgpPublicKeyBundle PublicKeyStorage;
        private PgpSecretKeyBundle SecretKeyStorage;

        private readonly Mock<IKeyStorage> MockInstance;

        public MockPgpKeyStorage()
        {
            PublicKeyStorage = null;
            SecretKeyStorage = null;

            MockInstance = new Mock<IKeyStorage>();
            MockInstance.Setup(a => a.GetPgpPublicKeysAsync(default)).ReturnsAsync(PublicKeyStorage);
            MockInstance.Setup(a => a.GetPgpSecretKeysAsync(default)).ReturnsAsync(SecretKeyStorage);
            MockInstance.Setup(a => a.SavePgpPublicKeys(It.IsAny<PgpPublicKeyBundle>()))
                                       .Callback<PgpPublicKeyBundle>((bundle) => PublicKeyStorage = bundle);
            MockInstance.Setup(a => a.SavePgpSecretKeys(It.IsAny<PgpSecretKeyBundle>()))
                                       .Callback<PgpSecretKeyBundle>((bundle) => SecretKeyStorage = bundle);
        }

        public IKeyStorage Get()
        {
            return MockInstance.Object;
        }
    }

    internal class TestKeyDerivationDetailsProvider : IKeyDerivationDetailsProvider
    {
        public string GetSaltPhrase()
        {
            return "Bla-bla";
        }

        public int GetSeedPhraseLength()
        {
            return 12;
        }

        public Dictionary<SpecialPgpKeyType, string> GetSpecialPgpKeyIdentities()
        {
            throw new NotImplementedException();
        }
    }

    internal static class TestData
    {
        public static string[] GetTestSeed()
        {
            return new string[] {
                "ozone",    "drill",    "grab",
                "fiber",    "curtain",  "grace",
                "pudding",  "thank",    "cruise",
                "elder",    "eight",    "picnic"
            };
        }

        public static List<KeyValuePair<string, bool>> GetDictionaryTestData()
        {
            return new List<KeyValuePair<string, bool>>()
            {
                new KeyValuePair<string, bool>("hello", true),
                new KeyValuePair<string, bool>("shine", true),
                new KeyValuePair<string, bool>("abracadabra", false),
                new KeyValuePair<string, bool>("fakdfbmsp", false)
            };
        }

        public static string PublicKeyPath = "./key.pub";
        public static string PrivateKeyPath = "./key.priv";

        public static readonly string[] TestSeedPhrase = {
            "abandon", "abandon", "abandon", "abandon",
            "abandon", "abandon", "abandon", "abandon",
            "abandon", "abandon", "abandon", "abandon"
        };

        public static readonly MasterKey MasterKey = CreateMasterKey(TestSeedPhrase);

        public static MasterKey CreateMasterKey(string[] seedPhrase)
        {
            MasterKeyFactory factory = new MasterKeyFactory(new TestKeyDerivationDetailsProvider());
            factory.RestoreSeedPhrase(seedPhrase);
            return factory.GetMasterKey();
        }

        public class TestAccount
        {
            public string Name;
            public string Address;

            public MailboxAddress GetMailbox()
            {
                return new MailboxAddress(Name, Address);
            }

            public EmailAddress GetEmailAddress()
            {
                return new EmailAddress(Address, Name);
            }

            public string GetPgpIdentity()
            {
                return Address;
            }
        };

        public static TestAccount GetAccount()
        {
            return new TestAccount { Address = "test@user.net", Name = "Test User" };
        }

        public static string TextContent = new string("Hello elliptic curve cryptography!");
    }
}
