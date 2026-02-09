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

using Eppie.App.ViewModels.Tests.TestDoubles;
using NUnit.Framework;
using Tuvi.App.ViewModels;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public sealed class FolderCreationTests
    {
        private static (MainPageViewModel Vm, FakeTuviMail Core) CreateViewModel()
        {
            var core = new FakeTuviMail();
            var vm = new MainPageViewModel();

            vm.SetCoreProvider(() => core);
            vm.SetDispatcherService(new TestDispatcherService());
            vm.SetLocalSettingsService(new TestLocalSettingsService());
            vm.SetLocalizationService(new TestLocalizationService());
            vm.SetMessageService(new TestMessageService());

            return (vm, core);
        }

        [Test]
        public async Task CreateFolderAsyncValidInputShouldCreateFolderAndRaiseEvent()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");
                var folderName = "TestFolder";

                bool eventRaised = false;
                Folder? createdFolder = null;
                core.FolderCreated += (sender, args) =>
                {
                    eventRaised = true;
                    createdFolder = args.Folder;
                };

                // Act
                await vm.CreateFolderAsync(accountEmail, folderName).ConfigureAwait(false);

                // Assert
                Assert.That(eventRaised, Is.True, "FolderCreated event should be raised");
                Assert.That(createdFolder, Is.Not.Null, "Created folder should not be null");
                Assert.That(createdFolder!.FullName, Is.EqualTo(folderName), "Folder name should match");
            }
        }

        [Test]
        public void CreateFolderAsyncEmptyFolderNameShouldNotThrow()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");

                // Act & Assert
                Assert.DoesNotThrowAsync(async () =>
                {
                    await vm.CreateFolderAsync(accountEmail, string.Empty).ConfigureAwait(false);
                });
            }
        }

        [Test]
        public async Task CreateFolderAsyncShouldTriggerViewModelEventSubscription()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");
                var folderName = "TestFolder";

                vm.OnNavigatedTo(null);

                bool folderCreatedEventFired = false;
                core.FolderCreated += (sender, args) =>
                {
                    folderCreatedEventFired = true;
                };

                // Act
                await vm.CreateFolderAsync(accountEmail, folderName).ConfigureAwait(false);

                // Assert
                Assert.That(folderCreatedEventFired, Is.True, "FolderCreated event should be fired and handled by ViewModel");
            }
        }
    }
}
