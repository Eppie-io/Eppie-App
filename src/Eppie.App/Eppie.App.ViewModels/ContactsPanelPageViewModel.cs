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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tuvi.App.ViewModels.Messages;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ContactsPanelPageViewModel : BaseViewModel, IControlModel, IIncrementalSource<ContactItem>, IDisposable
    {
        public ManagedCollection<ContactItem> Contacts { get; private set; }

        private const int ReconcileDelayMs = 200;

        private readonly ISearchFilter<ContactItem> _searchFilter = new SearchContactFilter();
        private CancellationTokenSource _debounceCts;
        private Contact _lastContact;
        private Func<Task<byte[]>> _avatarProvider;

        private ContactsSortOrder _sortOrder;
        public ContactsSortOrder SortOrder
        {
            get => _sortOrder;
            set
            {
                if (SetProperty(ref _sortOrder, value))
                {
                    RefreshContacts();
                }
            }
        }

        public ICommand ContactClickCommand => new RelayCommand<ContactItem>(OnContactClick);
        public ICommand RenameContactCommand => new AsyncRelayCommand<ContactItem>(RenameContactAsync);
        public ICommand ChangeContactAvatarCommand => new AsyncRelayCommand<ContactItem>(ChangeContactAvatarAsync);
        public ICommand RemoveContactCommand => new AsyncRelayCommand<ContactItem>(RemoveContactAsync);

        private ContactItem _selectedContact;
        public ContactItem SelectedContact
        {
            get { return _selectedContact; }
            set { SetProperty(ref _selectedContact, value); }
        }

        public ContactSortOrderItem[] SortingVariants { get; private set; }

        private int _selectedSortingIndex = -1;
        public int SelectedSortingIndex
        {
            get => _selectedSortingIndex;
            set
            {
                if (SetProperty(ref _selectedSortingIndex, value))
                {
                    OnSelectedSortingIndexChanged();
                }
            }
        }

        public ContactsPanelPageViewModel() { }

        public override void OnNavigatedTo(object data)
        {
            base.OnNavigatedTo(data);

            if (SortingVariants == null)
            {
                SortingVariants = new ContactSortOrderItem[]
                {
                    new ContactSortOrderItem(ContactsSortOrder.ByTime, GetLocalizedString("OrderByTimeContactComparerLabel")),
                    new ContactSortOrderItem(ContactsSortOrder.ByName, GetLocalizedString("OrderByNameContactComparerLabel")),
                    new ContactSortOrderItem(ContactsSortOrder.ByUnread, GetLocalizedString("OrderByUnreadContactComparerLabel")),
                };
            }

            RestoreSelectedSortingIndex();
            SubscribeToCoreEvents();
            RegisterMessagesListening();
        }

        public override void OnNavigatedFrom()
        {
            UnregisterMessagesListening();
            UnsubscribeFromCoreEvents();

            base.OnNavigatedFrom();
        }

        private void OnContactClick(ContactItem contactItem)
        {
            if (contactItem is null)
            {
                throw new ArgumentNullException(nameof(contactItem));
            }

            WeakReferenceMessenger.Default.Send(new ContactSelectedMessage(contactItem));
        }

        private async Task RenameContactAsync(ContactItem contactItem)
        {
            if (contactItem is null)
            {
                throw new ArgumentNullException(nameof(contactItem));
            }

            await Core.SetContactNameAsync(contactItem.Email, contactItem.FullName).ConfigureAwait(true);
        }

        private async Task ChangeContactAvatarAsync(ContactItem contactItem)
        {
            if (contactItem is null)
            {
                throw new ArgumentNullException(nameof(contactItem));
            }

            if (_avatarProvider != null)
            {
                var avatarBytes = await _avatarProvider().ConfigureAwait(true);
                if (avatarBytes != null)
                {
                    await Core.SetContactAvatarAsync(contactItem.Email, avatarBytes, ContactItem.DefaultAvatarSize, ContactItem.DefaultAvatarSize).ConfigureAwait(true);
                }
            }
        }

        private Task RemoveContactAsync(ContactItem contactItem)
        {
            if (contactItem is null)
            {
                throw new ArgumentNullException(nameof(contactItem));
            }

            return Core.RemoveContactAsync(contactItem.Email);
        }

        public void SetAvatarProvider(Func<Task<byte[]>> avatarProvider)
        {
            if (avatarProvider is null)
            {
                throw new ArgumentNullException(nameof(avatarProvider));
            }

            _avatarProvider = avatarProvider;
        }

        public void SetContactsCollection(ManagedCollection<ContactItem> collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            Contacts = collection;
            Contacts.SearchFilter = _searchFilter;
        }

        private void AddContact()
        {
            TriggerReconcile();
        }

        private void UpdateContact(ContactItem contactItem)
        {
            if (contactItem is null)
            {
                throw new ArgumentNullException(nameof(contactItem));
            }

            var existing = Contacts.OriginalItems.FirstOrDefault(x => x != null && x.Email == contactItem.Email);
            if (existing != null)
            {
                existing.UpdateFrom(contactItem);
                Contacts.RefilterItem(existing);
            }

            TriggerReconcile();
        }

        private void RemoveContactByEmail(EmailAddress contactEmail)
        {
            TriggerReconcile();
        }

        private void SubscribeToCoreEvents()
        {
            Core.ContactAdded += OnContactAdded;
            Core.ContactChanged += OnContactChanged;
            Core.ContactDeleted += OnContactDeleted;
            Core.MessageDeleted += OnMessageDeleted;
            Core.MessagesIsReadChanged += OnMessagesIsReadChanged;
            Core.UnreadMessagesReceived += OnUnreadMessagesReceived;
        }

        private void UnsubscribeFromCoreEvents()
        {
            Core.ContactAdded -= OnContactAdded;
            Core.ContactChanged -= OnContactChanged;
            Core.ContactDeleted -= OnContactDeleted;
            Core.MessageDeleted -= OnMessageDeleted;
            Core.MessagesIsReadChanged -= OnMessagesIsReadChanged;
            Core.UnreadMessagesReceived -= OnUnreadMessagesReceived;
        }

        private void OnMessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            _ = UpdateUnreadCountAsync();
        }

        private void OnMessagesIsReadChanged(object sender, MessagesAttributeChangedEventArgs e)
        {
            _ = UpdateUnreadCountAsync();
        }

        private void OnUnreadMessagesReceived(object sender, UnreadMessagesReceivedEventArgs e)
        {
            _ = UpdateUnreadCountAsync();
        }

        private async void OnContactAdded(object sender, ContactAddedEventArgs e)
        {
            try
            {
                await DispatcherService.RunAsync(() =>
                {
                    AddContact();
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async void OnContactChanged(object sender, ContactChangedEventArgs e)
        {
            try
            {
                await DispatcherService.RunAsync(() =>
                {
                    UpdateContact(new ContactItem(e.Contact));
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async void OnContactDeleted(object sender, ContactDeletedEventArgs e)
        {
            try
            {
                await DispatcherService.RunAsync(() =>
                {
                    RemoveContactByEmail(e.ContactEmail);
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void SetUnreadCount(IReadOnlyDictionary<EmailAddress, int> counts)
        {
            if (counts is null)
            {
                throw new ArgumentNullException(nameof(counts));
            }

            foreach (var contact in Contacts.OriginalItems)
            {
                int newCount = counts.TryGetValue(contact.Email, out int unreadCount) ? unreadCount : 0;
                if (contact.UnreadMessagesCount != newCount)
                {
                    contact.UnreadMessagesCount = newCount;
                }
            }

            TriggerReconcile();
        }

        private async Task UpdateUnreadCountAsync()
        {
            var counts = await Core.GetUnreadMessagesCountByContactAsync().ConfigureAwait(true);
            await DispatcherService.RunAsync(() =>
            {
                SetUnreadCount(counts);
            }).ConfigureAwait(true);
        }

        private void TriggerReconcile()
        {
            var newCts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _debounceCts, newCts);
            oldCts?.Cancel();
            oldCts?.Dispose();

            _ = ReconcileAfterDelayAsync(newCts.Token);
        }

        private async Task ReconcileAfterDelayAsync(CancellationToken ct)
        {
            try
            {
                await Task.Delay(ReconcileDelayMs, ct);

                int countToLoad = Contacts.OriginalItems.Count;
                var freshItems = await ReloadLoadedItemsAsync(countToLoad, ct);

                if (!ct.IsCancellationRequested)
                {
                    Contacts.ReconcileOriginalItems(freshItems, (oldItem, newItem) => oldItem.UpdateFrom(newItem));
                }
            }
            catch (OperationCanceledException) { }
        }

        private void OnSelectedSortingIndexChanged()
        {
            var item = SortingVariants?.ElementAtOrDefault(SelectedSortingIndex);

            if (item != null)
            {
                LocalSettingsService.ContactsSortOrder = item.SortOrder;
                SortOrder = item.SortOrder;
            }
        }

        private void RestoreSelectedSortingIndex()
        {
            if (SortingVariants is null || SortingVariants.Length == 0)
            {
                return;
            }

            var restored = LocalSettingsService.ContactsSortOrder;

            var idx = Array.FindIndex(SortingVariants, v => v != null && v.SortOrder == restored);
            if (idx >= 0)
            {
                SelectedSortingIndex = idx;
                return;
            }

            var defaultIdx = Array.FindIndex(SortingVariants, v => v != null && v.SortOrder == ContactsSortOrder.ByTime);
            if (defaultIdx >= 0)
            {
                SelectedSortingIndex = defaultIdx;
            }
        }

        private void RefreshContacts()
        {
            SelectedContact = null;
            _ = Contacts.RefreshAsync();
        }

        public async Task<IEnumerable<ContactItem>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
        {
            var contacts = await Core.GetContactsAsync(count, _lastContact, SortOrder, cancellationToken).ConfigureAwait(true);

            if (contacts.Count > 0)
            {
                _lastContact = contacts[contacts.Count - 1];
            }

            return contacts.Select(contact => new ContactItem(contact));
        }

        public async Task<IEnumerable<ContactItem>> ReloadLoadedItemsAsync(int count, CancellationToken cancellationToken)
        {
            var contacts = await Core.GetContactsAsync(count, null, SortOrder, cancellationToken).ConfigureAwait(true);

            _lastContact = contacts.Count > 0 ? contacts[contacts.Count - 1] : null;

            return contacts.Select(contact => new ContactItem(contact));
        }

        public void Reset()
        {
            _lastContact = null;
        }

        private void RegisterMessagesListening()
        {
            WeakReferenceMessenger.Default.Register<RequestSelectedContactMessage>(this, (r, m) =>
            {
                m.Reply(SelectedContact);
            });

            WeakReferenceMessenger.Default.Register<ClearSelectedContactMessage>(this, (r, m) =>
            {
                SelectedContact = null;
            });
        }


        private void UnregisterMessagesListening()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        public void Dispose()
        {
            UnregisterMessagesListening();
            UnsubscribeFromCoreEvents();

            _debounceCts?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
