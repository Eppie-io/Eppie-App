using NUnit.Framework;
using Tuvi.App.ViewModels;

namespace Tuvi.App.ViewModels.Tests
{
    [TestFixture]
    public class ThemeSettingsTests
    {
        [Test]
        public void ThemeSelectedIndexDefaultsToZero()
        {
            // Arrange & Act
            var vm = new SettingsPageViewModel();
            vm.OnNavigatedTo(null);

            // Assert
            Assert.That(vm.ThemeSelectedIndex, Is.EqualTo(0));
        }

        [Test]
        public void ThemeSelectedIndexCanBeChangedToLight()
        {
            // Arrange
            var vm = new SettingsPageViewModel();
            vm.OnNavigatedTo(null);

            // Act
            vm.ThemeSelectedIndex = 1;

            // Assert
            Assert.That(vm.ThemeSelectedIndex, Is.EqualTo(1));
        }

        [Test]
        public void ThemeSelectedIndexCanBeChangedToDark()
        {
            // Arrange
            var vm = new SettingsPageViewModel();
            vm.OnNavigatedTo(null);

            // Act
            vm.ThemeSelectedIndex = 2;

            // Assert
            Assert.That(vm.ThemeSelectedIndex, Is.EqualTo(2));
        }

        [Test]
        public void ThemeSelectedIndexChangesOnlyWhenValueDiffers()
        {
            // Arrange
            var vm = new SettingsPageViewModel();
            vm.OnNavigatedTo(null);
            vm.ThemeSelectedIndex = 1;

            // Act - set to the same value
            vm.ThemeSelectedIndex = 1;

            // Assert - should remain the same
            Assert.That(vm.ThemeSelectedIndex, Is.EqualTo(1));
        }
    }
}
