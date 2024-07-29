using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ContactsModel : ObservableObject, IControlModel
    {
        private ManagedCollection<ContactItem> _contacts = new ManagedCollection<ContactItem>();

        public ManagedCollection<ContactItem> Contacts
        {
            get { return _contacts; }
            set
            {
                SetProperty(ref _contacts, value);
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
