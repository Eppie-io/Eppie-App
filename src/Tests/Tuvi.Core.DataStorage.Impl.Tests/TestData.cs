using System;
using System.Collections.Generic;
using System.Globalization;
using KeyDerivation;
using Tuvi.Core.Entities;

namespace Tuvi.Core.DataStorage.Tests
{
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

    static class TestData
    {
        public const string Folder = "INBOX";
        public const string Subject = "This is test TuviMail message";
        public const string PlainText = "Text of the test message";
        public const string HtmlText = "<p><strong><em>Hello world!</em></strong></p><ol><li>text 1</li><li>text 2</li></ol>";

        static uint TestID;
        public static EmailAddress Email;
        public static Account Account;
        public static Account AccountWithFolder;
        public static Message Message;
        public static Contact Contact;
        public static ImageInfo ContactAvatar;
        public static Contact ContactWithAvatar;
        public static Attachment Attachment;
        public static SignatureInfo SignatureInfo;
        public static ProtectionInfo Protection;

        public static EmailAddress From = new EmailAddress("from@mail.box");
        public static EmailAddress ReplyTo = new EmailAddress("replyto@mail.box");
        public static EmailAddress To = new EmailAddress("to@mail.box");
        public static EmailAddress Cc = new EmailAddress("Cc@mail.box");
        public static EmailAddress Bcc = new EmailAddress("Bcc@mail.box");

        public static void Setup()
        {
            Email = new EmailAddress("tuvimail@email.test");
            Account = CreateAccount();
            Message = CreateMessage();
            TestID = 0;
            AccountWithFolder = CreateAccountWithFolder();
            Contact = CreateContact();
            ContactAvatar = CreateContactAvatarInfo();
            ContactWithAvatar = CreateContactWithAvatar();

            Attachment = new Attachment { FileName = "text_file.txt", Data = System.Text.Encoding.ASCII.GetBytes("text of file") };
            SignatureInfo = new SignatureInfo { DigestAlgorithm = "Sha1", IsVerified = true, SignerEmail = "tuvi.mail@email.test", SignerFingerprint = "C491157EDEB0CB0FADFF8780BD3BB71C09809E56" };
            Protection = new ProtectionInfo { Type = MessageProtectionType.SignatureAndEncryption, SignaturesInfo = { SignatureInfo } };
        }

        public static Account CreateAccount()
        {
            var account = new Account();
            account.Email = new EmailAddress("tuvi.mail@email.test", "Tuvi Mail Test");

            account.IncomingServerAddress = "imap.email.test";
            account.IncomingServerPort = 993;
            account.IncomingMailProtocol = MailProtocol.IMAP;

            account.OutgoingServerAddress = "smtp.email.test";
            account.OutgoingServerPort = 465;
            account.OutgoingMailProtocol = MailProtocol.SMTP;

            account.AuthData = new BasicAuthData() { Password = "1234567890" };

            return account;
        }

        public static Account CreateAccountWithFolder()
        {
            return CreateAccountWithFolder(new EmailAddress("tuvi2.mail@email.test", "Tuvi Mail Test 2"));
        }

        public static Account CreateAccountWithFolder(EmailAddress emailAddress)
        {
            var account = new Account();
            account.Email = emailAddress;

            account.IncomingServerAddress = "imap.email.test";
            account.IncomingServerPort = 993;
            account.IncomingMailProtocol = MailProtocol.IMAP;

            account.OutgoingServerAddress = "smtp.email.test";
            account.OutgoingServerPort = 465;
            account.OutgoingMailProtocol = MailProtocol.SMTP;

            account.AuthData = new BasicAuthData() { Password = "1234567890" };

            account.FoldersStructure.Add(new Folder(Folder, FolderAttributes.Inbox));
            account.DefaultInboxFolder = account.FoldersStructure[0];

            return account;
        }

        static Message CreateMessage()
        {
            var message = new Message();
            message.From.Add(new EmailAddress("from@mail.com", "TM"));
            message.To.Add(new EmailAddress("to@mail.com", "TM"));
            message.Subject = "Subject";
            message.TextBody = "Hello world message body";
            message.Id = 7;
            message.Folder = new Folder(Folder, FolderAttributes.Inbox) { AccountEmail = new EmailAddress("tuvi2.mail@email.test") };

            return message;
        }

        public static Message GetNewMessage()
        {
            return CreateNewMessage(Folder, TestID++, DateTimeOffset.Now);
        }

        public static Message CreateNewMessage(string folder, uint id, DateTimeOffset dateTime)
        {
            var message = new Message();
            message.From.Add(new EmailAddress("from@mail.com", "TM"));
            message.To.Add(new EmailAddress("to@mail.com", "TM"));
            message.Subject = "Messsage number " + id.ToString(CultureInfo.InvariantCulture);
            message.TextBody = "Hello world message body";
            message.Id = id;
            message.Folder = new Folder(folder, FolderAttributes.None);
            message.Date = dateTime;

            return message;
        }


        public static Message GetNewReadMessage()
        {
            var message = GetNewMessage();
            message.IsMarkedAsRead = true;
            return message;
        }

        public static Message GetNewUnreadMessage()
        {
            var message = GetNewMessage();
            message.IsMarkedAsRead = false;
            return message;
        }

        static Contact CreateContact()
        {
            var contact = new Contact();
            contact.Email = new EmailAddress("contact@mail.com");
            contact.FullName = "Contact Name";
            return contact;
        }

        static Contact CreateContactWithAvatar()
        {
            var contact = new Contact();
            contact.Email = new EmailAddress("contact@mail.com");
            contact.FullName = "Contact Name";
            contact.AvatarInfo = ContactAvatar;
            return contact;
        }

        static ImageInfo CreateContactAvatarInfo()
        {
            return new ImageInfo(240, 240, new byte[] { 69, 137, 84, 12, 0, 53, 14, 91 });
        }

        public static Contact GetNewContact()
        {
            var contact = new Contact();
            contact.Email = new EmailAddress($"contact{TestID}@mail.com");
            contact.FullName = $"Contact Name #{TestID}";
            contact.AvatarInfo = ContactAvatar;

            TestID++;

            return contact;
        }
    }
}
