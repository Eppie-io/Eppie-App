using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Tuvi.Core.Entities.Test
{
    public class MessageTests
    {
        static char[] Separators = { ',', ';' };
        static IEnumerable<EmailAddress> ParseEmailAddresses(string addresses)
        {
            return addresses.Split(Separators, System.StringSplitOptions.RemoveEmptyEntries).Select(x => new EmailAddress(x.Trim()));
        }

        [TestCase("", "", "", "", "", "", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "", "", "", "", "", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "", "", "", "", "tes@rre.r", "tet@err.f", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "rwr@tets.e", "ewe@tet.f", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "", "to1@te.c", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "replyto1@te.c", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "", ExpectedResult = false)]

        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "", "From1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "To1@te.c", "From1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "to1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "cc1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "bcc1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "replyto1@te.c", "to1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "To1@te.c", "Replyto1@te.c", ExpectedResult = true)]
#pragma warning disable CA1062
        public bool IsFromCorrespondenceWithContactTest(string from,
                                                        string replyTo,
                                                        string to,
                                                        string cc,
                                                        string bcc,
                                                        string account,
                                                        string contact)
        {
            var message = new Message();
            message.From.AddRange(ParseEmailAddresses(from));
            message.ReplyTo.AddRange(ParseEmailAddresses(replyTo));
            message.To.AddRange(ParseEmailAddresses(to));
            message.Cc.AddRange(ParseEmailAddresses(cc));
            message.Bcc.AddRange(ParseEmailAddresses(bcc));

            return message.IsFromCorrespondenceWithContact(new EmailAddress(account), new EmailAddress(contact));
        }
#pragma warning restore CA1062

        [TestCase("", "", "", "", "", "", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "", "", "", "", "", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "", "", "", "", "tes@rre.r", "tet@err.f", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "rwr@tets.e", "ewe@tet.f", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "replyto1@te.c", "", ExpectedResult = false)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "", ExpectedResult = false)]

        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "", "to1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "", "From1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "To1@te.c", "From1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "to1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "cc1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "from1@te.c", "bcc1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "replyto1@te.c", "to1@te.c", ExpectedResult = true)]
        [TestCase("From1@te.c", "ReplyTo1@te.c", "To1@te.c", "Cc1@te.c", "Bcc1@te.c", "To1@te.c", "Replyto1@te.c", ExpectedResult = true)]
#pragma warning disable CA1062
        public bool IsFromCorrespondenceWithContactTest2(string from,
                                                string replyTo,
                                                string to,
                                                string cc,
                                                string bcc,
                                                string account,
                                                string contact)
        {
            var message = new Message();
            message.From.AddRange(ParseEmailAddresses(from));
            message.ReplyTo.AddRange(ParseEmailAddresses(replyTo));
            message.To.AddRange(ParseEmailAddresses(to));
            message.Cc.AddRange(ParseEmailAddresses(cc));
            message.Bcc.AddRange(ParseEmailAddresses(bcc));

            return message.IsFromCorrespondenceWithContact(new EmailAddress(contact));
        }
#pragma warning restore CA1062


        [Test]
        public void GetMessageContactShouldFilterAccountAddress()
        {
            var message = new Message();
            message.From.Add(new EmailAddress("account@mail.box"));
            message.ReplyTo.Add(new EmailAddress("reply@mail.box"));
            message.To.Add(new EmailAddress("To@mail.box"));
            message.To.Add(new EmailAddress("account@mail.box"));
            message.Cc.Add(new EmailAddress("cc@mail.box"));
            message.Bcc.Add(new EmailAddress("bcc@mail.box"));


            var contactEmails = message.GetContactEmails(new EmailAddress("account@mail.box")).OrderBy(x => x.Address).ToList();
            Assert.That(contactEmails.Count, Is.EqualTo(3));
            Assert.That(contactEmails[0], Is.EqualTo(new EmailAddress("bcc@mail.box")));
            Assert.That(contactEmails[1], Is.EqualTo(new EmailAddress("cc@mail.box")));
            Assert.That(contactEmails[2], Is.EqualTo(new EmailAddress("to@mail.box")));
        }
    }
}
