using NUnit.Framework;

namespace Tuvi.Core.Entities.Test
{
    public class StringHelperTests
    {
        [Test]
        public void AreEmailsEqualOneArgumentIsNull()
        {
            Assert.IsFalse(StringHelper.AreEmailsEqual("some@email.com", null));
        }

        [Test]
        public void AreEmailsEqualTwoArgumentsAreNull()
        {
            Assert.IsTrue(StringHelper.AreEmailsEqual(null, null));
        }
    }
}
