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

using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Eppie.App.ViewModels.Tests.TestDoubles;
using NUnit.Framework;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Messages;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public sealed class ContactsPanelPageViewModelTests
    {
        private static readonly TimeSpan DefaultAsyncTimeout = Debugger.IsAttached ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(5);
        private static readonly TimeSpan SpinWaitPollInterval = TimeSpan.FromMilliseconds(10);

        private sealed class TestManagedCollection<T> : ManagedCollection<T>
        {
            public int RefreshCalls { get; private set; }

            protected override Task RefreshImplAsync()
            {
                RefreshCalls++;
                return Task.CompletedTask;
            }
        }

        private sealed class RecorderErrorHandler : Tuvi.App.ViewModels.Services.IErrorHandler
        {
            public readonly List<Exception> Errors = new();

            public void SetMessageService(Tuvi.App.ViewModels.Services.IMessageService messageService)
            {
            }

            public void OnError(Exception ex, bool silent = false)
            {
                if (ex != null)
                {
                    Errors.Add(ex);
                }
            }
        }

        private static Contact CreateContact(string email, string name, int unread = 0)
        {
            return new Contact
            {
                Email = new EmailAddress(email, name),
                FullName = name,
                UnreadCount = unread,
                LastMessageData = new LastMessageData { AccountEmail = new EmailAddress("acc@local") }
            };
        }

        private static (ContactsPanelPageViewModel Vm, TestManagedCollection<ContactItem> Collection, FakeTuviMail Core, TestLocalSettingsService Settings, RecorderErrorHandler Errors) CreateVm(
            IEnumerable<Contact>? seedContacts = null,
            IReadOnlyDictionary<EmailAddress, int>? unreadCounts = null)
        {
            var core = new FakeTuviMail();
            core.SeedContacts(seedContacts ?? Array.Empty<Contact>());
            core.SetUnreadCounts(unreadCounts ?? new Dictionary<EmailAddress, int>());

            var vm = new ContactsPanelPageViewModel();
            var settings = new TestLocalSettingsService();
            var errors = new RecorderErrorHandler();

            vm.SetCoreProvider(() => core);
            vm.SetDispatcherService(new TestDispatcherService());
            vm.SetLocalSettingsService(settings);
            vm.SetLocalizationService(new TestLocalizationService());
            vm.SetErrorHandler(errors);

            var collection = new TestManagedCollection<ContactItem>();
            vm.SetContactsCollection(collection);

            return (vm, collection, core, settings, errors);
        }

        private static async Task SpinWaitAsync(Func<bool> predicate, TimeSpan? timeout = null, string? failMessage = null)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var effectiveTimeout = timeout ?? DefaultAsyncTimeout;
            var sw = Stopwatch.StartNew();
            while (!predicate())
            {
                if (sw.Elapsed > effectiveTimeout)
                {
                    Assert.Fail(failMessage ?? $"Condition was not met within {effectiveTimeout.TotalSeconds:0.##}s timeout.");
                }

                await Task.Delay(SpinWaitPollInterval).ConfigureAwait(false);
            }
        }

        [TearDown]
        public void TearDown()
        {
            WeakReferenceMessenger.Default.Reset();
        }

        [Test]
        public void SetContactsCollectionSetsSearchFilter()
        {
            using var vm = new ContactsPanelPageViewModel();
            vm.SetCoreProvider(() => new FakeTuviMail());
            vm.SetLocalizationService(new TestLocalizationService());

            var collection = new ManagedCollection<ContactItem>();
            vm.SetContactsCollection(collection);

            Assert.That(collection.SearchFilter, Is.Not.Null);
        }

        [Test]
        public void ContactClickCommandNullArgumentThrowsArgumentNullException()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                Assert.That(() => vm.ContactClickCommand.Execute(null), Throws.InstanceOf<ArgumentNullException>());
            }
        }

        [Test]
        public void RenameContactCommandNullArgumentThrowsArgumentNullException()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                Assert.That(async () => await ((AsyncRelayCommand<ContactItem>)vm.RenameContactCommand).ExecuteAsync(null).ConfigureAwait(false), Throws.InstanceOf<ArgumentNullException>());
            }
        }

        [Test]
        public void ChangeContactAvatarCommandNullArgumentThrowsArgumentNullException()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                Assert.That(async () => await ((AsyncRelayCommand<ContactItem>)vm.ChangeContactAvatarCommand).ExecuteAsync(null).ConfigureAwait(false), Throws.InstanceOf<ArgumentNullException>());
            }
        }

        [Test]
        public void RemoveContactCommandNullArgumentThrowsArgumentNullException()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                Assert.That(async () => await ((AsyncRelayCommand<ContactItem>)vm.RemoveContactCommand).ExecuteAsync(null).ConfigureAwait(false), Throws.InstanceOf<ArgumentNullException>());
            }
        }

        [Test]
        public void ContactClickCommandSendsContactSelectedMessage()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                ContactSelectedMessage? received = null;
                WeakReferenceMessenger.Default.Register<ContactSelectedMessage>(this, (r, m) => received = m);

                var item = new ContactItem(new EmailAddress("a@b.com", "A"));

                vm.ContactClickCommand.Execute(item);

                Assert.That(received, Is.Not.Null);
                Assert.That(received!.Value, Is.SameAs(item));
            }
        }

        [Test]
        public async Task RenameContactCommandCallsCoreSetContactNameAsync()
        {
            var contact = CreateContact("user@site.com", "Old");
            var (vm, _, core, _, _) = CreateVm(new[] { contact });
            using (vm)
            {
                var item = new ContactItem(contact) { FullName = "New Name" };

                var command = (AsyncRelayCommand<ContactItem>)vm.RenameContactCommand;
                await command.ExecuteAsync(item).ConfigureAwait(false);

                Assert.That(core.LastSetContactName, Is.Not.Null);
                Assert.That(core.LastSetContactName!.Value.Email.Address, Is.EqualTo("user@site.com"));
                Assert.That(core.LastSetContactName!.Value.Name, Is.EqualTo("New Name"));
            }
        }

        [Test]
        public async Task ChangeContactAvatarCommandDoesNothingWhenNoProvider()
        {
            var contact = CreateContact("user@site.com", "User");
            var (vm, _, core, _, _) = CreateVm(new[] { contact });
            using (vm)
            {
                var command = (AsyncRelayCommand<ContactItem>)vm.ChangeContactAvatarCommand;
                await command.ExecuteAsync(new ContactItem(contact)).ConfigureAwait(false);

                Assert.That(core.SetContactAvatarCalls, Is.Zero);
            }
        }

        [Test]
        public async Task ChangeContactAvatarCommandCallsCoreWhenProviderReturnsBytes()
        {
            var contact = CreateContact("user@site.com", "User");
            var (vm, _, core, _, _) = CreateVm(new[] { contact });
            using (vm)
            {
                byte[] bytes = { 1, 2, 3 };
                vm.SetAvatarProvider(() => Task.FromResult(bytes));

                var command = (AsyncRelayCommand<ContactItem>)vm.ChangeContactAvatarCommand;
                await command.ExecuteAsync(new ContactItem(contact)).ConfigureAwait(false);

                Assert.That(core.LastSetContactAvatar, Is.Not.Null);
                Assert.That(core.LastSetContactAvatar!.Value.Email.Address, Is.EqualTo("user@site.com"));
                Assert.That(core.LastSetContactAvatar!.Value.Bytes, Is.EqualTo(bytes));
                Assert.That(core.LastSetContactAvatar!.Value.Width, Is.EqualTo(ContactItem.DefaultAvatarSize));
                Assert.That(core.LastSetContactAvatar!.Value.Height, Is.EqualTo(ContactItem.DefaultAvatarSize));
            }
        }

        [Test]
        public async Task ChangeContactAvatarCommandDoesNotCallCoreWhenProviderReturnsNull()
        {
            var contact = CreateContact("user@site.com", "User");
            var (vm, _, core, _, _) = CreateVm(new[] { contact });
            using (vm)
            {
                vm.SetAvatarProvider(() => Task.FromResult<byte[]?>(null));

                var command = (AsyncRelayCommand<ContactItem>)vm.ChangeContactAvatarCommand;
                await command.ExecuteAsync(new ContactItem(contact)).ConfigureAwait(false);

                Assert.That(core.SetContactAvatarCalls, Is.Zero);
            }
        }

        [Test]
        public async Task RemoveContactCommandCallsCoreRemoveContactAsync()
        {
            var contact = CreateContact("user@site.com", "User");
            var (vm, _, core, _, _) = CreateVm(new[] { contact });
            using (vm)
            {
                var command = (AsyncRelayCommand<ContactItem>)vm.RemoveContactCommand;
                await command.ExecuteAsync(new ContactItem(contact)).ConfigureAwait(false);

                Assert.That(core.LastRemovedContactEmail, Is.Not.Null);
                Assert.That(core.LastRemovedContactEmail!.Address, Is.EqualTo("user@site.com"));
            }
        }

        [Test]
        public void NavigatedToInitializesSortingVariantsAndRestoresSelection()
        {
            var (vm, _, _, settings, _) = CreateVm();
            using (vm)
            {
                settings.ContactsSortOrder = ContactsSortOrder.ByName;

                vm.OnNavigatedTo(null);

                Assert.That(vm.SortingVariants, Is.Not.Null);
                Assert.That(vm.SortingVariants.Length, Is.EqualTo(3));
                Assert.That(vm.SelectedSortingIndex, Is.Not.Negative);
                Assert.That(vm.SortOrder, Is.EqualTo(ContactsSortOrder.ByName));

                // Ensure labels are created (comes from localization service).
                Assert.That(vm.SortingVariants.All(v => !string.IsNullOrWhiteSpace(v.Label)), Is.True);
            }
        }

        [Test]
        public void SelectedSortingIndexUpdatesLocalSettingsAndRefreshesContacts()
        {
            var (vm, collection, _, settings, _) = CreateVm();
            using (vm)
            {
                vm.OnNavigatedTo(null);

                var initialRefresh = collection.RefreshCalls;

                var byNameIdx = Array.FindIndex(vm.SortingVariants, v => v.SortOrder == ContactsSortOrder.ByName);
                Assert.That(byNameIdx, Is.Not.Negative);

                vm.SelectedSortingIndex = byNameIdx;

                Assert.That(settings.ContactsSortOrder, Is.EqualTo(ContactsSortOrder.ByName));
                Assert.That(vm.SortOrder, Is.EqualTo(ContactsSortOrder.ByName));
                Assert.That(collection.RefreshCalls, Is.GreaterThan(initialRefresh));
                Assert.That(vm.SelectedContact, Is.Null);
            }
        }

        [Test]
        public void RequestSelectedContactMessageRepliesWithSelectedContact()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                vm.OnNavigatedTo(null);

                var selected = new ContactItem(new EmailAddress("x@y.com", "X"));
                vm.SelectedContact = selected;

                var request = new RequestSelectedContactMessage();
                WeakReferenceMessenger.Default.Send(request);

                Assert.That(request.HasReceivedResponse, Is.True);
                Assert.That(request.Response, Is.SameAs(selected));
            }
        }

        [Test]
        public void ClearSelectedContactMessageSetsSelectedContactToNull()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                vm.OnNavigatedTo(null);

                vm.SelectedContact = new ContactItem(new EmailAddress("x@y.com", "X"));

                WeakReferenceMessenger.Default.Send(new ClearSelectedContactMessage());

                Assert.That(vm.SelectedContact, Is.Null);
            }
        }

        [Test]
        public void NavigatedFromUnregistersMessages()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                vm.OnNavigatedTo(null);

                vm.OnNavigatedFrom();

                var request = new RequestSelectedContactMessage();
                WeakReferenceMessenger.Default.Send(request);

                Assert.That(request.HasReceivedResponse, Is.False);
            }
        }

        [Test]
        public async Task CoreUnreadEventsTriggerUnreadCountUpdateReconcileUpdatesContactItems()
        {
            var c1 = CreateContact("a@a.com", "A", unread: 0);
            var c2 = CreateContact("b@b.com", "B", unread: 0);

            var (vm, collection, core, _, _) = CreateVm(
                seedContacts: new[] { c1, c2 },
                unreadCounts: new Dictionary<EmailAddress, int>
                {
                    [c1.Email] = 0,
                    [c2.Email] = 0,
                });

            using (vm)
            {
                vm.OnNavigatedTo(null);

                // Initial load.
                var loaded = (await vm.ReloadLoadedItemsAsync(2, CancellationToken.None).ConfigureAwait(false)).ToList();
                collection.AddRange(loaded);

                Assert.That(collection.OriginalItems.Count, Is.EqualTo(2));
                Assert.That(collection.OriginalItems.All(i => i.UnreadMessagesCount == 0), Is.True);

                // Update counts, then raise event.
                core.SetUnreadCounts(new Dictionary<EmailAddress, int>
                {
                    [c1.Email] = 5,
                    [c2.Email] = 1,
                });

                core.RaiseUnreadMessagesReceived(new EmailAddress("acc@local"));

                await SpinWaitAsync(
                    () => collection.OriginalItems.Any(i => i.Email.Address == "a@a.com" && i.UnreadMessagesCount == 5)
                       && collection.OriginalItems.Any(i => i.Email.Address == "b@b.com" && i.UnreadMessagesCount == 1))
                    .ConfigureAwait(false);

                Assert.That(core.GetUnreadCountsCalls, Is.Positive);
            }
        }

        [Test]
        public async Task ContactChangedEventUpdatesExistingItemAndTriggersReconcile()
        {
            var c1 = CreateContact("a@a.com", "A", unread: 0);

            var (vm, collection, core, _, _) = CreateVm(seedContacts: new[] { c1 });
            using (vm)
            {
                vm.OnNavigatedTo(null);

                var loaded = (await vm.ReloadLoadedItemsAsync(1, CancellationToken.None).ConfigureAwait(false)).ToList();
                collection.AddRange(loaded);

                Assert.That(collection.OriginalItems.Single().FullName, Is.EqualTo("A"));

                // Update source & raise event
                var updated = CreateContact("a@a.com", "A2", unread: 3);
                core.SeedContacts(new[] { updated });
                core.RaiseContactChanged(updated);

                await SpinWaitAsync(() => collection.OriginalItems.Single().FullName == "A2").ConfigureAwait(false);
                Assert.That(collection.OriginalItems.Single().UnreadMessagesCount, Is.EqualTo(3));
            }
        }

        [Test]
        public async Task TriggerReconcileDebouncesMultipleEventsUsesLatestState()
        {
            var c1 = CreateContact("a@a.com", "A", unread: 0);
            var (vm, collection, core, _, _) = CreateVm(seedContacts: new[] { c1 });
            using (vm)
            {
                vm.OnNavigatedTo(null);

                var loaded = (await vm.ReloadLoadedItemsAsync(1, CancellationToken.None).ConfigureAwait(false)).ToList();
                collection.AddRange(loaded);

                // Fire two events quickly; second changes name.
                var updated1 = CreateContact("a@a.com", "A1", unread: 0);
                var updated2 = CreateContact("a@a.com", "A2", unread: 0);

                core.SeedContacts(new[] { updated1 });
                core.RaiseContactChanged(updated1);

                core.SeedContacts(new[] { updated2 });
                core.RaiseContactChanged(updated2);

                await SpinWaitAsync(() => collection.OriginalItems.Single().FullName == "A2").ConfigureAwait(false);
            }
        }

        [Test]
        public void DisposeUnregistersMessagesAndUnsubscribesFromCoreEvents()
        {
            var (vm, _, core, _, _) = CreateVm();
            vm.OnNavigatedTo(null);

            vm.Dispose();

            // Messages should not be handled.
            var request = new RequestSelectedContactMessage();
            WeakReferenceMessenger.Default.Send(request);
            Assert.That(request.HasReceivedResponse, Is.False);

            // Core events should not throw or update anything; just ensure no handler is attached.
            Assert.DoesNotThrow(() => core.RaiseUnreadMessagesReceived(new EmailAddress("acc@local")));
        }

        [Test]
        public async Task LoadMoreItemsAsyncUpdatesLastContactToSupportPagination()
        {
            var c1 = CreateContact("a@a.com", "A");
            var c2 = CreateContact("b@b.com", "B");
            var (vm, _, _, _, _) = CreateVm(new[] { c1, c2 });
            using (vm)
            {
                vm.SortOrder = ContactsSortOrder.ByName;

                // Load first item
                var batch1 = await vm.LoadMoreItemsAsync(1, CancellationToken.None).ConfigureAwait(false);
                Assert.That(batch1.Single().Email.Address, Is.EqualTo("a@a.com"));

                // Load second item (should skip first)
                var batch2 = await vm.LoadMoreItemsAsync(1, CancellationToken.None).ConfigureAwait(false);
                Assert.That(batch2.Single().Email.Address, Is.EqualTo("b@b.com"));
            }
        }

        [Test]
        public async Task ResetClearsLastContactRestartingPagination()
        {
            var c1 = CreateContact("a@a.com", "A");
            var c2 = CreateContact("b@b.com", "B");
            var (vm, _, _, _, _) = CreateVm(new[] { c1, c2 });
            using (vm)
            {
                vm.SortOrder = ContactsSortOrder.ByName;

                // Load first item
                var batch1 = await vm.LoadMoreItemsAsync(1, CancellationToken.None).ConfigureAwait(false);
                Assert.That(batch1.Single().Email.Address, Is.EqualTo("a@a.com"));

                vm.Reset();

                // Load first item again (should start over)
                var batch2 = await vm.LoadMoreItemsAsync(1, CancellationToken.None).ConfigureAwait(false);
                Assert.That(batch2.Single().Email.Address, Is.EqualTo("a@a.com"));
            }
        }

        [Test]
        public async Task ContactAddedEventTriggersReconcileAndUpdatesViewIfInRange()
        {
            // Initial: B. Sorted by Name.
            var cB = CreateContact("b@b.com", "B");
            var (vm, collection, core, _, _) = CreateVm(new[] { cB });
            using (vm)
            {
                vm.SortOrder = ContactsSortOrder.ByName;
                vm.OnNavigatedTo(null);

                // Load initial (B)
                var loaded = (await vm.ReloadLoadedItemsAsync(1, CancellationToken.None).ConfigureAwait(false)).ToList();
                collection.AddRange(loaded);
                Assert.That(collection.OriginalItems.Single().FullName, Is.EqualTo("B"));

                // Add A (should come before B)
                var cA = CreateContact("a@a.com", "A");
                core.SeedContacts(new[] { cA, cB });
                core.RaiseContactAdded(cA);

                // Reconcile repeats load of Count=1. Should get A now.
                await SpinWaitAsync(() => collection.OriginalItems.FirstOrDefault()?.FullName == "A").ConfigureAwait(false);
            }
        }

        [Test]
        public async Task ContactDeletedEventTriggersReconcileAndRemovesItem()
        {
            var cA = CreateContact("a@a.com", "A");
            var (vm, collection, core, _, _) = CreateVm(new[] { cA });
            using (vm)
            {
                vm.OnNavigatedTo(null);

                // Load initial
                var loaded = (await vm.ReloadLoadedItemsAsync(1, CancellationToken.None).ConfigureAwait(false)).ToList();
                collection.AddRange(loaded);
                Assert.That(collection.OriginalItems.Count, Is.EqualTo(1));

                // Delete A
                core.SeedContacts(Array.Empty<Contact>());
                core.RaiseContactDeleted(cA.Email);

                // Reconcile repeats load of Count=1. Source empty -> returns 0.
                await SpinWaitAsync(() => collection.OriginalItems.Count == 0).ConfigureAwait(false);
            }
        }

        [Test]
        public async Task ContactChangedEventDispatcherExceptionIsReportedToErrorHandler()
        {
            var c1 = CreateContact("a@a.com", "A", unread: 0);

            var (vm, _, core, _, errors) = CreateVm(seedContacts: new[] { c1 });
            using (vm)
            {
                vm.SetDispatcherService(new ThrowingDispatcherService(new InvalidOperationException("dispatcher failed")));

                vm.OnNavigatedTo(null);

                var updated = CreateContact("a@a.com", "A2", unread: 0);
                core.SeedContacts(new[] { updated });
                core.RaiseContactChanged(updated);

                await SpinWaitAsync(() => errors.Errors.Count > 0, failMessage: "Expected error handler to be invoked after dispatcher exception.").ConfigureAwait(false);
                Assert.That(errors.Errors.Last(), Is.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public async Task ContactAddedEventDispatcherExceptionIsReportedToErrorHandler()
        {
            var c1 = CreateContact("a@a.com", "A", unread: 0);

            var (vm, _, core, _, errors) = CreateVm(seedContacts: Array.Empty<Contact>());
            using (vm)
            {
                vm.SetDispatcherService(new ThrowingDispatcherService(new InvalidOperationException("dispatcher failed")));

                vm.OnNavigatedTo(null);

                core.SeedContacts(new[] { c1 });
                core.RaiseContactAdded(c1);

                await SpinWaitAsync(() => errors.Errors.Count > 0, failMessage: "Expected error handler to be invoked after dispatcher exception.").ConfigureAwait(false);
                Assert.That(errors.Errors.Last(), Is.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public async Task ContactDeletedEventDispatcherExceptionIsReportedToErrorHandler()
        {
            var c1 = CreateContact("a@a.com", "A", unread: 0);

            var (vm, _, core, _, errors) = CreateVm(seedContacts: new[] { c1 });
            using (vm)
            {
                vm.SetDispatcherService(new ThrowingDispatcherService(new InvalidOperationException("dispatcher failed")));

                vm.OnNavigatedTo(null);

                core.SeedContacts(Array.Empty<Contact>());
                core.RaiseContactDeleted(c1.Email);

                await SpinWaitAsync(() => errors.Errors.Count > 0, failMessage: "Expected error handler to be invoked after dispatcher exception.").ConfigureAwait(false);
                Assert.That(errors.Errors.Last(), Is.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public void ComposeEmailCommandNullArgumentThrowsArgumentNullException()
        {
            var (vm, _, _, _, _) = CreateVm();
            using (vm)
            {
                Assert.That(async () => await ((AsyncRelayCommand<ContactItem>)vm.ComposeEmailCommand).ExecuteAsync(null).ConfigureAwait(false), Throws.InstanceOf<ArgumentNullException>());
            }
        }

        [Test]
        public async Task ComposeEmailCommandNavigatesToNewMessagePageWithCorrectData()
        {
            var contact = CreateContact("user@site.com", "Test User");
            var (vm, _, _, _, _) = CreateVm(new[] { contact });
            using (vm)
            {
                var navService = new TestNavigationService();
                vm.SetNavigationService(navService);

                var contactItem = new ContactItem(contact);
                await ((AsyncRelayCommand<ContactItem>)vm.ComposeEmailCommand).ExecuteAsync(contactItem).ConfigureAwait(false);

                Assert.That(navService.LastNavigatedPage, Is.EqualTo(nameof(NewMessagePageViewModel)));
                Assert.That(navService.LastNavigationData, Is.Not.Null);
                Assert.That(navService.LastNavigationData, Is.TypeOf<SelectedContactNewMessageData>());

                var messageData = (SelectedContactNewMessageData)navService.LastNavigationData!;
                Assert.That(messageData.From.Address, Is.EqualTo("acc@local"));
                Assert.That(messageData.To, Is.EqualTo("user@site.com"));
            }
        }

        [Test]
        public async Task ComposeEmailCommandWithNoAccountsShowsAddAccountMessage()
        {
            var contact = CreateContact("user@site.com", "Test User");
            var (vm, _, _, _, _) = CreateVm(new[] { contact });
            using (vm)
            {
                var navService = new TestNavigationService();
                vm.SetNavigationService(navService);

                var messageService = new TestMessageService();
                vm.SetMessageService(messageService);

                // Create a ContactItem using the EmailAddress constructor
                var contactItem = new ContactItem(new EmailAddress("test@example.com", "Test Contact"));

                await ((AsyncRelayCommand<ContactItem>)vm.ComposeEmailCommand).ExecuteAsync(contactItem).ConfigureAwait(false);

                // Should not navigate when no accounts are available
                Assert.That(navService.LastNavigatedPage, Is.Null);
                Assert.That(messageService.ShowAddAccountMessageCalled, Is.True);
            }
        }

        [Test]
        public async Task ComposeEmailCommandWithNullEmailReportsError()
        {
            var contact = CreateContact("user@site.com", "Test User");
            var (vm, _, _, _, errors) = CreateVm(new[] { contact });
            using (vm)
            {
                var navService = new TestNavigationService();
                vm.SetNavigationService(navService);

                // Create a ContactItem with parameterless constructor, which leaves Email null
                var contactItem = new ContactItem();

                await ((AsyncRelayCommand<ContactItem>)vm.ComposeEmailCommand).ExecuteAsync(contactItem).ConfigureAwait(false);

                // Should not navigate when email is null
                Assert.That(navService.LastNavigatedPage, Is.Null);

                // Should report error
                Assert.That(errors.Errors.Count, Is.EqualTo(1));
                Assert.That(errors.Errors[0], Is.TypeOf<InvalidOperationException>());
                Assert.That(errors.Errors[0].Message, Is.EqualTo("Selected contact does not have an email address."));
            }
        }

        private sealed class ThrowingDispatcherService : Tuvi.App.ViewModels.Services.IDispatcherService
        {
            private readonly Exception _exception;

            public ThrowingDispatcherService(Exception exception)
            {
                _exception = exception ?? throw new ArgumentNullException(nameof(exception));
            }

            public Task RunAsync(Action action)
            {
                _ = action;
                throw _exception;
            }
        }
    }
}
