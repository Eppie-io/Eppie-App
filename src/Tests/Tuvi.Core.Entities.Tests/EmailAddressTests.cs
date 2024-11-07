using NUnit.Framework;

namespace Tuvi.Core.Entities.Test
{
    public class EmailAddressTests
    {
        [Test]
        public void ComparisonTest()
        {
            Assert.That(new EmailAddress("Test@address.a"), Is.EqualTo(new EmailAddress("test@address.A")));

            var a = new EmailAddress("A@address.io");
            var b = new EmailAddress("B@address.io");
            Assert.That(a, Is.Not.EqualTo(b));

            var a2 = new EmailAddress("a@address.Io");
            var a3 = new EmailAddress("A@Address.io", "Name");
            var a4 = new EmailAddress("a@Address.io", "Name");
            var a5 = new EmailAddress("a@Address.io", "Name2");
            var b2 = new EmailAddress("b@Address.io", "Name");

            // Test operator overloads
            Assert.That(a != b);
            Assert.That(a2 == a3);
            Assert.That(a3 == a4);
            Assert.That(a4 == a5);
            Assert.That(a3 != b2);
        }
    }
}
