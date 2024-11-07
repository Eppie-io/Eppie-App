using NUnit.Framework;

namespace Tuvi.Core.Entities.Test
{
    public class FolderTests
    {
        [Test]
        public void EqualityTest()
        {
            var folder1 = new Folder("Folder1", FolderAttributes.Inbox);
            var folder2 = new Folder("Folder2", FolderAttributes.Inbox);
            var folder3 = new Folder("Folder1", FolderAttributes.Inbox);
            var folder4 = new Folder("Folder1", FolderAttributes.None);

            Assert.That(folder1, Is.Not.EqualTo(folder2));
            Assert.That(folder3, Is.Not.EqualTo(folder4));
            Assert.That(folder1, Is.Not.EqualTo(folder4));
            Assert.That(new Folder("Folder1", FolderAttributes.Inbox) { AccountEmail = new EmailAddress("address@test.t") },
             Is.Not.EqualTo(new Folder("Folder1", FolderAttributes.Inbox) { AccountEmail = new EmailAddress("address2@test.t") }));


            Assert.That(new Folder("Folder1", FolderAttributes.Inbox) { AccountEmail = new EmailAddress("address@test.t") },
             Is.Not.EqualTo(new Folder("Folder1", FolderAttributes.Draft) { AccountEmail = new EmailAddress("address@test.t") }));
            Assert.That(new Folder("Folder1", FolderAttributes.Inbox) { AccountEmail = new EmailAddress("address@test.t") },
             Is.Not.EqualTo(new Folder("Folder2", FolderAttributes.Inbox) { AccountEmail = new EmailAddress("address@test.t") }));

            Assert.That(folder1, Is.EqualTo(folder1));
            Assert.That(folder1, Is.EqualTo(folder3));
            Assert.That(new Folder("Folder1", FolderAttributes.Inbox) { AccountId = 1, Id = 40 },
             Is.EqualTo(new Folder("Folder1", FolderAttributes.Inbox) { AccountId = 1, Id = 40 }));
        }

    }
}
