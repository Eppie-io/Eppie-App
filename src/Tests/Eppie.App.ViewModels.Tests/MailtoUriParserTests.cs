using NUnit.Framework;
using System;
using Tuvi.App.ViewModels.Helpers;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public class MailtoUriParserTests
    {
        [Test]
        public void ParseSimpleMailtoWithRecipientParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:user@example.com");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.EqualTo("user@example.com"));
            Assert.That(result.Cc, Is.Empty);
            Assert.That(result.Bcc, Is.Empty);
            Assert.That(result.Subject, Is.Empty);
            Assert.That(result.Body, Is.Empty);
        }

        [Test]
        public void ParseMailtoWithSubjectAndBodyParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:user@example.com?subject=Hello&body=Test%20message");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.EqualTo("user@example.com"));
            Assert.That(result.Subject, Is.EqualTo("Hello"));
            Assert.That(result.Body, Is.EqualTo("Test message"));
        }

        [Test]
        public void ParseMailtoWithCcAndBccParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:user@example.com?cc=cc@example.com&bcc=bcc@example.com");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.EqualTo("user@example.com"));
            Assert.That(result.Cc, Is.EqualTo("cc@example.com"));
            Assert.That(result.Bcc, Is.EqualTo("bcc@example.com"));
        }

        [Test]
        public void ParseMailtoWithMultipleRecipientsParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:user1@example.com?to=user2@example.com&cc=cc1@example.com,cc2@example.com");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.EqualTo("user1@example.com, user2@example.com"));
            Assert.That(result.Cc, Is.EqualTo("cc1@example.com,cc2@example.com"));
        }

        [Test]
        public void ParseMailtoWithEncodedCharactersDecodesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:user@example.com?subject=Test%20Subject%20%26%20More&body=Line1%0ALine2");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.Subject, Is.EqualTo("Test Subject & More"));
            Assert.That(result.Body, Is.EqualTo("Line1\nLine2"));
        }

        [Test]
        public void ParseMailtoWithoutRecipientParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:?subject=No%20Recipient&body=Body%20text");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.Empty);
            Assert.That(result.Subject, Is.EqualTo("No Recipient"));
            Assert.That(result.Body, Is.EqualTo("Body text"));
        }

        [Test]
        public void ParseMailtoWithAllFieldsParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:primary@example.com?to=secondary@example.com&cc=cc@example.com&bcc=bcc@example.com&subject=Complete%20Test&body=Full%20message%20body");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.EqualTo("primary@example.com, secondary@example.com"));
            Assert.That(result.Cc, Is.EqualTo("cc@example.com"));
            Assert.That(result.Bcc, Is.EqualTo("bcc@example.com"));
            Assert.That(result.Subject, Is.EqualTo("Complete Test"));
            Assert.That(result.Body, Is.EqualTo("Full message body"));
        }

        [Test]
        public void ParseWithNullUriThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => MailtoUriParser.Parse((Uri)null!));
        }



        [Test]
        public void ParseWithInvalidSchemeThrowsArgumentException()
        {
            // Arrange
            var httpUri = new Uri("http://example.com");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => MailtoUriParser.Parse(httpUri));
        }

        [Test]
        public void ParseCaseInsensitiveSchemeParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("MAILTO:user@example.com?SUBJECT=Test");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.EqualTo("user@example.com"));
            Assert.That(result.Subject, Is.EqualTo("Test"));
        }

        [Test]
        public void ParseCaseInsensitiveParametersParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:user@example.com?SUBJECT=Test&CC=cc@example.com&BCC=bcc@example.com&BODY=Message");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.Subject, Is.EqualTo("Test"));
            Assert.That(result.Cc, Is.EqualTo("cc@example.com"));
            Assert.That(result.Bcc, Is.EqualTo("bcc@example.com"));
            Assert.That(result.Body, Is.EqualTo("Message"));
        }

        [Test]
        public void ParseEmptyMailtoParsesCorrectly()
        {
            // Arrange
            var mailtoUri = new Uri("mailto:");

            // Act
            var result = MailtoUriParser.Parse(mailtoUri);

            // Assert
            Assert.That(result.To, Is.Empty);
            Assert.That(result.Cc, Is.Empty);
            Assert.That(result.Bcc, Is.Empty);
            Assert.That(result.Subject, Is.Empty);
            Assert.That(result.Body, Is.Empty);
        }
    }
}
