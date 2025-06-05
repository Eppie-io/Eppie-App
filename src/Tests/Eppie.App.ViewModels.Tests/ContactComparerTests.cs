using NUnit.Framework;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels.Tests
{
    public class ContactComparerTests
    {
        [Test]
        public void CompareContactsWithEmptyName()
        {
            var contact1 = new ContactItem { Email = new EmailAddress("ccc@mail.com") };
            var contact2 = new ContactItem { FullName = "aaa", Email = new EmailAddress("bbb@mail.com") };

            var comparer = new ByNameContactComparer();

            Assert.That(comparer.Compare(contact1, contact2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareContactsWithOnlyOneHasUnread()
        {
            var contact1 = new ContactItem { FullName = "aaa", Email = new EmailAddress("aaa@mail.com"), UnreadMessagesCount = 3 };
            var contact2 = new ContactItem { FullName = "bbb", Email = new EmailAddress("bbb@mail.com") };
            var contact3 = new ContactItem { FullName = "ccc", Email = new EmailAddress("ccc@mail.com"), UnreadMessagesCount = 1 };

            var comparer = new ByUnreadContactComparer();

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Compare(contact1, contact2), Is.LessThan(0));
                Assert.That(comparer.Compare(contact2, contact3), Is.GreaterThan(0));
            });
        }

        [Test]
        public void CompareContactsNoneHasUnread()
        {
            var email1 = new EmailAddress("aaa@mail.com");
            var email2 = new EmailAddress("bbb@mail.com");

            var contact1 = new ContactItem { FullName = "aaa", Email = email1, LastMessageData = new LastMessageData(1, email1, 1, new DateTime(2022, 12, 27)) };
            var contact2 = new ContactItem { FullName = "bbb", Email = email2, LastMessageData = new LastMessageData(2, email2, 5, new DateTime(2022, 12, 28)) };

            var comparer = new ByUnreadContactComparer();

            Assert.That(comparer.Compare(contact1, contact2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareContactsBothHasUnread()
        {
            var email1 = new EmailAddress("aaa@mail.com");
            var email2 = new EmailAddress("bbb@mail.com");

            var contact1 = new ContactItem { FullName = "aaa", Email = email1, UnreadMessagesCount = 4, LastMessageData = new LastMessageData(1, email1, 1, new DateTime(2022, 12, 27)) };
            var contact2 = new ContactItem { FullName = "bbb", Email = email2, UnreadMessagesCount = 2, LastMessageData = new LastMessageData(2, email2, 5, new DateTime(2022, 12, 28)) };

            var comparer = new ByUnreadContactComparer();

            Assert.That(comparer.Compare(contact1, contact2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareContactsBothHasUnreadAndSameDate()
        {
            var email1 = new EmailAddress("aaa@mail.com");
            var email2 = new EmailAddress("bbb@mail.com");

            var contact1 = new ContactItem { FullName = "aaa", Email = email1, UnreadMessagesCount = 4, LastMessageData = new LastMessageData(1, email1, 1, new DateTime(2022, 12, 27)) };
            var contact2 = new ContactItem { FullName = "bbb", Email = email2, UnreadMessagesCount = 2, LastMessageData = new LastMessageData(2, email2, 5, new DateTime(2022, 12, 27)) };

            var comparer = new ByUnreadContactComparer();

            Assert.That(comparer.Compare(contact1, contact2), Is.LessThan(0));
        }
    }
}
