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
using CommunityToolkit.WinUI.Controls;
using Tuvi.Core.Entities;
using Tuvi.App.ViewModels;
using System.Diagnostics.CodeAnalysis;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class EmailsInputControl : UserControl
    {
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


        public ObservableCollection<ContactItem> Contacts
        {
            get { return (ObservableCollection<ContactItem>)GetValue(ContactsProperty); }
            set { SetValue(ContactsProperty, value); }
        }
        public static readonly DependencyProperty ContactsProperty =
            DependencyProperty.Register(nameof(Contacts), typeof(object), typeof(EmailsInputControl), new PropertyMetadata(new ObservableCollection<ContactItem>()));

        private string _oldUserInputText = string.Empty;
        private void SuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _oldUserInputText = string.Empty;

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


        private void SuggestBox_TokenItemAdding(object sender, TokenItemAddingEventArgs e)
        {
            var contact = GetContactItemFromText(e.TokenText);

            e.Item = contact;

            if (contact == null || SelectedContacts.Any(selContact => selContact.Email == contact.Email))
            {
                e.Cancel = true;
            }
        }

        private void SuggestBox_TokenItemAdded(TokenizingTextBox sender, object args)
        {
            // Remove contact added by LostFocus if its DisplayName matches the previous user input
            var userInputText = _oldUserInputText?.Trim();

            _oldUserInputText = string.Empty;
            sender.Text = string.Empty;
            UntokenizedContact = null;

            // Remove any contact that was added by LostFocus from partial user input
            if (!string.IsNullOrWhiteSpace(userInputText) && args is ContactItem addedContact && addedContact != null)
            {
                var partialContact = SelectedContacts.LastOrDefault(c => c != addedContact &&
                    !string.IsNullOrWhiteSpace(c.DisplayName) &&
                    string.Equals(c.DisplayName, userInputText, StringComparison.OrdinalIgnoreCase));

                if (partialContact != null)
                {
                    SelectedContacts.Remove(partialContact);
                }
            }

            // Compare only the last two contacts and remove one if their emails match
            if (SelectedContacts.Count >= 2)
            {
                var last = SelectedContacts[SelectedContacts.Count - 1];
                var prev = SelectedContacts[SelectedContacts.Count - 2];
                if (last.Email == prev.Email)
                {
                    SelectedContacts.Remove(last);
                }
            }
        }

        private ContactItem GetContactItemFromText(string text)
        {
            text = CleanAddress(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Take the text and convert it to our data type (if we have a matching one).
            var contact = GetMatchedItem(text);

            // Otherwise, create a new version of our data type
            if (contact is null)
            {
                contact = new ContactItem()
                {
                    Email = new EmailAddress(text)
                };
            }

            return contact;
        }

        private static string CleanAddress(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            var result = email.Trim();

            const string MailtoPrefix = "mailto:";
            if (result.StartsWith(MailtoPrefix, StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(MailtoPrefix.Length);
            }

            return result;
        }

        private ContactItem GetMatchedItem(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return null;
            }

            return Contacts.FirstOrDefault(contact => StringHelper.AreEmailsEqual(contact.Email.Address, searchText));
        }

        private void SuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UntokenizedContact?.Email != null &&
                !SelectedContacts.Any(c => c.Email != null && c.Email.Address == UntokenizedContact.Email.Address))
            {
                SelectedContacts.Add(UntokenizedContact);

                UntokenizedContact = null;
                if (sender is TokenizingTextBox tokenizingTextBox)
                {
                    _oldUserInputText = tokenizingTextBox.Text;
                    tokenizingTextBox.Text = string.Empty;
                }
            }
        }

        public EmailsInputControl()
        {
            InitializeComponent();
        }
    }
}
