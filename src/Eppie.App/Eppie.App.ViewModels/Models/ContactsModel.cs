using System.Collections.Generic;
using System.Windows.Input;
using Tuvi.Core.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuvi.App.ViewModels
{
    public class ContactsModel : ObservableObject, IControlModel
    {
        private ManagedCollection<ContactItem> _contacts = new ManagedCollection<ContactItem>();
        private Dictionary<EmailAddress, ContactItem> _searchIndex = new Dictionary<EmailAddress, ContactItem>();
        public ManagedCollection<ContactItem> Contacts
        {
            get { return _contacts; }
            set
            {
                SetProperty(ref _contacts, value);
                _searchIndex.Clear();
                foreach (var c in _contacts)
                {
                    _searchIndex.Add(c.Email, c);
                }
            }
        }

        private ICommand _contactClickCommand;
        public ICommand ContactClickCommand
        {
            get { return _contactClickCommand; }
            set { SetProperty(ref _contactClickCommand, value); }
        }

        private ICommand _changeContactAvatarCommand;
        public ICommand ChangeContactAvatarCommand
        {
            get { return _changeContactAvatarCommand; }
            set { SetProperty(ref _changeContactAvatarCommand, value); }
        }

        private ICommand _removeContactCommand;
        public ICommand RemoveContactCommand
        {
            get { return _removeContactCommand; }
            set { SetProperty(ref _removeContactCommand, value); }
        }

        private ContactItem _selectedContact;
        public ContactItem SelectedContact
        {
            get { return _selectedContact; }
            set { SetProperty(ref _selectedContact, value); }
        }

        public void SetContacts(IEnumerable<ContactItem> contacts, bool preserveSelection = true)
        {
            EmailAddress selectedContactEmail = SelectedContact?.Email;

            Contacts.Clear();
            Contacts.AddRange(contacts);
            _searchIndex.Clear();
            foreach (var c in Contacts.OriginalItems)
            {
                _searchIndex.Add(c.Email, c);
            }

            SelectedContact = preserveSelection
                            ? GetContactByEmail(selectedContactEmail)
                            : null;
        }

        public void AddContact(ContactItem contactItem)
        {
            Contacts.Add(contactItem);
            _searchIndex.Add(contactItem.Email, contactItem);
        }

        public ContactItem GetContactByEmail(EmailAddress contactEmail)
        {
            if (contactEmail != null && _searchIndex.TryGetValue(contactEmail, out ContactItem item))
            {
                return item;
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
            _searchIndex[contactItem.Email] = contactItem;
        }

        public void RemoveContactByEmail(EmailAddress contactEmail)
        {
            var contactItem = GetContactByEmail(contactEmail);
            if (contactItem != null)
            {
                Contacts.Remove(contactItem);
                _searchIndex.Remove(contactEmail);
            }
        }

        public void SetUnreadCount(EmailAddress contactEmail, int unreadCount)
        {
            var contactItem = GetContactByEmail(contactEmail);
            if (contactItem != null)
            {
                contactItem.UnreadMessagesCount = unreadCount;
            }
        }
    }
}
