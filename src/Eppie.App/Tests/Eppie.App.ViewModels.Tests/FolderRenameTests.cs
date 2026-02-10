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
using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public sealed class FolderRenameTests
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
        public async Task RenameFolderAsyncValidInputShouldRenameFolderAndRaiseEvent()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");
                var folder = new Folder { FullName = "OldFolderName" };
                var newName = "NewFolderName";

                bool eventRaised = false;
                Folder? renamedFolder = null;
                string? oldFullName = null;
                core.FolderRenamed += (sender, args) =>
                {
                    eventRaised = true;
                    renamedFolder = args.Folder;
                    oldFullName = args.OldFullName;
                };

                // Act
                await vm.RenameFolderAsync(accountEmail, folder, newName).ConfigureAwait(false);

                // Assert
                Assert.That(eventRaised, Is.True, "FolderRenamed event should be raised");
                Assert.That(renamedFolder, Is.Not.Null, "Renamed folder should not be null");
                Assert.That(renamedFolder!.FullName, Is.EqualTo(newName), "Folder name should be updated");
                Assert.That(oldFullName, Is.EqualTo("OldFolderName"), "Old name should be preserved in event args");
            }
        }

        [Test]
        public async Task RenameFolderAsyncShouldTriggerViewModelEventSubscription()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");
                var folder = new Folder { FullName = "TestFolder" };
                var newName = "RenamedFolder";

                // Call OnNavigatedTo so the ViewModel subscribes to Core events (e.g., FolderRenamed)
                vm.OnNavigatedTo(null);

                int getCompositeAccountsCallsBefore = core.GetCompositeAccountsCalls;

                // Act
                await vm.RenameFolderAsync(accountEmail, folder, newName).ConfigureAwait(false);

                // Assert - verify that the ViewModel's event handler triggered UpdateAccountsList
                // which should call GetCompositeAccountsAsync
                // Note: TestDispatcherService executes synchronously, so no delay needed
                Assert.That(core.GetCompositeAccountsCalls, Is.GreaterThan(getCompositeAccountsCallsBefore),
                    "ViewModel should call GetCompositeAccountsAsync when FolderRenamed event is raised");
            }
        }

        [Test]
        public void RenameFolderAsyncEmptyNewNameShouldNotThrow()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");
                var folder = new Folder { FullName = "TestFolder" };

                // Act & Assert - empty name is handled by the Core implementation
                Assert.DoesNotThrowAsync(async () =>
                {
                    await vm.RenameFolderAsync(accountEmail, folder, string.Empty).ConfigureAwait(false);
                });
            }
        }
    }
}
