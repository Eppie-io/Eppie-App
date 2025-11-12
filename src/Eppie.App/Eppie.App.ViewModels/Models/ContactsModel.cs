// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
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
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ContactsModel : ObservableObject, IControlModel
    {
        public ManagedCollection<ContactItem> Contacts { get; private set; }

        private ICommand _contactClickCommand;
        public ICommand ContactClickCommand
        {
            get { return _contactClickCommand; }
            private set { SetProperty(ref _contactClickCommand, value); }
        }

        private ICommand _renameContactCommand;
        public ICommand RenameContactCommand
        {
            get { return _renameContactCommand; }
            private set { SetProperty(ref _renameContactCommand, value); }
        }

        private ICommand _changeContactAvatarCommand;
        public ICommand ChangeContactAvatarCommand
        {
            get { return _changeContactAvatarCommand; }
            private set { SetProperty(ref _changeContactAvatarCommand, value); }
        }

        private ICommand _removeContactCommand;
        public ICommand RemoveContactCommand
        {
            get { return _removeContactCommand; }
            private set { SetProperty(ref _removeContactCommand, value); }
        }

        private ContactItem _selectedContact;
        public ContactItem SelectedContact
        {
            get { return _selectedContact; }
            set { SetProperty(ref _selectedContact, value); }
        }

        private ILocalSettingsService LocalSettingsService { get; set; }

        public ContactsModel(
            ILocalSettingsService localSettingsService,
            IExtendedComparer<ContactItem>[] sortingVariants,
            IExtendedComparer<ContactItem> defaultComparer,
            ISearchFilter<ContactItem> searchFilter,
            ICommand contactClickCommand,
            ICommand removeContactCommand,
            ICommand changeContactAvatarCommand,
            ICommand renameContactCommand)
        {
            LocalSettingsService = localSettingsService;
            ContactClickCommand = contactClickCommand;
            RemoveContactCommand = removeContactCommand;
            ChangeContactAvatarCommand = changeContactAvatarCommand;
            RenameContactCommand = renameContactCommand;

            Contacts = new ManagedCollection<ContactItem>
            {
                FilterVariants = Array.Empty<IFilter<ContactItem>>(),
                SortingVariants = sortingVariants,
                ItemsFilter = null,
                ItemsComparer = defaultComparer,
                SearchFilter = searchFilter
            };
            (Contacts as INotifyPropertyChanged).PropertyChanged += OnContactsPropertyChanged;

            var name = LocalSettingsService?.SelectedContactsSortingComparer;
            var variants = Contacts.SortingVariants;
            if (!string.IsNullOrEmpty(name) && variants != null)
            {
                var idx = Array.FindIndex(variants, v => v?.GetType().Name == name);
                if (idx >= 0)
                {
                    Contacts.SelectedSortingIndex = idx;
                }
            }
        }

        public void SetContacts(IEnumerable<ContactItem> contacts, bool preserveSelection = true)
        {
            EmailAddress selectedContactEmail = SelectedContact?.Email;

            Contacts.StartChanging();
            try
            {
                Contacts.Clear();
                Contacts.AddRange(contacts);
            }
            finally
            {
                Contacts.EndChanging();
            }

            SelectedContact = preserveSelection
                            ? GetContactByEmail(selectedContactEmail)
                            : null;
        }

        public void AddContact(ContactItem contactItem)
        {
            Contacts.Add(contactItem);
        }

        public ContactItem GetContactByEmail(EmailAddress contactEmail)
        {
            if (contactEmail != null)
            {
                return Contacts.FirstOrDefault(contact => contact.Email == contactEmail);
            }
            return null;
        }

        public void UpdateContact(ContactItem contactItem)
        {
            var oldContact = GetContactByEmail(contactItem?.Email);
            if (oldContact == null)
            {
                return;
            }
            int index = Contacts.IndexOf(oldContact);
            if (index == -1)
            {
                return;
            }
            Contacts[index] = contactItem;
        }

        public void RemoveContactByEmail(EmailAddress contactEmail)
        {
            var contactItem = GetContactByEmail(contactEmail);
            if (contactItem != null)
            {
                Contacts.Remove(contactItem);
            }
        }

        public void SetUnreadCount(IReadOnlyDictionary<EmailAddress, int> counts)
        {
            if (counts is null)
            {
                throw new ArgumentException("Parameter can not be null", nameof(counts));
            }

            foreach (var contact in Contacts)
            {
                contact.UnreadMessagesCount = counts.TryGetValue(contact.Email, out int unreadCount) ? unreadCount : 0;
            }
        }

        private void OnContactsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ManagedCollection<ContactItem>.SelectedSortingIndex))
            {
                if (LocalSettingsService != null)
                {
                    var comparer = Contacts.SortingVariants.ElementAtOrDefault(Contacts.SelectedSortingIndex);
                    if (comparer != null)
                    {
                        LocalSettingsService.SelectedContactsSortingComparer = comparer.GetType().Name;
                    }
                }
            }
        }
    }
}
