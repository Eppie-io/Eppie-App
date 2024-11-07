using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Tuvi.Core.Entities;

namespace Tuvi.Core.DataStorage.Tests
{
    public class ContactsTests : TestWithStorageBase
    {
        [SetUp]
        public async Task SetupAsync()
        {
            DeleteStorage();
            TestData.Setup();

            await CreateDataStorageAsync().ConfigureAwait(true);
        }

        [TearDown]
        public void Teardown()
        {
            DeleteStorage();
        }

        [Test]
        public async Task AddContactToDataStorage()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            Assert.IsFalse(await db.ExistsContactWithEmailAddressAsync(TestData.Contact.Email, default).ConfigureAwait(true));

            await db.AddContactAsync(TestData.Contact, default).ConfigureAwait(true);
            var isAdded = await db.ExistsContactWithEmailAddressAsync(TestData.Contact.Email, default).ConfigureAwait(true);

            Assert.IsTrue(isAdded);
            var contact = await db.GetContactAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            Assert.IsTrue(ContactsAreEqual(contact, TestData.Contact));
            Assert.AreEqual(contact.Email, TestData.Contact.Email);
        }

        [Test]
        public async Task AddContactWithoutEmailToDataStorage()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);

            var contact = new Contact()
            {
                FullName = "Contact Name"
            };

            Assert.CatchAsync<DataBaseException>(async () => await db.AddContactAsync(contact, default).ConfigureAwait(true), "no email");
            contact.Email = new EmailAddress("contact@address1.io");
            Assert.DoesNotThrowAsync(async () => await db.AddContactAsync(contact, default).ConfigureAwait(true));
            Assert.IsTrue(await db.ExistsContactWithEmailAddressAsync(contact.Email, default).ConfigureAwait(true));
        }

        [Test]
        public async Task AddContactWithAvatarToDataStorage()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);

            Assert.IsFalse(await db.ExistsContactWithEmailAddressAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true));

            await db.AddContactAsync(TestData.ContactWithAvatar, default).ConfigureAwait(true);
            var isAdded = await db.ExistsContactWithEmailAddressAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);
            Assert.IsTrue(isAdded);

            var contact = await db.GetContactAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);
            Assert.IsTrue(ContactsAreEqual(contact, TestData.ContactWithAvatar));
        }

        private static bool ContactsAreEqual(Contact c1, Contact c2)
        {
            if (c1.Email == c2.Email &&
                c1.FullName == c2.FullName &&
                ImageInfosAreEqual(c1.AvatarInfo, c2.AvatarInfo))
            {
                return true;
            }

            return false;
        }

        private static bool ImageInfosAreEqual(ImageInfo i1, ImageInfo i2)
        {
            if (i1.Width == i2.Width && i1.Height == i2.Height)
            {
                if ((i1.Bytes == null && i2.Bytes == null) ||
                    i1.Bytes != null && i2.Bytes != null && i1.Bytes.SequenceEqual(i2.Bytes))
                {
                    return true;
                }
            }

            return false;
        }

        [Test]
        public async Task SetContactAvatar()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            Assert.IsFalse(await db.ExistsContactWithEmailAddressAsync(TestData.Contact.Email, default).ConfigureAwait(true));

            await db.AddContactAsync(TestData.Contact, default).ConfigureAwait(true);
            var isAdded = await db.ExistsContactWithEmailAddressAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            Assert.IsTrue(isAdded);

            Assert.CatchAsync<DataBaseException>(async () => await db.AddContactAsync(TestData.ContactWithAvatar, default).ConfigureAwait(true), "Duplicate contacts are prohibited");

            await db.SetContactAvatarAsync(TestData.Contact.Email, TestData.ContactAvatar.Bytes, TestData.ContactAvatar.Width, TestData.ContactAvatar.Height, default).ConfigureAwait(true);
            var contact = await db.GetContactAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);

            Assert.IsTrue(ImageInfosAreEqual(contact.AvatarInfo, TestData.ContactAvatar));
        }

        [Test]
        public async Task RemoveContactAvatar()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);

            await db.AddContactAsync(TestData.ContactWithAvatar, default).ConfigureAwait(true);
            var isAdded = await db.ExistsContactWithEmailAddressAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);
            Assert.IsTrue(isAdded);

            await db.RemoveContactAvatarAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);

            var contact = await db.GetContactAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);

            Assert.IsTrue(contact.AvatarInfo.IsEmpty);
        }

        [Test]
        public async Task UpdateContact()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);

            await db.AddContactAsync(TestData.Contact, default).ConfigureAwait(true);
            var isAdded = await db.ExistsContactWithEmailAddressAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            Assert.IsTrue(isAdded);

            var contact = await db.GetContactAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            contact.FullName = "Updated name";
            await db.UpdateContactAsync(contact, default).ConfigureAwait(true);

            var updatedContact = await db.GetContactAsync(TestData.Contact.Email, default).ConfigureAwait(true);

            Assert.IsTrue(ContactsAreEqual(contact, updatedContact));
        }

        [Test]
        public async Task ChangeContactAvatar()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);

            await db.AddContactAsync(TestData.ContactWithAvatar, default).ConfigureAwait(true);
            var isAdded = await db.ExistsContactWithEmailAddressAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);
            Assert.IsTrue(isAdded);

            var updatedAvatarInfo = new ImageInfo(360, 360, new byte[] { 25, 182, 137, 59, 46, 78, 69, 214 });
            await db.SetContactAvatarAsync(TestData.ContactWithAvatar.Email, updatedAvatarInfo.Bytes, updatedAvatarInfo.Width, updatedAvatarInfo.Height, default).ConfigureAwait(true);
            var contact = await db.GetContactAsync(TestData.ContactWithAvatar.Email, default).ConfigureAwait(true);

            Assert.IsTrue(ImageInfosAreEqual(contact.AvatarInfo, updatedAvatarInfo));
        }

        [Test]
        public async Task RemoveContactByEmail()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddContactAsync(TestData.Contact, default).ConfigureAwait(true);

            await db.RemoveContactAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            Assert.IsFalse(await db.ExistsContactWithEmailAddressAsync(TestData.Contact.Email, default).ConfigureAwait(true));
        }

        [Test]
        public async Task CheckLastMessageData()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddContactAsync(TestData.Contact, default).ConfigureAwait(true);

            var contact = await db.GetContactAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            Assert.IsNull(contact.LastMessageData);
            Assert.IsTrue(contact.LastMessageDataId == 0);

            contact.LastMessageData = new LastMessageData(TestData.Account.Email, TestData.Message.Id, System.DateTimeOffset.Now);

            await db.UpdateContactAsync(contact, default).ConfigureAwait(true);

            contact = await db.GetContactAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            Assert.IsNotNull(contact.LastMessageData);
            Assert.IsTrue(contact.LastMessageData.MessageId == TestData.Message.Id);
            Assert.IsTrue(contact.LastMessageData.Date > System.DateTimeOffset.MinValue);
            Assert.That(contact.LastMessageData.AccountEmail, Is.EqualTo(TestData.Account.Email));
        }

        [Test]
        public async Task TryAddContactAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);

            Assert.IsTrue(await db.TryAddContactAsync(TestData.Contact, default).ConfigureAwait(true));
            Assert.IsFalse(await db.TryAddContactAsync(TestData.Contact, default).ConfigureAwait(true));
        }

        [Test]
        public async Task GetContactsWithLastMessageIdAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder, default).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            var message2 = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message2).ConfigureAwait(true);

            var contacts = await db.GetContactsWithLastMessageIdAsync(accountEmail, message.Id, default).ConfigureAwait(true);
            Assert.IsFalse(contacts.Any());

            var contacts2 = await db.GetContactsWithLastMessageIdAsync(accountEmail, message2.Id, default).ConfigureAwait(true);
            Assert.IsTrue(contacts2.Count() == 1);
        }

        [Test]
        public async Task GetUnknownContactShouldThrowException()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddContactAsync(TestData.Contact, default).ConfigureAwait(true);

            Assert.CatchAsync<DataBaseException>(async () => await db.GetContactAsync(new EmailAddress("unknown@mail.box"), default).ConfigureAwait(true));
        }

        [Test]
        public async Task AddUnreadMessagesShouldShouldIncrementContactUnreadCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddContactAsync(TestData.Contact, default).ConfigureAwait(true);
            var message = TestData.GetNewUnreadMessage();
            message.From.Add(TestData.Contact.Email);

            await db.AddMessageListAsync(accountEmail, TestData.Folder, new List<Message>() { message }, updateUnreadAndTotal: false).ConfigureAwait(true);


            int count = await db.GetContactUnreadMessagesCountAsync(TestData.Contact.Email, default).ConfigureAwait(true);
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddUnreadMessagesShouldShouldIncrementSeveralContactUnreadCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddContactAsync(new Contact("To", TestData.To), default).ConfigureAwait(true);
            await db.AddContactAsync(new Contact("Cc", TestData.Cc), default).ConfigureAwait(true);
            await db.AddContactAsync(new Contact("Bcc", TestData.Bcc), default).ConfigureAwait(true);

            var message = TestData.GetNewUnreadMessage();
            message.From.Clear();
            message.To.Clear();
            message.From.Add(accountEmail);
            message.To.Add(TestData.To);
            message.Cc.Add(TestData.Cc);
            message.Bcc.Add(TestData.Bcc);

            await db.AddMessageListAsync(accountEmail, TestData.Folder, new List<Message>() { message }, updateUnreadAndTotal: false).ConfigureAwait(true);

            var counts = (await db.GetUnreadMessagesCountByContactAsync(default).ConfigureAwait(true)).OrderBy(x => x.Key).ToList();
            Assert.That(counts.Count, Is.EqualTo(3));
            Assert.That(counts[0].Value, Is.EqualTo(1));
            Assert.That(counts[1].Value, Is.EqualTo(1));
            Assert.That(counts[2].Value, Is.EqualTo(1));

            Assert.That(counts[0].Key, Is.EqualTo(TestData.Bcc));
            Assert.That(counts[1].Key, Is.EqualTo(TestData.Cc));
            Assert.That(counts[2].Key, Is.EqualTo(TestData.To));
        }
    }
}
