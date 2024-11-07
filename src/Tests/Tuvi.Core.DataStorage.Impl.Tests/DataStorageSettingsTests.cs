using NUnit.Framework;
using System.Threading.Tasks;

namespace Tuvi.Core.DataStorage.Tests
{
    public class SettingsTests : TestWithStorageBase
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
        public async Task SetDecentralizedAccountCounter()
        {
            using (var db = GetDataStorage())
            {
                await db.OpenAsync(Password).ConfigureAwait(true);

                var settings = await db.GetSettingsAsync().ConfigureAwait(true);

                Assert.IsTrue(settings.DecentralizedAccountCounter == 0);

                settings.DecentralizedAccountCounter = 123;
                await db.SetSettingsAsync(settings).ConfigureAwait(true);

                var settings2 = await db.GetSettingsAsync().ConfigureAwait(true);
                Assert.IsTrue(settings2.DecentralizedAccountCounter == 123);
            }
        }
    }
}
