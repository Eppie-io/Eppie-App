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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class InvitationPageViewModel : BaseViewModel
    {
        public ObservableCollection<ContactItem> Recipients { get; } = new ObservableCollection<ContactItem>();
        public ManagedCollection<ContactItem> SuitableContacts { get; } = new ManagedCollection<ContactItem>();
        public ObservableCollection<AddressItem> SenderAddresses { get; } = new ObservableCollection<AddressItem>();
        public ObservableCollection<AddressItem> EppieAddresses { get; } = new ObservableCollection<AddressItem>();

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
                }
            }
        }

        public bool IsAnyRecipient => Recipients.Count > 0;
        public bool CanInvite => IsAnyRecipient && SenderAddressIndex != -1 && EppieAddressIndex != -1;

        public IRelayCommand SendInviteCommand { get; }
        public Action ClosePopupAction { get; set; }

        public InvitationPageViewModel() : base()
        {
            SendInviteCommand = new RelayCommand(SendInvite, () => CanInvite);
            SenderAddressIndex = -1;
            EppieAddressIndex = -1;

            SuitableContacts.SearchFilter = _contactsSearchFilter;
            _contactsSearchFilter.IncludeItemsOnEmptySearch = false;
            _contactsSearchFilter.SearchText = string.Empty;
        }

        public override void OnNavigatedTo(object data)
        {
            base.OnNavigatedTo(data);

            LoadAddresses();
            LoadContacts();
        }

        private async void LoadAddresses()
        {
            try
            {
                await LoadAddressesAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task LoadAddressesAsync()
        {
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

            SenderAddressIndex = SenderAddresses.Count > 0 ? 0 : -1;
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

        private void SendInvite()
        {
            var sender = SenderAddresses[SenderAddressIndex].Account.Email;
            var senderName = SenderAddresses[SenderAddressIndex].DisplayName;
            var eppieAddressString = EppieAddresses[EppieAddressIndex].Account.Email.DisplayAddress;
            var downloadLink = BrandService.GetHomepage();
            var subject = string.Format(System.Globalization.CultureInfo.InvariantCulture, GetLocalizedString("InvitationSubjectText"), senderName);
            var body = string.Format(System.Globalization.CultureInfo.InvariantCulture, GetLocalizedString("InvitationBodyText"), senderName, eppieAddressString, downloadLink);

            _ = SendEmailsAsync(Core, sender, Recipients.ToList(), subject, body);

            ClosePopupAction?.Invoke();
        }

        private static async Task SendEmailsAsync(Core.ITuviMail core, EmailAddress sender, List<ContactItem> recipients, string subject, string body)
        {
            foreach (var recipient in recipients)
            {
                if (recipient.Email != null)
                {
                    var message = new Message();
                    message.From.Add(sender);
                    message.To.Add(recipient.Email);
                    message.Subject = subject;
                    message.TextBody = body;

                    await core.SendMessageAsync(message, false, false).ConfigureAwait(false);
                }
            }
        }

        public void OnRecipientRemoved(ContactItem item)
        {
            Recipients.Remove(item);

            OnPropertyChanged(nameof(IsAnyRecipient));
            OnPropertyChanged(nameof(CanInvite));
            SendInviteCommand.NotifyCanExecuteChanged();
        }
    }
}
