using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Tuvi.Core.Entities;

namespace Tuvi.Core.DataStorage.Tests
{
    public class MessagesTests : TestWithStorageBase
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
        public async Task DeleteMessage()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.Message;

            if (!await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true))
            {
                await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);
            }

            Assert.IsTrue(await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true));

            await db.DeleteMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);
            Assert.IsFalse(await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true));
        }

        [Test]
        public async Task DeleteMessages()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(accountEmail, TestData.Folder, 0).ConfigureAwait(true);

            Assert.AreEqual(addMessageList.Count, receivedMessageList.Count);

            await db.DeleteMessagesAsync(accountEmail, TestData.Folder, addMessageList.ConvertAll(x => x.Id)).ConfigureAwait(true);

            foreach (var message in addMessageList)
            {
                Assert.IsFalse(await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true));
            }
        }

        [Test]
        public async Task AddMessage()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.Message;
            if (await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true))
            {
                await db.DeleteMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);
            }

            Assert.IsFalse(await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true));

            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            Assert.IsTrue(await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true));
            Assert.That(message.Folder, Is.Not.Null);
            Assert.That(message.FolderId, Is.Not.EqualTo(0));
            Assert.That(message.Folder.AccountEmail, Is.Not.Null);
            Assert.That(message.Folder.AccountId, Is.Not.EqualTo(0));
        }

        [Test]
        public async Task AddMessageWithEqualID()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.Message;
            if (!await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true))
            {
                await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);
            }

            Assert.ThrowsAsync<MessageAlreadyExistInDatabaseException>(() => db.AddMessageAsync(accountEmail, message));
        }

        [Test]
        public async Task ChangeMessage()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.Message;
            if (!await db.IsMessageExistAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true))
            {
                await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);
            }

            var newTextBody = "new text body";

            message.TextBody = newTextBody;

            await db.UpdateMessageAsync(accountEmail, message).ConfigureAwait(true);

            var newMessage = await db.GetMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);

            Assert.IsTrue(message.TextBody == newMessage.TextBody);
        }

        [Test]
        public async Task GetLatestMessageAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            var latestMessage = await db.GetLatestMessageAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.IsTrue(latestMessage.Id == message.Id);
        }

        [Test]
        public async Task GetContactLastMessageAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.GetNewMessage();
            var email = new EmailAddress("user@mail.com", "UM");
            message.From.Add(email);
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            var lastMessage = await db.GetContactLastMessageAsync(accountEmail, TestData.Folder, email, default).ConfigureAwait(true);

            Assert.IsTrue(lastMessage.Id == message.Id);
            Assert.Contains(email, lastMessage.From);
        }

        [Test]
        public async Task GetMessagesSeveralFoldersAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            const int messageCount = 10;
            var account = TestData.CreateAccount();
            account.FoldersStructure.Add(new Folder(TestData.Folder, FolderAttributes.Inbox));
            account.FoldersStructure.Add(new Folder("TestFolder", FolderAttributes.Sent));
            account.DefaultInboxFolder = account.FoldersStructure[0];
            await db.AddAccountAsync(account).ConfigureAwait(true);

            var contactEmail = new EmailAddress("contact@mail.com", "CM");
            DateTimeOffset start = DateTimeOffset.Now;
            foreach (var folder in new[] { "TestFolder", TestData.Folder })
            {
                var addMessageList = new List<Message>();
                for (int i = 0; i < messageCount; ++i)
                {
                    var message = TestData.CreateNewMessage(folder, (uint)i + 1, start.AddSeconds(1));
                    message.From.Add(contactEmail);
                    addMessageList.Add(message);
                }
                await db.AddMessageListAsync(account.Email, folder, addMessageList).ConfigureAwait(true);
            }

            Func<Folder, Task> verifyFolder = async (Folder folder) =>
            {
                var earlierMessages = await db.GetEarlierMessagesAsync(folder, 5, null, default).ConfigureAwait(true);
                Assert.That(earlierMessages.Count, Is.EqualTo(5));
                Assert.That(earlierMessages[0].Id, Is.EqualTo(10));
                Assert.That(earlierMessages[1].Id, Is.EqualTo(9));
                Assert.That(earlierMessages[2].Id, Is.EqualTo(8));
                Assert.That(earlierMessages[3].Id, Is.EqualTo(7));
                Assert.That(earlierMessages[4].Id, Is.EqualTo(6));
                Assert.That(earlierMessages.All(x => x.FolderId == folder.Id));

                var earlierMessages2 = await db.GetEarlierMessagesAsync(folder, 5, earlierMessages[4], default).ConfigureAwait(true);
                Assert.That(earlierMessages2.Count, Is.EqualTo(5));
                Assert.That(earlierMessages2[0].Id, Is.EqualTo(5));
                Assert.That(earlierMessages2[1].Id, Is.EqualTo(4));
                Assert.That(earlierMessages2[2].Id, Is.EqualTo(3));
                Assert.That(earlierMessages2[3].Id, Is.EqualTo(2));
                Assert.That(earlierMessages2[4].Id, Is.EqualTo(1));
                Assert.That(earlierMessages2.All(x => x.FolderId == folder.Id));

                var earlierMessages3 = await db.GetEarlierMessagesAsync(folder, 5, earlierMessages2[1], default).ConfigureAwait(true);
                Assert.That(earlierMessages3.Count, Is.EqualTo(3));
                Assert.That(earlierMessages3[0].Id, Is.EqualTo(3));
                Assert.That(earlierMessages3[1].Id, Is.EqualTo(2));
                Assert.That(earlierMessages3[2].Id, Is.EqualTo(1));
                Assert.That(earlierMessages3.All(x => x.FolderId == folder.Id));
            };

            await verifyFolder(account.FoldersStructure[0]).ConfigureAwait(true);
            await verifyFolder(account.FoldersStructure[1]).ConfigureAwait(true);

            var earlierMessages = await db.GetEarlierContactMessagesAsync(contactEmail, 10, null, default).ConfigureAwait(true);
            Assert.That(earlierMessages.Count, Is.EqualTo(10));
            earlierMessages = await db.GetEarlierContactMessagesAsync(contactEmail, 20, null, default).ConfigureAwait(true);
            Assert.That(earlierMessages.Count, Is.EqualTo(20));
            earlierMessages = await db.GetEarlierContactMessagesAsync(contactEmail, 22, null, default).ConfigureAwait(true);
            Assert.That(earlierMessages.Count, Is.EqualTo(20));

            // Messages have the same date, so they are odered 
            // * by folder
            // * by id

            for (int i = 0, id = 10; i < 10; ++i, --id)
            {
                Assert.That(earlierMessages[i].Id, Is.EqualTo(id));
                Assert.That(earlierMessages[i + 10].Id, Is.EqualTo(id));
                Assert.That(earlierMessages[i].FolderId, Is.EqualTo(account.FoldersStructure[0].Id));
                Assert.That(earlierMessages[i + 10].FolderId, Is.EqualTo(account.FoldersStructure[1].Id));
            }

            var earlierMessages2 = await db.GetEarlierContactMessagesAsync(contactEmail, 10, earlierMessages[1], default).ConfigureAwait(true);
            Assert.That(earlierMessages2.Count, Is.EqualTo(10));

            for (int i = 0, id = 8; i < 8; ++i, --id)
            {
                Assert.That(earlierMessages2[i].Id, Is.EqualTo(id));
                Assert.That(earlierMessages2[i].FolderId, Is.EqualTo(account.FoldersStructure[0].Id));
            }

            Assert.That(earlierMessages2[8].Id, Is.EqualTo(10));
            Assert.That(earlierMessages2[8].FolderId, Is.EqualTo(account.FoldersStructure[1].Id));

            Assert.That(earlierMessages2[9].Id, Is.EqualTo(9));
            Assert.That(earlierMessages2[9].FolderId, Is.EqualTo(account.FoldersStructure[1].Id));


            earlierMessages2 = await db.GetEarlierContactMessagesAsync(contactEmail, 10, earlierMessages[0], default).ConfigureAwait(true);
            Assert.That(earlierMessages2.Count, Is.EqualTo(10));

            for (int i = 0, id = 9; i < 9; ++i, --id)
            {
                Assert.That(earlierMessages2[i].Id, Is.EqualTo(id));
                Assert.That(earlierMessages2[i].FolderId, Is.EqualTo(account.FoldersStructure[0].Id));
            }

            Assert.That(earlierMessages2[9].Id, Is.EqualTo(10));
            Assert.That(earlierMessages2[9].FolderId, Is.EqualTo(account.FoldersStructure[1].Id));

            earlierMessages2 = await db.GetEarlierContactMessagesAsync(contactEmail, 10, earlierMessages[17], default).ConfigureAwait(true);
            Assert.That(earlierMessages2.Count, Is.EqualTo(2));
            Assert.That(earlierMessages2[0].Id, Is.EqualTo(2));
            Assert.That(earlierMessages2[1].Id, Is.EqualTo(1));
            Assert.That(earlierMessages2[0].FolderId, Is.EqualTo(account.FoldersStructure[1].Id));
            Assert.That(earlierMessages2[1].FolderId, Is.EqualTo(account.FoldersStructure[1].Id));

            earlierMessages2 = await db.GetEarlierContactMessagesAsync(contactEmail, 10, earlierMessages[18], default).ConfigureAwait(true);
            Assert.That(earlierMessages2.Count, Is.EqualTo(1));
            Assert.That(earlierMessages2[0].Id, Is.EqualTo(1));
            Assert.That(earlierMessages2[0].FolderId, Is.EqualTo(account.FoldersStructure[1].Id));

            earlierMessages2 = await db.GetEarlierContactMessagesAsync(contactEmail, 10, earlierMessages[19], default).ConfigureAwait(true);
            Assert.That(earlierMessages2.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetEarlierContactMessagesAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            var contactEmail = new EmailAddress("contact@mail.com", "CM");
            for (int i = 0; i < messageCount; ++i)
            {
                var message = TestData.GetNewMessage();
                message.From.Add(contactEmail);
                addMessageList.Add(message);
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            var earlierMessages = await db.GetEarlierContactMessagesAsync(contactEmail, 10, null, default).ConfigureAwait(true); ;

            Assert.AreEqual(addMessageList.Count, earlierMessages.Count);

            int lastMessageIndex = 5;
            var earlierMessages2 = await db.GetEarlierContactMessagesAsync(contactEmail, 10, addMessageList[lastMessageIndex], default).ConfigureAwait(true);

            Assert.AreEqual(lastMessageIndex, earlierMessages2.Count);
        }

        [Test]
        public async Task GetAllMessages()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(accountEmail, TestData.Folder, 0).ConfigureAwait(true);

            Assert.AreEqual(addMessageList.Count, receivedMessageList.Count);
        }

        [Test]
        public async Task GetFirstNMessages()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            uint countToReceive = 5;
            var receivedMessageList = await db.GetMessageListAsync(accountEmail, TestData.Folder, countToReceive).ConfigureAwait(true);

            Assert.AreEqual(countToReceive, receivedMessageList.Count);
        }


        [Test]
        public async Task GetFirstMoreThenExistMessages()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            uint countToReceive = 20;
            var receivedMessageList = await db.GetMessageListAsync(accountEmail, TestData.Folder, countToReceive).ConfigureAwait(true);

            Assert.AreEqual(messageCount, receivedMessageList.Count);
        }

        [Test]
        public async Task GetPartMessages()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            uint countToReceive = 3;
            int startPosition = 5;
            var receivedMessageList = await db.GetMessageListAsync(accountEmail, TestData.Folder, addMessageList[startPosition].Id, countToReceive).ConfigureAwait(true);
            Assert.AreEqual(countToReceive, receivedMessageList.Count);

            bool isCorrect = true;
            var temp = addMessageList.Where(x => x.Id < addMessageList[startPosition].Id).OrderByDescending(x => x.Id).Take((int)countToReceive).ToList();
            for (int i = 0; i < countToReceive; ++i)
            {
                if (temp[i].Id != receivedMessageList[i].Id)
                {
                    isCorrect = false;
                }
            }

            Assert.IsTrue(isCorrect);
        }

        [Test]
        public async Task GetPartMoreThenExistMessages()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            uint countToReceive = 15;
            int startPosition = 5;
            var receivedMessageList = await db.GetMessageListAsync(accountEmail, TestData.Folder, addMessageList[startPosition].Id, countToReceive).ConfigureAwait(true);

            Assert.AreEqual(messageCount - startPosition, receivedMessageList.Count);

            bool isCorrect = true;
            var temp = addMessageList.Where(x => x.Id < addMessageList[startPosition].Id).OrderByDescending(x => x.Id).Take((int)countToReceive).ToList();
            for (int i = 0; i < receivedMessageList.Count; ++i)
            {
                if (temp[i].Id != receivedMessageList[i].Id)
                {
                    isCorrect = false;
                }
            }

            Assert.IsTrue(isCorrect);
        }

        [Test]
        public async Task GetMessageListByRange1To9()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await AddTestMessageListAsync(db).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(TestData.AccountWithFolder.Email, TestData.Folder, (1, 9)).ConfigureAwait(true);

            Assert.That(receivedMessageList.Count, Is.EqualTo(5));
            Assert.That(receivedMessageList[0].Id, Is.EqualTo(8));
            Assert.That(receivedMessageList[1].Id, Is.EqualTo(7));
            Assert.That(receivedMessageList[2].Id, Is.EqualTo(5));
            Assert.That(receivedMessageList[3].Id, Is.EqualTo(3));
            Assert.That(receivedMessageList[4].Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetMessageListByRange9To1()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await AddTestMessageListAsync(db).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(TestData.AccountWithFolder.Email, TestData.Folder, (9, 1)).ConfigureAwait(true);

            Assert.That(receivedMessageList.Count, Is.EqualTo(5));
            Assert.That(receivedMessageList[0].Id, Is.EqualTo(8));
            Assert.That(receivedMessageList[1].Id, Is.EqualTo(7));
            Assert.That(receivedMessageList[2].Id, Is.EqualTo(5));
            Assert.That(receivedMessageList[3].Id, Is.EqualTo(3));
            Assert.That(receivedMessageList[4].Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetMessageListByRange3To7()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await AddTestMessageListAsync(db).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(TestData.AccountWithFolder.Email, TestData.Folder, (3, 7)).ConfigureAwait(true);

            Assert.That(receivedMessageList.Count, Is.EqualTo(2));
            Assert.That(receivedMessageList[0].Id, Is.EqualTo(5));
            Assert.That(receivedMessageList[1].Id, Is.EqualTo(3));
        }

        [Test]
        public async Task GetMessageListByRange3To7Fast()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await AddTestMessageListAsync(db).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(TestData.AccountWithFolder.Email, TestData.Folder, (3, 7), fast: true).ConfigureAwait(true);

            Assert.That(receivedMessageList.Count, Is.EqualTo(2));
            Assert.That(receivedMessageList[0].Id, Is.EqualTo(5));
            Assert.That(receivedMessageList[1].Id, Is.EqualTo(3));
        }

        [Test]
        public async Task GetMessageListByRange7To7Fast()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await AddTestMessageListAsync(db).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(TestData.Email, TestData.Folder, (7, 7), fast: true).ConfigureAwait(true);

            Assert.That(receivedMessageList.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetMessageListByRange6To2Fast()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await AddTestMessageListAsync(db).ConfigureAwait(true);

            var receivedMessageList = await db.GetMessageListAsync(TestData.AccountWithFolder.Email, TestData.Folder, (6, 2), fast: true).ConfigureAwait(true);

            Assert.That(receivedMessageList.Count, Is.EqualTo(2));
            Assert.That(receivedMessageList[0].Id, Is.EqualTo(5));
            Assert.That(receivedMessageList[1].Id, Is.EqualTo(3));
        }

        private static async Task AddTestMessageListAsync(IDataStorage db)
        {
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var addMessageList = new List<Message>()
            {
                TestData.CreateNewMessage(TestData.Folder, 1, DateTimeOffset.Now),
                TestData.CreateNewMessage(TestData.Folder, 3, DateTimeOffset.Now),
                TestData.CreateNewMessage(TestData.Folder, 5, DateTimeOffset.Now),
                TestData.CreateNewMessage(TestData.Folder, 7, DateTimeOffset.Now),
                TestData.CreateNewMessage(TestData.Folder, 8, DateTimeOffset.Now),
            };
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);
        }

        [Test]
        public async Task GetUnreadMessagesCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);

            var storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);
            Assert.AreEqual(storedUnreadMessagesCount, unreadMessagesCount);
            var newMessage1 = TestData.GetNewUnreadMessage();
            await db.AddMessageAsync(accountEmail, newMessage1).ConfigureAwait(true);
            storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);
            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 1));
            var newMessage2 = TestData.GetNewUnreadMessage();
            await db.AddMessageAsync(accountEmail, newMessage2).ConfigureAwait(true);
            storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);
            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 2));
            newMessage2.IsMarkedAsRead = true;
            await db.UpdateMessageAsync(accountEmail, newMessage2).ConfigureAwait(true);
            storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);
            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 1));
            newMessage1.IsMarkedAsRead = true;
            var updatedMessages = new List<Message>() { newMessage1, newMessage2 };
            await db.UpdateMessagesFlagsAsync(accountEmail, updatedMessages).ConfigureAwait(true);
            storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);
            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
            newMessage1.IsMarkedAsRead = false;
            newMessage2.IsMarkedAsRead = false;
            await db.UpdateMessagesFlagsAsync(accountEmail, updatedMessages).ConfigureAwait(true);
            storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);
            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 2));
        }

        [Test]
        public async Task AddUnreadMessageShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);

            var newMessage1 = TestData.GetNewUnreadMessage();
            await db.AddMessageAsync(accountEmail, newMessage1, updateUnreadAndTotal: false).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task AddUnreadMessageShouldChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);

            var newMessage1 = TestData.GetNewUnreadMessage();
            await db.AddMessageAsync(accountEmail, newMessage1).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 1));
        }

        public async Task AddUnreadMessagesShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);

            var newMessages = new List<Message>() { TestData.GetNewUnreadMessage(),
                                                    TestData.GetNewUnreadMessage() };
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task AddReadMessageShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);

            var newMessage1 = TestData.GetNewReadMessage();
            await db.AddMessageAsync(accountEmail, newMessage1).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        public async Task AddReadMessagesShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);

            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage() };
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task UpdateUnreadMessageShouldChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessage1 = TestData.GetNewReadMessage();
            await db.AddMessageAsync(accountEmail, newMessage1).ConfigureAwait(true);

            newMessage1.IsMarkedAsRead = false;
            await db.UpdateMessageAsync(accountEmail, newMessage1).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 1));
        }

        [Test]
        public async Task UpdateUnreadMessageShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessage1 = TestData.GetNewReadMessage();
            await db.AddMessageAsync(accountEmail, newMessage1).ConfigureAwait(true);

            await db.UpdateMessageAsync(accountEmail, newMessage1).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task UpdateUnreadMessagesShouldChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);

            newMessages[0].IsMarkedAsRead = false;
            await db.UpdateMessagesAsync(accountEmail, newMessages).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 1));
        }

        [Test]
        public async Task UpdateUnreadMessagesShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);

            await db.UpdateMessagesAsync(accountEmail, newMessages).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        public async Task UpdateMessagesShouldChangeExternalId()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var newMessages = new List<Message>() { TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);
            var storedMessages = await db.GetMessageListAsync(accountEmail, TestData.Folder, 100).ConfigureAwait(true);
            Assert.IsTrue(storedMessages.Count == 1);
            int pk = storedMessages[0].Pk;
            uint oldId = storedMessages[0].Id;
            newMessages[0].Id = 98765;
            await db.UpdateMessagesAsync(accountEmail, newMessages).ConfigureAwait(true);

            storedMessages = await db.GetMessageListAsync(accountEmail, TestData.Folder, 100).ConfigureAwait(true);
            Assert.IsTrue(storedMessages.Count == 1);
            Assert.IsTrue(storedMessages[0].Id == newMessages[0].Id);
            Assert.IsFalse(storedMessages[0].Id == oldId);
            Assert.IsTrue(storedMessages[0].Pk == pk);
        }

        [Test]
        public async Task UpdateMessagesFlagsShouldChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);

            newMessages[0].IsMarkedAsRead = false;
            await db.UpdateMessagesFlagsAsync(accountEmail, newMessages).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 1));
        }

        [Test]
        public async Task UpdateMessagesFlagsOnSyncShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages, updateUnreadAndTotal: false).ConfigureAwait(true);

            newMessages[0].IsMarkedAsRead = false;
            await db.UpdateMessagesFlagsAsync(accountEmail, newMessages, updateUnreadAndTotal: false).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task UpdateMessagesFlagsShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);

            await db.UpdateMessagesFlagsAsync(accountEmail, newMessages).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task DeleteUnreadMessageShouldChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var unreadMessage = TestData.GetNewUnreadMessage();
            await db.AddMessageAsync(accountEmail, unreadMessage).ConfigureAwait(true);

            await db.DeleteMessageAsync(accountEmail, TestData.Folder, unreadMessage.Id).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteReadMessageShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var readMessage = TestData.GetNewReadMessage();
            await db.AddMessageAsync(accountEmail, readMessage).ConfigureAwait(true);

            await db.DeleteMessageAsync(accountEmail, TestData.Folder, readMessage.Id).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteUnreadMessagesShouldChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewUnreadMessage(),
                                                    TestData.GetNewUnreadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);
            await db.AddMessageAsync(accountEmail, TestData.GetNewUnreadMessage()).ConfigureAwait(true);

            await db.DeleteMessagesAsync(accountEmail, TestData.Folder, newMessages.Select(x => x.Id).ToList()).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount + 1));
        }

        [Test]
        public async Task DeleteReadMessagesShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);

            await db.DeleteMessagesAsync(accountEmail, TestData.Folder, newMessages.Select(x => x.Id).ToList()).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task DeleteReadMessagesOnSyncShouldNotChangeFolderMessageCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int unreadMessagesCount = await AddRandomUnreadMessagesAsync(db, accountEmail, 10).ConfigureAwait(true);
            var newMessages = new List<Message>() { TestData.GetNewReadMessage(),
                                                    TestData.GetNewReadMessage()};
            await db.AddMessageListAsync(accountEmail, TestData.Folder, newMessages).ConfigureAwait(true);

            await db.DeleteMessagesAsync(accountEmail, TestData.Folder, newMessages.Select(x => x.Id).ToList(), updateUnreadAndTotal: false).ConfigureAwait(true);
            int storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.That(storedUnreadMessagesCount, Is.EqualTo(unreadMessagesCount));
        }

        [Test]
        public async Task AddMessageShouldIncrementFolderLoadedCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);

            await db.AddMessageAsync(accountEmail, TestData.GetNewMessage()).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);

            Assert.That(account.FoldersStructure[0].LocalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateMessageShouldNotChangeFolderLoadedCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var message = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);
            message.IsFlagged = true;

            await db.UpdateMessageAsync(accountEmail, message).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);
            Assert.That(account.FoldersStructure[0].LocalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleMessageShouldDecrementFolderLoadedCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var message = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            await db.DeleteMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);
            Assert.That(account.FoldersStructure[0].LocalCount, Is.EqualTo(0));
        }

        [Test]
        public async Task AddMessageShouldIncrementFolderTotalCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);

            await db.AddMessageAsync(accountEmail, TestData.GetNewMessage()).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);

            Assert.That(account.FoldersStructure[0].TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task AddMessageShouldNotIncrementFolderTotalCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);

            await db.AddMessageAsync(accountEmail, TestData.GetNewMessage(), updateUnreadAndTotal: false).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);

            Assert.That(account.FoldersStructure[0].TotalCount, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateMessageShouldNotChangeFolderTotalCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var message = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);
            message.IsFlagged = true;

            await db.UpdateMessageAsync(accountEmail, message).ConfigureAwait(true);
            await db.UpdateMessageAsync(accountEmail, message, updateUnreadAndTotal: false).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);
            Assert.That(account.FoldersStructure[0].TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleMessageShouldDecrementFolderTotalCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var message = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            await db.DeleteMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);
            Assert.That(account.FoldersStructure[0].TotalCount, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleMessageShouldNotDecrementFolderTotalCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var message = TestData.GetNewMessage();
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            await db.DeleteMessageAsync(accountEmail, TestData.Folder, message.Id, updateUnreadAndTotal: false).ConfigureAwait(true);

            var account = await db.GetAccountAsync(accountEmail).ConfigureAwait(true);
            Assert.That(account.FoldersStructure[0].TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetContactUnreadMessagesCountAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            int messageCount = 10;
            int unreadMessagesCount = 0;
            var contactEmail = new EmailAddress("contact@mail.com", "CM");
            Random rnd = new Random();
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                Message message = null;
#pragma warning disable CA5394 // Do not use insecure randomness
                if (rnd.Next(2) % 2 == 1)
#pragma warning restore CA5394 // Do not use insecure randomness
                {
                    message = TestData.GetNewReadMessage();
                }
                else
                {
                    message = TestData.GetNewUnreadMessage();
                    unreadMessagesCount++;
                }

                message.From.Add(contactEmail);
                addMessageList.Add(message);
            }
            await db.AddMessageListAsync(TestData.AccountWithFolder.Email, TestData.Folder, addMessageList).ConfigureAwait(true);

            var storedUnreadMessagesCount = await db.GetContactUnreadMessagesCountAsync(contactEmail, default).ConfigureAwait(true);

            Assert.AreEqual(storedUnreadMessagesCount, unreadMessagesCount);
        }

        //[Test]
        //public async Task ContactsUnreadCountShouldNotBeNegativeAsync()
        //{
        //    using var db = await OpenDataStorageAsync().ConfigureAwait(true);
        //    await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
        //    var message = TestData.GetNewReadMessage();
        //    message.From.Add(new EmailAddress("from@test.mail"));
        //    message.ReplyTo.Add(new EmailAddress("replyto@test.mail"));
        //    message.To.Add(new EmailAddress("to@test.mail"));
        //    message.Cc.Add(new EmailAddress("cc@test.mail"));
        //    message.Bcc.Add(new EmailAddress("bcc@test.mail"));

        //    db.AddContactAsync(new Contact(message.From))


        //}

        [Test]
        public async Task GetContactsUnreadMessagesCountAsync()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            int messageCount = 10;
            int contactCount = 10;
            int unreadMessagesCount = 0;
            var contacts = new List<EmailAddress>(contactCount);
            for (int j = 0; j < contactCount; ++j)
            {
                var contactEmail = new EmailAddress($"contact{j}@mail.com", $"CM{j}");
                contacts.Add(contactEmail);
                await db.AddContactAsync(new Contact($"Contact {j}", contactEmail), default).ConfigureAwait(true);
                Random rnd = new Random();
                var addMessageList = new List<Message>();
                for (int i = 0; i < messageCount; ++i)
                {
                    Message message = null;
#pragma warning disable CA5394 // Do not use insecure randomness
                    if (rnd.Next(2) % 2 == 1)
#pragma warning restore CA5394 // Do not use insecure randomness
                    {
                        message = TestData.GetNewReadMessage();
                    }
                    else
                    {
                        message = TestData.GetNewUnreadMessage();
                        unreadMessagesCount++;
                    }

                    message.From.Add(contactEmail);
                    addMessageList.Add(message);
                }
                await db.AddMessageListAsync(TestData.AccountWithFolder.Email, TestData.Folder, addMessageList).ConfigureAwait(true);
            }

            var storedCounts = await db.GetUnreadMessagesCountByContactAsync(default).ConfigureAwait(true);

            // each message linked to two contacts: from and contact
            var expectedCount = unreadMessagesCount * 2;
            Assert.AreEqual(expectedCount, storedCounts.Select(x => x.Value).Sum());
            Assert.That(storedCounts.Count, Is.EqualTo(contactCount + 1));
            var fromAddress = new EmailAddress("from@mail.com", "TM");
            Assert.That(storedCounts.Where(x => x.Key == fromAddress).FirstOrDefault().Value, Is.EqualTo(unreadMessagesCount));
        }

        static async Task<int> AddRandomUnreadMessagesAsync(IDataStorage db, EmailAddress accountEmail, int messageCount)
        {
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            int unreadMessagesCount = 0;
            Random rnd = new Random();
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
#pragma warning disable CA5394 // Do not use insecure randomness
                if (rnd.Next(2) % 2 == 1)
#pragma warning restore CA5394 // Do not use insecure randomness
                {
                    addMessageList.Add(TestData.GetNewReadMessage());
                }
                else
                {
                    addMessageList.Add(TestData.GetNewUnreadMessage());
                    unreadMessagesCount++;
                }
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);
            return unreadMessagesCount;
        }

        [Test]
        public async Task GetUnreadMessagesCountNone()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewReadMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            var storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.AreEqual(storedUnreadMessagesCount, 0);
        }

        [Test]
        public async Task GetUnreadMessagesCountAll()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewUnreadMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            var storedUnreadMessagesCount = await db.GetUnreadMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.AreEqual(storedUnreadMessagesCount, messageCount);
        }

        [Test]
        public async Task GetAllInboxMessagesShouldReturnAllMessages()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var account1 = TestData.AccountWithFolder;
            var account2 = TestData.CreateAccountWithFolder(new EmailAddress("account2@mail.box"));

            await db.AddAccountAsync(account1).ConfigureAwait(true);
            await db.AddAccountAsync(account2).ConfigureAwait(true);
            var now = DateTimeOffset.Now;
            var messageList1 = new List<Message>()
            {
                TestData.CreateNewMessage(TestData.Folder, 1, now.AddMinutes(1)),
            };
            var messageList2 = new List<Message>()
            {
                TestData.CreateNewMessage(TestData.Folder, 101, now.AddMinutes(2)),
            };
            await db.AddMessageListAsync(account1.Email, TestData.Folder, messageList1).ConfigureAwait(true);
            await db.AddMessageListAsync(account2.Email, TestData.Folder, messageList2).ConfigureAwait(true);


            var messages = await db.GetEarlierMessagesAsync(null, 10, null, default).ConfigureAwait(true);
            Assert.That(messages.Count, Is.EqualTo(2));
            Assert.That(messages[0].Id, Is.EqualTo(101));
            Assert.That(messages[1].Id, Is.EqualTo(1));
            messages = await db.GetEarlierMessagesAsync(null, 10, messages[0], default).ConfigureAwait(true);
            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0].Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllInboxMessagesShouldReturnSingleMessage()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            var account1 = TestData.AccountWithFolder;
            var account2 = TestData.CreateAccountWithFolder(new EmailAddress("account2@mail.box"));
            account1.FoldersStructure[0].TotalCount = 10;
            account2.FoldersStructure[0].TotalCount = 10;

            await db.AddAccountAsync(account1).ConfigureAwait(true);
            await db.AddAccountAsync(account2).ConfigureAwait(true);
            var now = DateTimeOffset.Now;
            var messageList1 = new List<Message>()
            {
                TestData.CreateNewMessage(TestData.Folder, 1, now.AddMinutes(1)),
            };
            var messageList2 = new List<Message>()
            {
                TestData.CreateNewMessage(TestData.Folder, 101, now.AddMinutes(2)),
            };
            await db.AddMessageListAsync(account1.Email, TestData.Folder, messageList1).ConfigureAwait(true);
            await db.AddMessageListAsync(account2.Email, TestData.Folder, messageList2).ConfigureAwait(true);


            var messages = await db.GetEarlierMessagesAsync(null, 20, null, default).ConfigureAwait(true);
            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0].Id, Is.EqualTo(101));
            messages = await db.GetEarlierMessagesAsync(null, 20, messages[0], default).ConfigureAwait(true);
            Assert.That(messages.Count, Is.EqualTo(0));
        }


        [Test]
        public async Task GetMessagesCount()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            int messageCount = 10;
            var addMessageList = new List<Message>();
            for (int i = 0; i < messageCount; ++i)
            {
                addMessageList.Add(TestData.GetNewUnreadMessage());
            }
            await db.AddMessageListAsync(accountEmail, TestData.Folder, addMessageList).ConfigureAwait(true);

            var storedMessagesCount = await db.GetMessagesCountAsync(accountEmail, TestData.Folder).ConfigureAwait(true);

            Assert.AreEqual(storedMessagesCount, messageCount);
        }

        [Test]
        public async Task GetAttachments()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.GetNewMessage();
            message.Attachments.Add(TestData.Attachment);
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            message = await db.GetMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);

            Assert.IsTrue(message.Attachments.Count == 1);
            Assert.IsTrue(message.Attachments.First().FileName == TestData.Attachment.FileName);
            Assert.IsTrue(message.Attachments.First().Data.SequenceEqual(TestData.Attachment.Data));
            Assert.IsTrue(message.Attachments.First().MessageId == message.Pk);
        }

        [Test]
        public async Task GetProtection()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.GetNewMessage();
            message.Protection = TestData.Protection;
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            message = await db.GetMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);
            var signatureInfo = message.Protection.SignaturesInfo.First();

            Assert.IsTrue(message.Protection.Type == TestData.Protection.Type);
            Assert.IsTrue(message.Protection.SignaturesInfo.Count == 1);
            Assert.IsTrue(signatureInfo.SignerEmail == TestData.SignatureInfo.SignerEmail);
            Assert.IsTrue(signatureInfo.SignerFingerprint == TestData.SignatureInfo.SignerFingerprint);
            Assert.IsTrue(signatureInfo.DigestAlgorithm == TestData.SignatureInfo.DigestAlgorithm);
        }

        [Test]
        public async Task GetAddresses()
        {
            using var db = await OpenDataStorageAsync().ConfigureAwait(true);
            await db.AddAccountAsync(TestData.AccountWithFolder).ConfigureAwait(true);
            var accountEmail = TestData.AccountWithFolder.Email;
            var message = TestData.GetNewMessage();
            message.From[0] = TestData.Email;
            message.To[0] = TestData.Email;
            message.Cc.Add(TestData.Email);
            message.Bcc.Add(TestData.Email);
            message.ReplyTo.Add(TestData.Email);
            await db.AddMessageAsync(accountEmail, message).ConfigureAwait(true);

            message = await db.GetMessageAsync(accountEmail, TestData.Folder, message.Id).ConfigureAwait(true);

            Assert.IsTrue(message.From.First().Equals(TestData.Email));
            Assert.IsTrue(message.To.First().Equals(TestData.Email));
            Assert.IsTrue(message.Cc.First().Equals(TestData.Email));
            Assert.IsTrue(message.Bcc.First().Equals(TestData.Email));
            Assert.IsTrue(message.ReplyTo.First().Equals(TestData.Email));
        }
    }
}
