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

using System.Collections.Specialized;
using System.ComponentModel;

using Eppie.App.ViewModels.Tests.TestDoubles;
using NUnit.Framework;
using Tuvi.App.ViewModels;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public sealed class InvitationPageViewModelTests : IDisposable
    {
        private static readonly TimeSpan DefaultAsyncTimeout = TimeSpan.FromSeconds(5);

        // Constants for test strings
        private const string InvitationSubjectText = "InvitationSubjectText";
        private const string InvitationBodyText = "InvitationBodyText";

        private InvitationPageViewModel _vm = null!;
        private FakeTuviMail _core = null!;
        private TestDispatcherService _dispatcherService = null!;
        private TestLocalizationService _localizationService = null!;
        private TestBrandService _brandService = null!;
        private RecorderErrorHandler _errorHandler = null!;

        [SetUp]
        public void SetUp()
        {
            _core = new FakeTuviMail();
            _dispatcherService = new TestDispatcherService();
            _localizationService = new TestLocalizationService();
            _brandService = new TestBrandService();
            _errorHandler = new RecorderErrorHandler();

            _vm = new InvitationPageViewModel();
            _vm.SetCoreProvider(() => _core);
            _vm.SetDispatcherService(_dispatcherService);
            _vm.SetLocalizationService(_localizationService);
            _vm.SetBrandService(_brandService);
            _vm.SetErrorHandler(_errorHandler);
        }

        // Helper factories to reduce duplication and improve null-safety
        private static Account CreateAccount(int id, string address, string display, MailBoxType type = MailBoxType.Email)
        {
            return new Account { Id = id, Email = new EmailAddress(address, display), Type = type };
        }

        private static ContactItem CreateContact(string address, string display)
        {
            return new ContactItem(new EmailAddress(address, display));
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        private void SeedAccounts(params Account[] accounts)
        {
            _core.SeedAccounts(accounts);
        }

        private async Task WaitForSuitableContactsAsyncViaEvent(int expectedCount)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            NotifyCollectionChangedEventHandler handler = (s, e) => TrySet();

            _vm.SuitableContacts.CollectionChanged += handler;

            void TrySet()
            {
                if (_vm.SuitableContacts.OriginalItems.Count == expectedCount)
                {
                    tcs.TrySetResult(true);
                }
            }

            TrySet();
            if (tcs.Task.IsCompleted)
            {
                _vm.SuitableContacts.CollectionChanged -= handler;
                return;
            }

            try
            {
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(DefaultAsyncTimeout)).ConfigureAwait(false);
                if (completed != tcs.Task)
                {
                    Assert.Fail($"SuitableContacts did not reach expected count {expectedCount} within {DefaultAsyncTimeout.TotalSeconds:0.##}s. Current: {_vm.SuitableContacts.OriginalItems.Count}");
                }
            }
            finally
            {
                _vm.SuitableContacts.CollectionChanged -= handler;
            }
        }

        private async Task WaitForErrorCountAsync(int expectedCount)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void TrySet()
            {
                if (_errorHandler.Errors.Count >= expectedCount)
                {
                    tcs.TrySetResult(true);
                }
            }

            EventHandler<Exception>? handler = (s, ex) => TrySet();
            _errorHandler.ErrorRecorded += handler;

            TrySet();
            if (tcs.Task.IsCompleted)
            {
                _errorHandler.ErrorRecorded -= handler;
                return;
            }

            try
            {
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(DefaultAsyncTimeout)).ConfigureAwait(false);
                if (completed != tcs.Task)
                {
                    Assert.Fail($"Expected {expectedCount} recorded errors within {DefaultAsyncTimeout.TotalSeconds:0.##}s. Current: {_errorHandler.Errors.Count}");
                }
            }
            finally
            {
                _errorHandler.ErrorRecorded -= handler;
            }
        }

        private async Task WaitForAddressesAsync(bool requireSender = true, bool requireEppie = true)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void TrySet()
            {
                var senderReady = !requireSender || (_vm.SenderAddresses.Count > 0 && _vm.SenderAddressIndex != -1);
                var eppieReady = !requireEppie || (_vm.EppieAddresses.Count > 0 && _vm.EppieAddressIndex != -1);

                if (senderReady && eppieReady)
                {
                    tcs.TrySetResult(true);
                }
            }

            // If condition already met, return immediately
            TrySet();
            if (tcs.Task.IsCompleted) return;

            NotifyCollectionChangedEventHandler? senderHandler = null;
            NotifyCollectionChangedEventHandler? eppieHandler = null;
            PropertyChangedEventHandler? propHandler = null;

            senderHandler = (s, e) => TrySet();
            eppieHandler = (s, e) => TrySet();
            propHandler = (s, e) => { if (e.PropertyName == nameof(_vm.SenderAddressIndex) || e.PropertyName == nameof(_vm.EppieAddressIndex)) TrySet(); };

            _vm.SenderAddresses.CollectionChanged += senderHandler;
            _vm.EppieAddresses.CollectionChanged += eppieHandler;
            _vm.PropertyChanged += propHandler;

            try
            {
                var delay = Task.Delay(DefaultAsyncTimeout);
                var completed = await Task.WhenAny(tcs.Task, delay).ConfigureAwait(false);
                if (completed != tcs.Task)
                {
                    Assert.Fail($"Addresses were not ready within {DefaultAsyncTimeout.TotalSeconds:0.##}s");
                }
            }
            finally
            {
                _vm.SenderAddresses.CollectionChanged -= senderHandler;
                _vm.EppieAddresses.CollectionChanged -= eppieHandler;
                _vm.PropertyChanged -= propHandler;
            }
        }

        private async Task WaitForSentMessagesAsync(int expectedCount)
        {
            if (_core is null)
            {
                throw new InvalidOperationException("Core not initialized");
            }
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler? handler = null;
            handler = (s, e) =>
            {
                if (_core.SentMessages.Count == expectedCount)
                {
                    tcs.TrySetResult(true);
                }
            };

            _core.MessageSent += handler;
            try
            {
                // Already satisfied?
                if (_core.SentMessages.Count == expectedCount)
                {
                    tcs.TrySetResult(true);
                }

                var delayTask = Task.Delay(DefaultAsyncTimeout);
                var completed = await Task.WhenAny(tcs.Task, delayTask).ConfigureAwait(false);
                if (completed != tcs.Task)
                {
                    Assert.Fail($"Expected {expectedCount} sent messages within {DefaultAsyncTimeout.TotalSeconds:0.##}s");
                }
            }
            finally
            {
                _core.MessageSent -= handler;
            }
        }

        // Use shared FakeTuviMail from TestDoubles for accounts and sent messages

        // Use TestBrandService and RecorderErrorHandler from TestDoubles folder

        [Test]
        public async Task SendInviteSendsMessagesAndClosesPopup()
        {
            var senderAcc = CreateAccount(1, "from@site.com", "Sender");
            var eppieAcc = CreateAccount(2, "eppie@network", "Eppie", MailBoxType.Dec);

            SeedAccounts(senderAcc, eppieAcc);

            bool closed = false;
            _vm.ClosePopupAction = () => closed = true;

            _vm.OnNavigatedTo(null);

            await WaitForAddressesAsync().ConfigureAwait(false);

            Assert.That(_vm.CanInvite, Is.False, "CanInvite should be false before selecting recipient and addresses");

            var contact = CreateContact("to@recipient.test", "Recipient");
            _vm.OnContactQuerySubmitted(contact, null);

            Assert.That(_vm.IsAnyRecipient, Is.True, "IsAnyRecipient should be true after adding a recipient");
            Assert.That(_vm.CanInvite, Is.True, "CanInvite should be true after recipient and addresses are set");

            _vm.SendInviteCommand.Execute(null);

            await WaitForSentMessagesAsync(1).ConfigureAwait(false);

            var sent = _core.SentMessages.First();

            // Validate From contains sender address
            // Prefer declarative collection/string constraints instead of predicate Any(...)=true
            Assert.That(sent.From.Select(f => f.Address), Does.Contain(senderAcc.Email.Address).IgnoreCase, "Sent message should contain sender address in From");
            Assert.That(sent.To.Select(t => t.Address), Does.Contain(contact.Email.Address).IgnoreCase, "Sent message should contain recipient address in To");
            Assert.That(sent.Subject, Is.EqualTo(InvitationSubjectText), "Sent message subject should match localized invitation subject");
            Assert.That(sent.TextBody, Is.EqualTo(InvitationBodyText), "Sent message body should match localized invitation body");
            Assert.That(closed, Is.True, "ClosePopupAction should be invoked after sending invites");
        }

        [Test]
        public async Task SendInviteSendsOneMessagePerDistinctRecipient()
        {
            var senderAcc = CreateAccount(1, "from@site.com", "Sender");
            var eppieAcc = CreateAccount(2, "eppie@network", "Eppie", MailBoxType.Dec);
            SeedAccounts(senderAcc, eppieAcc);

            _vm.OnNavigatedTo(null);
            await WaitForAddressesAsync().ConfigureAwait(false);

            var c1 = CreateContact("a@x.test", "A");
            var c2 = CreateContact("b@x.test", "B");
            var c1dup = CreateContact("a@x.test", "A Duplicate");

            _vm.OnContactQuerySubmitted(c1, null);
            _vm.OnContactQuerySubmitted(c2, null);
            _vm.OnContactQuerySubmitted(c1dup, null);

            Assert.That(_vm.Recipients, Has.Count.EqualTo(2));

            _vm.SendInviteCommand.Execute(null);

            await WaitForSentMessagesAsync(2).ConfigureAwait(false);

            var toAddresses = _core.SentMessages.Select(m => m.To.First().Address).OrderBy(a => a).ToList();
            Assert.That(toAddresses, Is.EqualTo(new[] { "a@x.test", "b@x.test" }));
        }

        [Test]
        public async Task InitializeFromContactSelectsRequestedSenderAccount()
        {
            var senderAcc1 = CreateAccount(10, "s1@local", "S1");
            var senderAcc2 = CreateAccount(11, "s2@local", "S2");
            var eppieAcc = CreateAccount(20, "e@dec", "Eppie", MailBoxType.Dec);

            SeedAccounts(senderAcc1, senderAcc2, eppieAcc);

            var contact = new ContactItem();
            contact.LastMessageData = new LastMessageData { AccountId = senderAcc2.Id };

            _vm.OnNavigatedTo(contact);

            await WaitForAddressesAsync().ConfigureAwait(false);

            Assert.That(_vm.SenderAddressIndex, Is.EqualTo(1));
            Assert.That(_vm.EppieAddressIndex, Is.Zero);
        }

        [Test]
        public async Task InitializeFromContactRequestedIdNotFoundSelectsFirstOrMinusOne()
        {
            var senderAcc = CreateAccount(1, "s@local", "S");
            SeedAccounts(senderAcc);

            var contact = new ContactItem();
            contact.LastMessageData = new LastMessageData { AccountId = 9999 };

            _vm.OnNavigatedTo(contact);

            await WaitForAddressesAsync(requireSender: true, requireEppie: false).ConfigureAwait(false);

            Assert.That(_vm.SenderAddressIndex, Is.Zero);
        }

        [Test]
        public void CanInviteFalseWhenMissingParts()
        {
            _vm.SenderAddressIndex = -1;
            _vm.EppieAddressIndex = -1;
            Assert.That(_vm.CanInvite, Is.False);

            _vm.Recipients.Add(new ContactItem(new EmailAddress("a@x.test", "A")));
            _vm.SenderAddressIndex = -1;
            _vm.EppieAddressIndex = -1;
            Assert.That(_vm.CanInvite, Is.False);

            _vm.SenderAddresses.Add(new AddressItem(new Account()));
            _vm.SenderAddressIndex = 0;
            Assert.That(_vm.CanInvite, Is.False);

            _vm.EppieAddresses.Add(new AddressItem(new Account()));
            _vm.EppieAddressIndex = 0;
            Assert.That(_vm.CanInvite, Is.True);
        }

        [Test]
        public void AddRecipientIgnoresInvalidOrDuplicate()
        {
            // null item
            _vm.OnContactQuerySubmitted(null, null);
            Assert.That(_vm.Recipients, Is.Empty);

            // invalid text
            _vm.OnContactQuerySubmitted(null, "");
            Assert.That(_vm.Recipients, Is.Empty);

            // valid
            var c1 = new ContactItem(new EmailAddress("dup@x.test", "D"));
            _vm.OnContactQuerySubmitted(c1, null);
            Assert.That(_vm.Recipients, Has.Count.EqualTo(1));

            // duplicate case-insensitive
            var cdup = new ContactItem(new EmailAddress("DUP@x.test", "Dup"));
            _vm.OnContactQuerySubmitted(cdup, null);
            Assert.That(_vm.Recipients, Has.Count.EqualTo(1));
        }

        [Test]
        public void OnRecipientRemovedUpdatesCanInvite()
        {
            _vm.Recipients.Add(new ContactItem(new EmailAddress("a@x.test", "A")));
            _vm.SenderAddresses.Add(new AddressItem(new Account()));
            _vm.EppieAddresses.Add(new AddressItem(new Account()));

            _vm.SenderAddressIndex = 0;
            _vm.EppieAddressIndex = 0;

            Assert.That(_vm.CanInvite, Is.True);

            _vm.OnRecipientRemoved(_vm.Recipients.First());

            Assert.That(_vm.CanInvite, Is.False);
        }

        [Test]
        public void OnContactQueryChangedUpdatesSearchFilterText()
        {
            _vm.OnContactQueryChanged("abc");

            var actualFilter = _vm.SuitableContacts.SearchFilter as SearchContactFilter;
            Assert.That(actualFilter?.SearchText, Is.EqualTo("abc"));
        }

        [Test]
        public async Task LoadContactsRemovesDuplicatesByAddress()
        {
            var c1 = new Contact("A", new EmailAddress("dup@x.test", "A"));
            var c2 = new Contact("B", new EmailAddress("dup@x.test", "B"));

            _core.SeedContacts(new[] { c1, c2 });

            _vm.OnNavigatedTo(null);

            await WaitForSuitableContactsAsyncViaEvent(1).ConfigureAwait(false);

            Assert.That(_vm.SuitableContacts.OriginalItems, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task ChangingSenderOrEppieIndexUpdatesCanExecute()
        {
            // No accounts -> indices are -1
            Assert.That(_vm.SendInviteCommand.CanExecute(null), Is.False);

            // Add recipient but still no addresses
            _vm.Recipients.Add(new ContactItem(new EmailAddress("r@x.test", "R")));
            Assert.That(_vm.SendInviteCommand.CanExecute(null), Is.False);

            // Seed accounts and set indices
            var senderAcc = CreateAccount(1, "from@site.com", "Sender");
            var eppieAcc = CreateAccount(2, "eppie@network", "Eppie", MailBoxType.Dec);
            SeedAccounts(senderAcc, eppieAcc);

            // simulate navigation initialization that populates addresses
            _vm.OnNavigatedTo(null);

            await WaitForAddressesAsync().ConfigureAwait(false);

            Assert.That(_vm.SendInviteCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void SendInviteCommandCannotExecuteWhenMissingParts()
        {
            // no recipients and no addresses
            Assert.That(_vm.SendInviteCommand.CanExecute(null), Is.False);

            _vm.Recipients.Add(new ContactItem(new EmailAddress("a@x.test", "A")));
            Assert.That(_vm.SendInviteCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void OnContactQuerySubmittedWithTextAddsParsedEmailRecipient()
        {
            _vm.OnContactQuerySubmitted(null, "Name <parsed@x.test>");
            Assert.That(_vm.Recipients, Has.Count.EqualTo(1));
            Assert.That(_vm.Recipients.First().Email.Address, Is.EqualTo("parsed@x.test"));
        }

        [Test]
        public async Task SendInviteIgnoresNullRecipientEntries()
        {
            var senderAcc = CreateAccount(1, "from@site.com", "Sender");
            var eppieAcc = CreateAccount(2, "eppie@network", "Eppie", MailBoxType.Dec);

            SeedAccounts(senderAcc, eppieAcc);

            _vm.OnNavigatedTo(null);
            await WaitForAddressesAsync().ConfigureAwait(false);

            var good = new ContactItem(new EmailAddress("g@x.test", "G"));
            var bad = new ContactItem(); // no email
            _vm.Recipients.Add(bad);
            _vm.Recipients.Add(good);

            _vm.SendInviteCommand.Execute(null);

            await WaitForSentMessagesAsync(1).ConfigureAwait(false);
            Assert.That(_core.SentMessages, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task SendInviteCreatesDecentralizedAccountWhenSelectedCreateItem()
        {
            var senderAcc = CreateAccount(1, "from@site.com", "Sender");
            SeedAccounts(senderAcc);

            _vm.OnNavigatedTo(null);
            await WaitForAddressesAsync(requireSender: true, requireEppie: true).ConfigureAwait(false);

            var contact = CreateContact("to@recipient.test", "Recipient");
            _vm.OnContactQuerySubmitted(contact, null);

            _vm.SendInviteCommand.Execute(null);

            await WaitForSentMessagesAsync(1).ConfigureAwait(false);

            Assert.That(_core.AddedAccounts, Has.Count.EqualTo(1));
            var created = _core.AddedAccounts.First();
            Assert.That(created.Type, Is.EqualTo(MailBoxType.Dec));
            Assert.That(created.Email.Network, Is.EqualTo(NetworkType.Eppie));
        }

        [Test]
        public void CreateAddressFromTextInvalidDoesNotAdd()
        {
            // Provide invalid text (no @) - EmailAddress.Parse may still create but test conservative behavior
            _vm.OnContactQuerySubmitted(null, "invalid-email");
            Assert.That(_vm.Recipients, Is.Empty);
        }

        [Test]
        public async Task LoadContactsReconcileOriginalItemsNoDuplicatesOnRepeatedLoad()
        {
            var c1 = new Contact("A", new EmailAddress("x1@x.test", "A"));
            _core.SeedContacts(new[] { c1 });

            _vm.OnNavigatedTo(null);
            await WaitForSuitableContactsAsyncViaEvent(1).ConfigureAwait(false);

            // Add new contact with same address
            var c2 = new Contact("A2", new EmailAddress("x1@x.test", "A2"));
            _core.SeedContacts(new[] { c1, c2 });

            _vm.OnContactQueryChanged(string.Empty); // trigger re-filter/load
            _vm.OnNavigatedTo(null);
            await WaitForSuitableContactsAsyncViaEvent(1).ConfigureAwait(false);
            Assert.That(_vm.SuitableContacts.OriginalItems, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task SendInviteHandlesSendFailure()
        {
            var senderAcc = new Account { Id = 1, Email = new EmailAddress("from@site.com", "Sender"), Type = MailBoxType.Email };
            var eppieAcc = new Account { Id = 2, Email = new EmailAddress("eppie@network", "Eppie"), Type = MailBoxType.Dec };

            SeedAccounts(senderAcc, eppieAcc);
            _core.ShouldThrowOnSend = true;

            bool closed = false;
            _vm.ClosePopupAction = () => closed = true;

            _vm.OnNavigatedTo(null);
            await WaitForAddressesAsync().ConfigureAwait(false);

            var contact = new ContactItem(new EmailAddress("to@recipient.test", "Recipient"));
            _vm.OnContactQuerySubmitted(contact, null);

            _vm.SendInviteCommand.Execute(null);

            await WaitForErrorCountAsync(1).ConfigureAwait(false);

            Assert.That(closed, Is.True);
            Assert.That(_errorHandler.Errors, Has.Count.EqualTo(1));
        }

        public void Dispose()
        {
            _core?.Dispose();
        }
    }
}
