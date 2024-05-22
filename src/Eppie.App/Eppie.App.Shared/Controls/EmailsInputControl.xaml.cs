using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EmailValidation;
using Tuvi.Core.Entities;
using Tuvi.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class EmailsInputControl : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(EmailsInputControl), null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ObservableCollection<ContactItem> SelectedContacts
        {
            get { return (ObservableCollection<ContactItem>)GetValue(SelectedContactsProperty); }
            set { SetValue(SelectedContactsProperty, value); }
        }
        public static readonly DependencyProperty SelectedContactsProperty =
            DependencyProperty.Register(nameof(SelectedContacts), typeof(object), typeof(EmailsInputControl), new PropertyMetadata(new ObservableCollection<ContactItem>()));

        public ContactItem UntokenizedContact
        {
            get { return (ContactItem)GetValue(UntokenizedContactProperty); }
            set { SetValue(UntokenizedContactProperty, value); }
        }
        public static readonly DependencyProperty UntokenizedContactProperty =
            DependencyProperty.Register(nameof(UntokenizedContact), typeof(ContactItem), typeof(EmailsInputControl), new PropertyMetadata(null));


        // TODO: TVM-373
#if !NETFX_CORE
        public string SelectedContactsText
        {
            get { return (string)GetValue(SelectedContactsTextProperty); }
            set { SetValue(SelectedContactsTextProperty, value); }
        }
        public static readonly DependencyProperty SelectedContactsTextProperty =
            DependencyProperty.Register(nameof(SelectedContactsText), typeof(string), typeof(EmailsInputControl), new PropertyMetadata(string.Empty, OnSelectedContactsTextPropertyChanged));

        private static void OnSelectedContactsTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmailsInputControl control)
            {
                string[] emails = (e.NewValue as string).Split(new char[] { ',', ';' });

                control.SelectedContacts.Clear();
                foreach (var email in emails.Select(em => em.Trim()))
                {
                    var contact = control.Contacts.Where(c => c.Email.Address.Equals(email, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (contact == null && EmailValidator.Validate(email, allowTopLevelDomains: true))
                    {
                        contact = new ContactItem() { Email = new EmailAddress(email) };
                    }

                    if (contact != null)
                    {
                        control.SelectedContacts.Add(contact);
                    }
                }
            }
        }
#endif

        public ObservableCollection<ContactItem> Contacts
        {
            get { return (ObservableCollection<ContactItem>)GetValue(ContactsProperty); }
            set { SetValue(ContactsProperty, value); }
        }
        public static readonly DependencyProperty ContactsProperty =
            DependencyProperty.Register(nameof(Contacts), typeof(object), typeof(EmailsInputControl), new PropertyMetadata(new ObservableCollection<ContactItem>()));


        private void SuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var text = sender.Text.Trim();

            if (args.CheckCurrent() && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = GetSuggestedItems(text);
            }

            UntokenizedContact = GetContactItemFromText(text);
        }

        private IEnumerable<ContactItem> GetSuggestedItems(string searchText)
        {
            var availableContacts = Contacts.Where(contact => !SelectedContacts.Contains(contact));

            var namesStartWith = availableContacts.Where(contact => contact.FullName != null && contact.FullName.StartsWith(searchText, StringComparison.CurrentCultureIgnoreCase));
            var emailsStartWith = availableContacts.Where(contact => contact.Email != null && StringHelper.EmailStartsWith(contact.Email.Address, searchText));
            var namesContain = availableContacts.Where(contact => contact.FullName != null && StringHelper.StringContains(contact.FullName, searchText, StringComparison.CurrentCultureIgnoreCase));
            var emailsContain = availableContacts.Where(contact => contact.Email != null && StringHelper.EmailContains(contact.Email.Address, searchText));

            return namesStartWith.Concat(emailsStartWith).Concat(namesContain).Concat(emailsContain).Distinct();
        }

#if NETFX_CORE
        private void SuggestBox_TokenItemAdding(object sender, Microsoft.Toolkit.Uwp.UI.Controls.TokenItemAddingEventArgs e)
        {
            var contact = GetContactItemFromText(e.TokenText);

            e.Item = contact;

            if (contact == null || SelectedContacts.Any(selContact => selContact.Email == contact.Email))
            {
                e.Cancel = true;
            }
        }
#endif

        private ContactItem GetContactItemFromText(string text)
        {
            // Take the text and convert it to our data type (if we have a matching one).
            var contact = GetMatchedItem(text);

            // Otherwise, create a new version of our data type
            if (contact is null)
            {
                var email = new EmailAddress(text);

                if(EmailValidator.Validate(email.IsHybrid ? email.StandardAddress : text, allowTopLevelDomains: true))
                {
                    contact = new ContactItem()
                    {
                        Email = email
                    };
                }
            }

            return contact;
        }

        private ContactItem GetMatchedItem(string searchText)
        {
            return Contacts.FirstOrDefault(contact => StringHelper.AreEmailsEqual(contact.Email.Address, searchText));
        }


        public EmailsInputControl()
        {
            InitializeComponent();
        }
    }
}
