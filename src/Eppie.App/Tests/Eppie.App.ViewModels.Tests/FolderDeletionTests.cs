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
    public sealed class FolderDeletionTests
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
        public async Task DeleteFolderAsyncValidInputShouldDeleteFolderAndRaiseEvent()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");
                var folder = new CompositeFolder(new Folder { FullName = "TestFolder" });

                bool eventRaised = false;
                Folder? deletedFolder = null;
                core.FolderDeleted += (sender, args) =>
                {
                    eventRaised = true;
                    deletedFolder = args.Folder;
                };

                // Act
                await vm.DeleteFolderAsync(accountEmail, folder).ConfigureAwait(false);

                // Assert
                Assert.That(eventRaised, Is.True, "FolderDeleted event should be raised");
                Assert.That(deletedFolder, Is.Not.Null, "Deleted folder should not be null");
                Assert.That(deletedFolder!.FullName, Is.EqualTo("TestFolder"), "Folder name should match");
            }
        }

        [Test]
        public async Task DeleteFolderAsyncShouldTriggerViewModelEventSubscription()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");
                var folder = new CompositeFolder(new Folder { FullName = "TestFolder" });

                // Call OnNavigatedTo so the ViewModel subscribes to Core events (e.g., FolderDeleted)
                vm.OnNavigatedTo(null);

                int getCompositeAccountsCallsBefore = core.GetCompositeAccountsCalls;

                // Act
                await vm.DeleteFolderAsync(accountEmail, folder).ConfigureAwait(false);
                
                // Assert - verify that the ViewModel's event handler triggered UpdateAccountsList
                // which should call GetCompositeAccountsAsync
                // Note: TestDispatcherService executes synchronously, so no delay needed
                Assert.That(core.GetCompositeAccountsCalls, Is.GreaterThan(getCompositeAccountsCallsBefore), 
                    "ViewModel should call GetCompositeAccountsAsync when FolderDeleted event is raised");
            }
        }

        [Test]
        public void DeleteFolderAsyncNullFolderShouldNotThrow()
        {
            // Arrange
            var (vm, core) = CreateViewModel();
            using (core)
            {
                vm.InitializeMailboxModel(null, null);
                var accountEmail = new EmailAddress("test@example.com");

                // Act & Assert - null is now handled gracefully by short-circuiting
                Assert.DoesNotThrowAsync(async () =>
                {
                    await vm.DeleteFolderAsync(accountEmail, null!).ConfigureAwait(false);
                });
            }
        }
    }
}
