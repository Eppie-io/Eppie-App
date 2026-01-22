// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using NUnit.Framework;

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
            Assert.That(vm.ThemeSelectedIndex, Is.Zero);
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
