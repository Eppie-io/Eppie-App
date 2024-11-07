using System;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using TuviPgpLibImpl;
using Tuvi.Base32EConverterLib;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using MimeKit;
using System.Collections.Generic;
using Tuvi.Core.Utils;

namespace SecurityManagementTests
{
    public class PublicKeyConvertingTests
    {
        private readonly DateTime KeyCreationTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        [Test]
        public void ECPubKeyConverting()
        {            
            for (int i = 0; i < 50; i++)
            {
                var keyPair = EccPgpContext.DeriveKeyPair(TestData.MasterKey, i);
                var publicKey = keyPair.Public as ECPublicKeyParameters;

                byte[] publicKeyAsBytes = publicKey.Q.GetEncoded(true);
                string emailName = Base32EConverter.ConvertBytesToEmailName(publicKeyAsBytes);
                var reconvertedPublicKeyAsBytes = Base32EConverter.ConvertStringToByteArray(emailName);

                Assert.AreEqual(publicKeyAsBytes, reconvertedPublicKeyAsBytes);
            }
        }

        [Test]
        public void ECPubKeyParametersConverting()
        {
            for (int i = 0; i < 50; i++)
            {
                var keyPair = EccPgpContext.DeriveKeyPair(TestData.MasterKey, i);
                var publicKey = keyPair.Public as ECPublicKeyParameters;

                string emailName = PublicKeyConverter.ConvertPublicKeyToEmailName(publicKey);
                var reconvertedPublicKey = PublicKeyConverter.ConvertEmailNameToPublicKey(emailName);

                Assert.AreEqual(publicKey, reconvertedPublicKey);
            }
        }

        [Test]
        public void PublicKeyImportTest()
        {
            string emailName = "agwaxxb4zchc8digxdxryn5fzs5s2r32swwajipn4bewski276k2c";
            var reconvertedPublicKey = PublicKeyConverter.ConvertEmailNameToPublicKey(emailName);

            PgpPublicKey publicKey = new PgpPublicKey(Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ECDH, reconvertedPublicKey, DateTime.Now);

            using TuviPgpContext ctx = InitializeTuviPgpContext();
            ctx.Import(new PgpPublicKeyRing(publicKey.GetEncoded()));

            Assert.AreEqual(1, ctx.PublicKeyRingBundle.Count, "Public key was not imported");
        }

        [Test]
        public void PublicKeyEncryptDecryptTest()
        {
            using Stream inputData = new MemoryStream();
            using Stream encryptedData = new MemoryStream();
            using var messageBody = new TextPart() { Text = TestData.TextContent };
            messageBody.WriteTo(inputData);
            inputData.Position = 0;

            string emailName = "ae5ky7ah5gepibreyyts88vcdenmhk786cmec8xyjburepk5bxufc";

            ECPublicKeyParameters reconvertedPublicKey = PublicKeyConverter.ConvertEmailNameToPublicKey(emailName);

            PgpPublicKey publicKey = new PgpPublicKey(Org.BouncyCastle.Bcpg.PublicKeyAlgorithmTag.ECDH, reconvertedPublicKey, KeyCreationTime);

            using EccPgpContext ctx = InitializeTuviPgpContext();
            var encryptedMime = ctx.Encrypt(new List<PgpPublicKey> { publicKey }, inputData);

            ctx.DeriveKeyPair(TestData.MasterKey, TestData.GetAccount().GetPgpIdentity());

            encryptedMime.WriteTo(encryptedData);
            encryptedData.Position = 0;

            var mime = ctx.Decrypt(encryptedData);
            var decryptedBody = mime as TextPart;

            Assert.IsTrue(
                TestData.TextContent.SequenceEqual(decryptedBody?.Text ?? string.Empty),
                "Decrypted content is corrupted");
        }

        [Test]
        public void EmailNameConvertingNullEmailNameThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PublicKeyConverter.ConvertEmailNameToPublicKey(null), "Email name can not be a null.");
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(15)]
        public void ECPubKeyConvertingTooLongPubKeyThrowArgumentException(int childKeyNum)
        {
            var keyPair = EccPgpContext.DeriveKeyPair(TestData.MasterKey, childKeyNum);
            var publicKey = keyPair.Public as ECPublicKeyParameters;

            byte[] publicKeyAsBytes = publicKey.Q.GetEncoded(false);

            Assert.Throws<ArgumentException>(() => Base32EConverter.ConvertBytesToEmailName(publicKeyAsBytes));
        }

        [TestCase("")]
        [TestCase("auubcdefg")]
        [TestCase("zxy8pt5roasd3mefe")]
        [TestCase("abracadabraabracadabraabracadabraabracadabraabracada")]
        [TestCase("abracadabraabracadabraabracadabraabracadabraabracadabr")]
        [TestCase("abracadabraabracadabraabracadabraabracadabraabracadabraabracadabraabracadabra")]
        public void EmailNameConvertingWrongEmailNameLengthThrowArgumentException(string emailName)
        {
            Assert.Throws<ArgumentException>(() => PublicKeyConverter.ConvertEmailNameToPublicKey(emailName), "Incorrect length of email name.");
        }

        [TestCase("abracadabraabracadabraabracadabraabracadabraabracadab")]
        [TestCase("adracadabraabracadabraabracadabraabracadabraabracadab")]
        [TestCase("apracadabraabracadabraabracadabraabracadabraabracadab")]
        [TestCase("atracadabraabracadabraabracadabraabracadabraabracadab")]
        [TestCase("a2racadabraabracadabraabracadabraabracadabraabracadab")]
        [TestCase("b2racadabraabracadabraabracadabraabracadabraabracadab")]
        [TestCase("r2racadabraabracadabraabracadabraabracadabraabracadab")]
        [TestCase("xgwaxxb4zchc8digxdxryn5fzs5s2r32swwajipn4bewski276k2c")]
        [TestCase("6gwaxxb4zchc8digxdxryn5fzs5s2r32swwajipn4bewski276k2c")]
        public void EmailNameConvertingWrongFormatThrowArgumentException(string emailName)
        {
            Assert.Throws<FormatException>(() => PublicKeyConverter.ConvertEmailNameToPublicKey(emailName), "Invalid point format. Encoded public key should start with 0x02 or 0x03.");
        }

        private static TuviPgpContext InitializeTuviPgpContext()
        {
            var keyStorage = new MockPgpKeyStorage().Get();
            var context = new TuviPgpContext(keyStorage);
            context.LoadContextAsync().Wait();
            return context;
        }
    }
}
