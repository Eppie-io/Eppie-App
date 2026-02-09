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

using Tuvi.Core;
using Tuvi.Core.DataStorage;
using Tuvi.Core.Entities;
using Tuvi.Core.Utils;

namespace Eppie.App.ViewModels.Tests.TestDoubles
{
    internal sealed class FakeTuviMail : ITuviMail
    {
        private readonly List<Account> _accounts = new();
        public readonly List<Message> SentMessages = new();

        public bool ShouldThrowOnSend { get; set; }

        public void SeedAccounts(IEnumerable<Account> accounts)
        {
            _accounts.Clear();
            if (accounts != null)
            {
                _accounts.AddRange(accounts);
            }
            // Notify listeners that accounts are available/updated
            foreach (var acc in _accounts)
            {
                AccountAdded?.Invoke(this, new AccountEventArgs(acc));
            }
        }
        private readonly List<Contact> _contacts = new();
        private readonly Dictionary<EmailAddress, int> _unreadCounts = new();

        public int GetContactsCalls { get; private set; }
        public int GetUnreadCountsCalls { get; private set; }
        public int GetCompositeAccountsCalls { get; private set; }

        public int SetContactNameCalls { get; private set; }
        public (EmailAddress Email, string Name)? LastSetContactName { get; private set; }

        public int SetContactAvatarCalls { get; private set; }
        public (EmailAddress Email, byte[] Bytes, int Width, int Height)? LastSetContactAvatar { get; private set; }

        public int RemoveContactCalls { get; private set; }
        public EmailAddress? LastRemovedContactEmail { get; private set; }

#pragma warning disable CS0067
        public event EventHandler<MessagesReceivedEventArgs>? MessagesReceived;
        public event EventHandler<UnreadMessagesReceivedEventArgs>? UnreadMessagesReceived;
        public event EventHandler<MessageDeletedEventArgs>? MessageDeleted;
        public event EventHandler<MessagesAttributeChangedEventArgs>? MessagesIsReadChanged;
        public event EventHandler<MessagesAttributeChangedEventArgs>? MessagesIsFlaggedChanged;
        public event EventHandler<AccountEventArgs>? AccountAdded;
        public event EventHandler<AccountEventArgs>? AccountUpdated;
        public event EventHandler<AccountEventArgs>? AccountDeleted;
        public event EventHandler<ExceptionEventArgs>? ExceptionOccurred;
        public event EventHandler<ContactAddedEventArgs>? ContactAdded;
        public event EventHandler<ContactChangedEventArgs>? ContactChanged;
        public event EventHandler<ContactDeletedEventArgs>? ContactDeleted;
        public event EventHandler<EventArgs>? WipeAllDataNeeded;
#pragma warning restore CS0067

        public void SeedContacts(IEnumerable<Contact> contacts)
        {
            _contacts.Clear();
            _contacts.AddRange(contacts);
            // Notify listeners that contacts are available/updated
            foreach (var c in _contacts)
            {
                ContactAdded?.Invoke(this, new ContactAddedEventArgs(c));
            }
        }

        public void SetUnreadCounts(IReadOnlyDictionary<EmailAddress, int> counts)
        {
            _unreadCounts.Clear();
            foreach (var kv in counts)
            {
                _unreadCounts[kv.Key] = kv.Value;
            }
        }

        public void RaiseContactAdded(Contact contact)
            => ContactAdded?.Invoke(this, new ContactAddedEventArgs(contact));

        public void RaiseContactChanged(Contact contact)
            => ContactChanged?.Invoke(this, new ContactChangedEventArgs(contact));

        public void RaiseContactDeleted(EmailAddress email)
            => ContactDeleted?.Invoke(this, new ContactDeletedEventArgs(email));

        public void RaiseMessageDeleted(EmailAddress accountEmail)
            => MessageDeleted?.Invoke(this, new MessageDeletedEventArgs(accountEmail, new Folder(), 0));

        public void RaiseMessagesIsReadChanged(EmailAddress accountEmail)
            => MessagesIsReadChanged?.Invoke(this, new MessagesAttributeChangedEventArgs(accountEmail, new Folder(), Array.Empty<Message>()));

        public void RaiseUnreadMessagesReceived(EmailAddress accountEmail)
            => UnreadMessagesReceived?.Invoke(this, new UnreadMessagesReceivedEventArgs(accountEmail, new Folder()));

        public IBackupManager GetBackupManager() => throw new NotImplementedException();
        public ISecurityManager GetSecurityManager() => throw new NotImplementedException();
        public ICredentialsManager CredentialsManager => throw new NotImplementedException();
        public ITextUtils GetTextUtils() => throw new NotImplementedException();
        public IAIAgentsStorage GetAIAgentsStorage() => throw new NotImplementedException();

        public Task TestMailServerAsync(string serverAddress, int serverPort, MailProtocol protocol, ICredentialsProvider credentialsProvider, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> InitializeApplicationAsync(string password, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> IsFirstApplicationStartAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task ResetApplicationAsync() => throw new NotImplementedException();
        public Task<bool> ChangeApplicationPasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<bool> ExistsAccountWithEmailAddressAsync(EmailAddress email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Account> GetAccountAsync(EmailAddress email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<CompositeAccount>> GetCompositeAccountsAsync(CancellationToken cancellationToken = default)
        {
            GetCompositeAccountsCalls++;
            // Return empty list - tests that need accounts should use contacts with LastMessageData set
            return Task.FromResult<IReadOnlyList<CompositeAccount>>(Array.Empty<CompositeAccount>());
        }
        public Task<IAccountService> GetAccountServiceAsync(EmailAddress email, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddAccountAsync(Account account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAccountAsync(Account account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAccountAsync(Account account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task CreateHybridAccountAsync(Account account, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task CheckForNewMessagesInFolderAsync(CompositeFolder folder, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task CheckForNewInboxMessagesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IEnumerable<Contact>> GetContactsAsync(CancellationToken cancellationToken = default)
        {
            GetContactsCalls++;
            return Task.FromResult<IEnumerable<Contact>>(_contacts.ToList());
        }

        public Task<List<Account>> GetAccountsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<Account>(_accounts));
        }

        public Task<IReadOnlyList<Contact>> GetContactsAsync(int count, Contact lastContact, ContactsSortOrder sortOrder, CancellationToken cancellationToken = default)
        {
            GetContactsCalls++;

            IEnumerable<Contact> ordered = sortOrder switch
            {
                ContactsSortOrder.ByName => _contacts.OrderBy(c => c.FullName ?? string.Empty, StringComparer.OrdinalIgnoreCase),
                _ => _contacts
            };

            if (lastContact != null)
            {
                var skipUntil = ordered.ToList();
                var idx = skipUntil.FindIndex(c => c.Email.HasSameAddress(lastContact.Email));
                if (idx >= 0)
                {
                    ordered = skipUntil.Skip(idx + 1);
                }
            }

            return Task.FromResult<IReadOnlyList<Contact>>(ordered.Take(count).ToList());
        }

        public Task SetContactNameAsync(EmailAddress contactEmail, string newName, CancellationToken cancellationToken = default)
        {
            SetContactNameCalls++;
            LastSetContactName = (contactEmail, newName);

            var existing = _contacts.FirstOrDefault(c => c.Email.HasSameAddress(contactEmail));
            if (existing != null)
            {
                existing.FullName = newName;
            }

            return Task.CompletedTask;
        }

        public Task SetContactAvatarAsync(EmailAddress contactEmail, byte[] avatarBytes, int avatarWidth, int avatarHeight, CancellationToken cancellationToken = default)
        {
            SetContactAvatarCalls++;
            LastSetContactAvatar = (contactEmail, avatarBytes, avatarWidth, avatarHeight);

            var existing = _contacts.FirstOrDefault(c => c.Email.HasSameAddress(contactEmail));
            if (existing != null)
            {
                existing.AvatarInfo = new ImageInfo { Bytes = avatarBytes, Width = avatarWidth, Height = avatarHeight };
            }

            return Task.CompletedTask;
        }

        public Task RemoveContactAsync(EmailAddress contactEmail, CancellationToken cancellationToken = default)
        {
            RemoveContactCalls++;
            LastRemovedContactEmail = contactEmail;

            _contacts.RemoveAll(c => c.Email.HasSameAddress(contactEmail));

            return Task.CompletedTask;
        }

        public Task<IReadOnlyDictionary<EmailAddress, int>> GetUnreadMessagesCountByContactAsync(CancellationToken cancellationToken = default)
        {
            GetUnreadCountsCalls++;
            return Task.FromResult<IReadOnlyDictionary<EmailAddress, int>>(new Dictionary<EmailAddress, int>(_unreadCounts));
        }

        public Task SendMessageAsync(Message message, bool encrypt, bool sign, CancellationToken cancellationToken = default)
        {
            if (ShouldThrowOnSend)
            {
                throw new InvalidOperationException("Simulated send failure");
            }
            SentMessages.Add(message);
            MessageSent?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public event EventHandler? MessageSent;

        public Task<Folder> CreateFolderAsync(EmailAddress accountEmail, string folderName, CancellationToken cancellationToken = default)
        {
            var folder = new Folder { FullName = folderName };
            FolderCreated?.Invoke(this, new FolderCreatedEventArgs(folder, accountEmail));
            return Task.FromResult(folder);
        }

        public event EventHandler<FolderCreatedEventArgs>? FolderCreated;


        public Task DeleteFolderAsync(EmailAddress accountEmail, Folder folder, CancellationToken cancellationToken = default)
        {
            FolderDeleted?.Invoke(this, new FolderDeletedEventArgs(folder, accountEmail));
            return Task.CompletedTask;
        }

        public Task DeleteFolderAsync(EmailAddress accountEmail, CompositeFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is null)
            {
                return Task.CompletedTask;
            }

            // CompositeFolder contains the base Folder, so we can pass it to the event
            var baseFolder = new Folder { FullName = folder.FullName };
            FolderDeleted?.Invoke(this, new FolderDeletedEventArgs(baseFolder, accountEmail));
            return Task.CompletedTask;
        }

        public event EventHandler<FolderDeletedEventArgs>? FolderDeleted;


        public Task<Folder> RenameFolderAsync(EmailAddress accountEmail, Folder folder, string newName, CancellationToken cancellationToken = default)
        {
            var oldName = folder.FullName;
            folder.FullName = newName;
            FolderRenamed?.Invoke(this, new FolderRenamedEventArgs(folder, accountEmail, oldName));
            return Task.FromResult(folder);
        }

        public event EventHandler<FolderRenamedEventArgs>? FolderRenamed;

        public Task<IReadOnlyList<Message>> GetAllEarlierMessagesAsync(int count, Message lastMessage, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Message>> GetContactEarlierMessagesAsync(EmailAddress contactEmail, int count, Message lastMessage, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Message>> GetFolderEarlierMessagesAsync(Folder folder, int count, Message lastMessage, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Message>> GetFolderEarlierMessagesAsync(CompositeFolder folder, int count, Message lastMessage, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetUnreadCountForAllAccountsInboxAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteMessagesAsync(IReadOnlyList<Message> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RestoreFromBackupIfNeededAsync(Uri downloadUri) => throw new NotImplementedException();
        public Task MarkMessagesAsReadAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task MarkMessagesAsUnReadAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task FlagMessagesAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UnflagMessagesAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Message> GetMessageBodyAsync(Message message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Message> GetMessageBodyHighPriorityAsync(Message message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Message> CreateDraftMessageAsync(Message message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Message> UpdateDraftMessageAsync(uint id, Message message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task MoveMessagesAsync(IReadOnlyList<Message> messages, CompositeFolder targetFolder, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateMessageProcessingResultAsync(Message message, string result, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ClaimDecentralizedNameAsync(string name, EmailAddress address, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> ClaimDecentralizedNameAsync(string name, Account account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void Dispose()
        {
        }
    }
}
