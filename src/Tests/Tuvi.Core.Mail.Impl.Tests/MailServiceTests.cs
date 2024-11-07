using Tuvi.Core.Entities;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

using Tuvi.Core.Mail.Impl.Protocols;
using Tuvi.Core.Mail.Impl.Protocols.IMAP;

namespace Tuvi.Core.Mail.Impl.Test
{
    public class MailServiceTests
    {
        private const string ServerAddress = "mail.test.com.test";
        private const int ServerPort = 12345;

        [SetUp]
        public void Setup()
        {

        }

        private static Mock<MailKit.Net.Imap.ImapClient> CreateClientMockForConnectTests()
        {
            var imapClientMock = new Mock<MailKit.Net.Imap.ImapClient>();
            imapClientMock.Setup(a => a.ConnectAsync(ServerAddress,
                                                     ServerPort,
                                                     MailKit.Security.SecureSocketOptions.Auto,
                                                     default));

            imapClientMock.Setup(a => a.ConnectAsync(It.Is<string>(s => s != ServerAddress),
                                                     ServerPort,
                                                     MailKit.Security.SecureSocketOptions.Auto,
                                                     default)).Throws(new System.Net.Sockets.SocketException());

            imapClientMock.Setup(a => a.ConnectAsync(ServerAddress,
                                                     It.Is<int>(i => i != ServerPort),
                                                     MailKit.Security.SecureSocketOptions.Auto,
                                                     default)).Throws(new System.Net.Sockets.SocketException());

            imapClientMock.Setup(a => a.ConnectAsync(It.Is<string>(s => s != ServerAddress),
                                                     It.Is<int>(i => i != ServerPort),
                                                     MailKit.Security.SecureSocketOptions.Auto,
                                                     default)).Throws(new System.Net.Sockets.SocketException());

            return imapClientMock;
        }

        [Test]
        public async Task ConnectWithCorrectParametersTest()
        {
            var imapClientMock = CreateClientMockForConnectTests();
            imapClientMock.Setup(a => a.IsConnected).Returns(true);
            using var service = new IMAPMailService(imapClientMock.Object, ServerAddress, ServerPort);
            await service.ConnectAsync(default).ConfigureAwait(false);
            Assert.IsTrue(service.IsConnected);
        }

        [Test]
        public void ConnectWithIncorrectServerAddressTest()
        {
            var imapClientMock = CreateClientMockForConnectTests();
            using var service = new IMAPMailService(imapClientMock.Object, "mail.test.com.mail.test", ServerPort);
            Assert.ThrowsAsync<ConnectionException>(async () => await service.ConnectAsync(default).ConfigureAwait(false));
        }

        [Test]
        public void ConnectWithIncorrectPortNumberTest()
        {
            var imapClientMock = CreateClientMockForConnectTests();
            using var service = new IMAPMailService(imapClientMock.Object, ServerAddress, 54321);
            Assert.ThrowsAsync<ConnectionException>(async () => await service.ConnectAsync(default).ConfigureAwait(false));
        }

        [Test]
        public void ConnectWithIncorrectAddressNameAndPortNumberTest()
        {
            var imapClientMock = CreateClientMockForConnectTests();
            using var service = new IMAPMailService(imapClientMock.Object, "mail.test.com.mail.test", 54321);
            Assert.ThrowsAsync<ConnectionException>(async () => await service.ConnectAsync(default).ConfigureAwait(false));
        }


    }
}
