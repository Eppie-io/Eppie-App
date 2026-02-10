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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Common;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{

    public class CreateEppieAddressItem
    { }

    public class InvitationPageViewModel : BaseViewModel
    {
        public ObservableCollection<ContactItem> Recipients { get; } = new ObservableCollection<ContactItem>();
        public ManagedCollection<ContactItem> SuitableContacts { get; } = new ManagedCollection<ContactItem>();
        public ObservableCollection<AddressItem> SenderAddresses { get; } = new ObservableCollection<AddressItem>();
        public ObservableCollection<object> EppieAddresses { get; } = new ObservableCollection<object>();

        private readonly ObservableCollection<ContactItem> _allContacts = new ObservableCollection<ContactItem>();

        private readonly SearchContactFilter _contactsSearchFilter = new SearchContactFilter();
        private string _currentContactsQuery = string.Empty;

        private int _senderAddressIndex;
        public int SenderAddressIndex
        {
            get { return _senderAddressIndex; }
            set
            {
                if (SetProperty(ref _senderAddressIndex, value))
                {
                    OnPropertyChanged(nameof(CanInvite));
                    SendInviteCommand.NotifyCanExecuteChanged();
                    PreviewCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private int _eppieAddressIndex;
        public int EppieAddressIndex
        {
            get { return _eppieAddressIndex; }
            set
            {
                if (SetProperty(ref _eppieAddressIndex, value))
                {
                    OnPropertyChanged(nameof(CanInvite));
                    SendInviteCommand.NotifyCanExecuteChanged();
                    PreviewCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public bool IsAnyRecipient => Recipients.Count > 0;
        public bool CanInvite => IsAnyRecipient && SenderAddressIndex != -1 && EppieAddressIndex != -1;

        public IRelayCommand SendInviteCommand { get; }
        public IRelayCommand PreviewCommand { get; }
        public Action ClosePopupAction { get; set; }

        public InvitationPageViewModel() : base()
        {
            SendInviteCommand = new AsyncRelayCommand(SendInviteAsync, () => CanInvite);
            PreviewCommand = new AsyncRelayCommand(PreviewInviteAsync, () => CanInvite);

            SenderAddressIndex = -1;
            EppieAddressIndex = -1;

            SuitableContacts.SearchFilter = _contactsSearchFilter;
            _contactsSearchFilter.IncludeItemsOnEmptySearch = false;
            _contactsSearchFilter.SearchText = string.Empty;
        }

        public override void OnNavigatedTo(object data)
        {
            base.OnNavigatedTo(data);

            InitializeFromContact(data as ContactItem);
            LoadContacts();
        }

        private async void InitializeFromContact(ContactItem contactItem)
        {
            try
            {
                await InitializeFromContactAsync(contactItem).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task InitializeFromContactAsync(ContactItem contactItem)
        {
            if (contactItem != null)
            {
                AddRecipient(contactItem);
            }

            var accounts = await Core.GetAccountsAsync().ConfigureAwait(true);

            var senderAccounts = accounts.Where(a => a.Type != MailBoxType.Dec).ToList();
            var eppieAccounts = accounts.Where(a => a.Type == MailBoxType.Dec).ToList();

            SenderAddresses.Clear();
            foreach (var account in senderAccounts)
            {
                SenderAddresses.Add(new AddressItem(account));
            }

            EppieAddresses.Clear();
            foreach (var account in eppieAccounts)
            {
                EppieAddresses.Add(new AddressItem(account));
            }
            EppieAddresses.Add(new CreateEppieAddressItem());

            var requestedAccountId = contactItem?.LastMessageData?.AccountId;
            if (requestedAccountId.HasValue)
            {
                int requestedId = requestedAccountId.Value;
                SenderAddressIndex = senderAccounts.FindIndex(a => a != null && a.Id == requestedId);
                if (SenderAddressIndex < 0)
                {
                    SenderAddressIndex = SenderAddresses.Count > 0 ? 0 : -1;
                }
            }
            else
            {
                SenderAddressIndex = SenderAddresses.Count > 0 ? 0 : -1;
            }
            EppieAddressIndex = EppieAddresses.Count > 0 ? 0 : -1;
        }

        private void LoadContacts()
        {
            _allContacts.Clear();
            SuitableContacts.Clear();

            _ = LoadContactsInBackgroundAsync();
        }

        private async Task LoadContactsInBackgroundAsync()
        {
            var existingEmails = new HashSet<string>(
                _allContacts.Select(c => c?.Email?.Address ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);

            var contacts = await Core.GetContactsAsync().ConfigureAwait(true);

            var newItems = contacts
                .Where(contact => !existingEmails.Contains(contact.Email?.Address ?? string.Empty))
                .Select(contact => new ContactItem(contact))
                .ToList();

            foreach (var item in DistinctByAddress(newItems))
            {
                _allContacts.Add(item);
            }

            SuitableContacts.ReconcileOriginalItems(_allContacts);

            if (!string.IsNullOrWhiteSpace(_currentContactsQuery))
            {
                UpdateSuitableContacts(_currentContactsQuery);
            }
        }

        public void OnContactQuerySubmitted(ContactItem queryItem, string queryText)
        {
            if (queryItem != null)
            {
                AddRecipient(queryItem);
            }
            else if (!string.IsNullOrWhiteSpace(queryText))
            {
                AddRecipient(CreateAddressItemFromText(queryText));
            }

            OnPropertyChanged(nameof(IsAnyRecipient));
            OnPropertyChanged(nameof(CanInvite));
            SendInviteCommand.NotifyCanExecuteChanged();
            PreviewCommand.NotifyCanExecuteChanged();
        }

        public void OnContactQueryChanged(string queryText)
        {
            UpdateSuitableContacts(queryText);
        }

        private void UpdateSuitableContacts(string queryText)
        {
            var query = (queryText ?? string.Empty).Trim();

            _currentContactsQuery = query;
            _contactsSearchFilter.SearchText = query;
        }

        private void AddRecipient(ContactItem item)
        {
            if (item is null)
            {
                return;
            }

            string address = item.Email?.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                return;
            }

            bool exists = Recipients.Any(r => StringComparer.OrdinalIgnoreCase.Equals(r?.Email?.Address, address));
            if (!exists)
            {
                Recipients.Add(item);
            }

            OnPropertyChanged(nameof(IsAnyRecipient));
            OnPropertyChanged(nameof(CanInvite));
            SendInviteCommand.NotifyCanExecuteChanged();
            PreviewCommand.NotifyCanExecuteChanged();
        }

        private static ContactItem CreateAddressItemFromText(string queryText)
        {
            var email = EmailAddress.Parse(queryText);
            return new ContactItem(new Contact(string.Empty, email));
        }

        private static IEnumerable<ContactItem> DistinctByAddress(IEnumerable<ContactItem> items)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items ?? Enumerable.Empty<ContactItem>())
            {
                var address = item?.Email?.Address;
                if (string.IsNullOrWhiteSpace(address))
                {
                    continue;
                }

                if (seen.Add(address))
                {
                    yield return item;
                }
            }
        }

        private async Task SendInviteAsync()
        {
            var context = await BuildInviteMessageAsync().ConfigureAwait(true);
            if (context is null)
            {
                return;
            }

            _ = SendEmailsAsync(Core, context).ConfigureAwait(true);

            ClosePopupAction?.Invoke();
        }

        private static async Task SendEmailsAsync(Core.ITuviMail core, InviteMessageContext context)
        {
            foreach (var recipient in context.Recipients.Where(r => r != null && r.Email != null))
            {
                var message = new Message();
                message.From.Add(context.MessageData.From);
                message.To.Add(recipient.Email);
                message.Subject = context.MessageData.Subject;
                message.TextBody = context.MessageData.TextBody;

                await core.SendMessageAsync(message, false, false).ConfigureAwait(false);
            }
        }

        public void OnRecipientRemoved(ContactItem item)
        {
            Recipients.Remove(item);

            OnPropertyChanged(nameof(IsAnyRecipient));
            OnPropertyChanged(nameof(CanInvite));
            SendInviteCommand.NotifyCanExecuteChanged();
            PreviewCommand.NotifyCanExecuteChanged();
        }

        private async Task PreviewInviteAsync()
        {
            try
            {
                var context = await BuildInviteMessageAsync().ConfigureAwait(true);
                if (context is null)
                {
                    return;
                }

                NavigationService?.Navigate(nameof(NewMessagePageViewModel), context.MessageData);
                ClosePopupAction?.Invoke();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task<InviteMessageContext> BuildInviteMessageAsync()
        {
            if (!CanInvite)
            {
                return null;
            }

            string eppieAddressString;
            if (EppieAddresses[EppieAddressIndex] is AddressItem eppieAddressItem)
            {
                eppieAddressString = eppieAddressItem.Account.Email.DisplayAddress;
            }
            else
            {
                eppieAddressString = await CreateEppieAddressAsync().ConfigureAwait(true);
                if (string.IsNullOrWhiteSpace(eppieAddressString))
                {
                    return null;
                }
            }

            var senderAddress = SenderAddresses[SenderAddressIndex];
            var senderName = senderAddress.DisplayName;
            var downloadLink = BrandService.GetHomepage();
            var subject = string.Format(System.Globalization.CultureInfo.InvariantCulture, GetLocalizedString("InvitationSubjectText"), senderName);
            var body = string.Format(System.Globalization.CultureInfo.InvariantCulture, GetLocalizedString("InvitationBodyText"), senderName, eppieAddressString, downloadLink);

            var recipients = Recipients.ToList();
            var to = string.Join(", ", recipients
                .Where(recipient => recipient?.Email?.Address != null)
                .Select(recipient => recipient.Email.Address)
                .Distinct(StringComparer.OrdinalIgnoreCase));

            var messageData = new NewMessageData(senderAddress.Account.Email, to, string.Empty, string.Empty, subject, body);

            return new InviteMessageContext(messageData, recipients);
        }

        private async Task<string> CreateEppieAddressAsync()
        {
            try
            {
                var account = await CreateDecentralizedAccountAsync(NetworkType.Eppie, CancellationToken.None)
                    .ConfigureAwait(true);

                await Core.AddAccountAsync(account, CancellationToken.None).ConfigureAwait(true);
                _ = BackupIfNeededAsync();

                return account.Email.DisplayAddress;
            }
            catch (Exception ex)
            {
                OnError(ex);
                return string.Empty;
            }
        }

        private sealed class InviteMessageContext
        {
            public InviteMessageContext(NewMessageData messageData, List<ContactItem> recipients)
            {
                MessageData = messageData;
                Recipients = recipients;
            }

            public NewMessageData MessageData { get; }
            public List<ContactItem> Recipients { get; }
        }
    }
}
