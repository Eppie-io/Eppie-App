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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Tsp;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    using AttachmentsCollection = List<Attachment>;

    public interface IMessageInfo
    {
        string MessageSender { get; }
        string MessageReceiver { get; }
        string MessageSubject { get; }
        string DateBriefString { get; }
        string DateFullString { get; }
        string PreviewText { get; }
        bool HasAttachments { get; }
        bool IsMarkedAsRead { get; }
        bool IsFlagged { get; }
        bool WasDeleted { get; }
        bool IsDecentralized { get; }
    }

    public class MessageInfo : ObservableObject, IMessageInfo
    {
        private EmailAddress _email;
        public EmailAddress Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        public Folder Folder => MessageData.Folder;

        private Message _message;
        public Message MessageData
        {
            get { return _message; }
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value), nameof(MessageData));
                }

                SetProperty(ref _message, value);
                UpdateMessageProperties();
            }
        }

        public uint MessageID => MessageData.Id;

        public string MessageSender
        {
            get
            {
                return _message.From != null
                    ? EmailAddressesToString(_message.From)
                    : "";
            }
        }

        public string MessageReceiver
        {
            get
            {
                return _message.To != null
                    ? EmailAddressesToString(_message.To)
                    : "";
            }
        }

        public string MessageCopy
        {
            get
            {
                return _message.Cc != null
                    ? EmailAddressesToString(_message.Cc)
                    : "";
            }
        }

        public string MessageHiddenCopy
        {
            get
            {
                return _message.Bcc != null
                    ? EmailAddressesToString(_message.Bcc)
                    : "";
            }
        }

        static private string EmailAddressesToString(IEnumerable<EmailAddress> values)
        {
            var result = values.Where(m => !string.IsNullOrWhiteSpace(m?.Address)).Select(m => m.DisplayName);

            return result.Any()
                ? string.Join(", ", result)
                : "";
        }

        public string MessageSubject
        {
            get { return _message.Subject ?? ""; }
        }

        public string MessageTextBody
        {
            get { return _message.TextBody ?? ""; }
        }

        public string MessageHtmlBody
        {
            get { return _message.HtmlBody ?? ""; }
        }

        public string AIAgentProcessedBody
        {
            get { return MessageData.TextBodyProcessed; }
            set
            {
                MessageData.TextBodyProcessed = value;
                OnPropertyChanged();
            }
        }

        public bool IsEmptyBody
        {
            get { return string.IsNullOrEmpty(_message.TextBody) && string.IsNullOrEmpty(_message.HtmlBody); }
        }

        public bool HasHtmlBody
        {
            get { return !string.IsNullOrEmpty(MessageHtmlBody); }
        }

        public bool HasTextBody
        {
            get { return !string.IsNullOrEmpty(MessageTextBody); }
        }

        public string PreviewText
        {
            get { return _message.PreviewText ?? ""; }
        }

        public AttachmentsCollection Attachments
        {
            get { return _message.Attachments ?? new AttachmentsCollection(); }
        }

        public bool HasAttachments
        {
            get { return Attachments.Count > 0; }
        }

        public bool IsMarkedAsRead
        {
            get { return _message.IsMarkedAsRead; }
        }

        public bool IsFlagged
        {
            get { return _message.IsFlagged; }
        }

        private bool _wasDeleted;
        public bool WasDeleted
        {
            get => _wasDeleted;
            set => SetProperty(ref _wasDeleted, value);
        }

        public bool IsDecentralized
        {
            get { return _message.IsDecentralized; }
        }

        private string _dateBriefString;
        public string DateBriefString
        {
            get
            {
                if (!string.IsNullOrEmpty(_dateBriefString))
                {
                    return _dateBriefString;
                }
                var date = _message.Date.ToLocalTime();

                if (date.Day == DateTimeOffset.Now.Day &&
                    date.Month == DateTimeOffset.Now.Month &&
                    date.Year == DateTimeOffset.Now.Year)
                {
                    _dateBriefString = date.ToString("t", CultureInfo.CurrentCulture);
                }
                else if (_message.Date.Year != DateTimeOffset.Now.Year)
                {
                    _dateBriefString = date.ToString("d", CultureInfo.CurrentCulture);
                }
                else
                {
                    _dateBriefString = date.ToString("M", CultureInfo.CurrentCulture);
                }

                return _dateBriefString;
            }
        }

        public string DateFullString
        {
            get { return _message.Date.ToLocalTime().ToString("g", CultureInfo.CurrentCulture); }
        }

        public bool IsEncrypted
        {
            get
            {
                return _message.Protection.Type == MessageProtectionType.Encryption
                      || _message.Protection.Type == MessageProtectionType.SignatureAndEncryption;
            }
        }

        public bool IsSigned
        {
            get
            {
                return (
                           _message.Protection.Type == MessageProtectionType.Signature
                        || _message.Protection.Type == MessageProtectionType.SignatureAndEncryption
                       )
                    && VerifySignature(_message);
            }
        }

        public MessageInfo(EmailAddress email, Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Email = email;
            MessageData = message;
        }

        private static bool VerifySignature(Message message)
        {
            var fromAddresses = message.From;
            var signatures = message.Protection.SignaturesInfo;

            var verified = fromAddresses.TrueForAll((from) => signatures.Any((signature) => { return from.HasSameAddress(signature.SignerEmail) && signature.IsVerified; }));

            return verified;
        }

        public void MarkAsRead()
        {
            SetRead(true);
        }

        public void MarkAsUnread()
        {
            SetRead(false);
        }

        public void Flag()
        {
            SetFlaged(true);
        }

        public void Unflag()
        {
            SetFlaged(false);
        }

        public void UpdateMessageAttributes(Message message)
        {
            if (_message.TryToUpdate(message))
            {
                OnPropertyChanged(nameof(IsMarkedAsRead));
                OnPropertyChanged(nameof(IsFlagged));
            }
        }

        private void UpdateMessageProperties()
        {
            OnPropertyChanged(nameof(MessageSender));
            OnPropertyChanged(nameof(MessageReceiver));
            OnPropertyChanged(nameof(MessageSubject));
            OnPropertyChanged(nameof(MessageTextBody));
            OnPropertyChanged(nameof(MessageHtmlBody));
            OnPropertyChanged(nameof(HasHtmlBody));
            OnPropertyChanged(nameof(PreviewText));
            OnPropertyChanged(nameof(Attachments));
            OnPropertyChanged(nameof(HasAttachments));
            OnPropertyChanged(nameof(IsMarkedAsRead));
            OnPropertyChanged(nameof(IsFlagged));
            OnPropertyChanged(nameof(DateBriefString));
            OnPropertyChanged(nameof(DateFullString));
            OnPropertyChanged(nameof(IsSigned));
            OnPropertyChanged(nameof(IsEncrypted));
            OnPropertyChanged(nameof(AIAgentProcessedBody));
        }

        private void SetFlaged(bool value)
        {
            _message.IsFlagged = value;
            OnPropertyChanged(nameof(IsFlagged));
        }

        private void SetRead(bool value)
        {
            _message.IsMarkedAsRead = value;
            OnPropertyChanged(nameof(IsMarkedAsRead));
        }
    }

    public class Problem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public EmailAddress Email { get; set; }
        public IRelayCommand<Problem> ActionCommand { get; set; }
        public string SolutionText { get; set; }
        public MainPageViewModel ViewModel { get; set; }
        public IRelayCommand<Problem> CloseCommand => ViewModel.CloseProblemCommand;

    }

    public class MainPageViewModel : BaseViewModel
    {
        public NavPanelTabModel NavPanelTabModel { get; private set; }

        private ContactsModel ContactsModel => NavPanelTabModel.ContactsModel;

        private MailBoxesModel MailBoxesModel => NavPanelTabModel.MailBoxesModel;

        public ICommand WriteNewMessageCommand
        {
            get
            {
                return new AsyncRelayCommand(WriteNewMessageAsync);
            }
        }

        public ObservableCollection<Problem> Problems { get; } = new ObservableCollection<Problem>();

        private async void NavigateToMailboxSettingsForRelogin(EmailAddress emailAddress)
        {
            try
            {
                var account = await Core.GetAccountAsync(emailAddress).ConfigureAwait(true);

                NavigateToMailboxSettingsPage(account, isReloginNeeded: true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task WriteNewMessageAsync()
        {
            try
            {
                if (MailBoxesModel.AccountList.Count > 0)
                {
                    NewMessageData messageData = null;
                    if (NavPanelTabModel.SelectedItemModel is ContactsModel contactsModel
                        && contactsModel.SelectedContact != null)
                    {
                        messageData = new SelectedContactNewMessageData(
                            contactsModel.SelectedContact.LastMessageData.AccountEmail,
                            contactsModel.SelectedContact.Email);
                    }
                    else if (NavPanelTabModel.SelectedItemModel is MailBoxesModel mailBoxesModel
                        && mailBoxesModel.SelectedItem != null)
                    {
                        messageData = new SelectedAccountNewMessageData(mailBoxesModel.SelectedItem.Email);
                    }
                    NavigationService?.Navigate(nameof(NewMessagePageViewModel), messageData);
                }
                else
                {
                    await MessageService.ShowAddAccountMessageAsync().ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public IRelayCommand<Problem> CloseProblemCommand => new RelayCommand<Problem>(CloseProblem);

        private void CloseProblem(Problem problem)
        {
            Problems.Remove(problem);
        }

        public ICommand RemoveContactCommand => new AsyncRelayCommand<ContactItem>(RemoveContactAsync);

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                base.OnNavigatedTo(data);

                await UpdateContactsAsync().ConfigureAwait(true);
                await UpdateAccountListAsync().ConfigureAwait(true);

                SubscribeEvents();

                UpdateUnreadCounts();

                // Test node URI
                const string downloadUrl = "https://testnet.eppie.io/api/DownloadBackupFunction?code=1&name=";
                // Local node URI
                //const string downloadUrl = "http://localhost:7071/api/DownloadBackupFunction?name=";

                await Core.RestoreFromBackupIfNeededAsync(new Uri(downloadUrl)).ConfigureAwait(true);

                await RequestReviewAsync().ConfigureAwait(true);

                LogEnabledWarning();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task RequestReviewAsync()
        {
            const int ReviewRequestsThreshold = 10;
            const int ReviewRequestsDisabled = -1;

            var count = LocalSettingsService.RequestReviewCount;

            if (count > ReviewRequestsThreshold)
            {
                if (await MessageService.ShowRequestReviewMessageAsync().ConfigureAwait(true))
                {
                    await AppStoreService.RequestReviewAsync().ConfigureAwait(true);
                }

                LocalSettingsService.RequestReviewCount = ReviewRequestsDisabled;
            }
            else if (count != ReviewRequestsDisabled)
            {
                LocalSettingsService.RequestReviewCount++;
            }
        }

        private async void UpdateUnreadCounts()
        {
            try
            {
                await UpdateContactsUnreadCountAsync().ConfigureAwait(true);
                await UpdateMailboxItemsUnreadCountAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task UpdateMailboxItemsUnreadCountAsync()
        {
            var accounts = await Core.GetCompositeAccountsAsync().ConfigureAwait(true);
            foreach (var account in accounts)
            {
                await UpdateMailboxItemUnreadCountAsync(account.Email).ConfigureAwait(true);
            }
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            Core.ExceptionOccurred += OnCoreException;
            Core.UnreadMessagesReceived += OnUnreadMessagesReceived;
            Core.MessageDeleted += OnMessageDeleted;
            Core.MessagesIsReadChanged += OnMessagesIsReadChanged;
            Core.AccountAdded += OnAccountAdded;
            Core.AccountUpdated += OnAccountUpdated;
            Core.ContactAdded += OnContactAdded;
            Core.ContactChanged += OnContactChanged;
            Core.ContactDeleted += OnContactDeleted;

            LocalSettingsService.SettingChanged += LocalSettingsService_SettingChanged;
        }

        private void UnsubscribeEvents()
        {
            Core.ExceptionOccurred -= OnCoreException;
            Core.UnreadMessagesReceived -= OnUnreadMessagesReceived;
            Core.MessageDeleted -= OnMessageDeleted;
            Core.MessagesIsReadChanged -= OnMessagesIsReadChanged;
            Core.AccountAdded -= OnAccountAdded;
            Core.AccountUpdated -= OnAccountUpdated;
            Core.ContactAdded -= OnContactAdded;
            Core.ContactChanged -= OnContactChanged;
            Core.ContactDeleted -= OnContactDeleted;

            LocalSettingsService.SettingChanged -= LocalSettingsService_SettingChanged;
        }

        private void OnCoreException(object sender, ExceptionEventArgs e)
        {
            OnError(e.Exception);
        }

        public override async void OnError(Exception e)
        {
            await DispatcherService.RunAsync(() =>
            {
                if (e is AuthorizationException ex)
                {
                    if (ex.Email != null)
                    {
                        AddProblem(GetLocalizedString("AuthorizationProblem"), GetLocalizedString("GoToSettings"), ex.Email, ex.Message);

                        return;
                    }
                }
                else if (e is AuthenticationException ex2)
                {
                    if (ex2.Email != null)
                    {
                        AddProblem(GetLocalizedString("AuthenticationProblem"), GetLocalizedString("GoToSettings"), ex2.Email, ex2.Message);

                        return;
                    }
                }

                base.OnError(e);
            }).ConfigureAwait(true);
        }

        private void AddProblem(string title, string solution, EmailAddress email, string message)
        {
            if (!Problems.Any(x => { return x.Title == title && x.SolutionText == solution && x.Email == email; }))
            {
                Problems.Add(new Problem()
                {
                    ViewModel = this,
                    Title = title,
                    SolutionText = solution,
                    Email = email,
                    Message = message,
                    ActionCommand = new RelayCommand<Problem>((p) =>
                    {
                        NavigateToMailboxSettingsForRelogin(p.Email);
                        CloseProblem(p);
                    })
                });
            }
        }

        private void OnMessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            UpdateUnreadCounts(e.Email);
        }

        private void OnMessagesIsReadChanged(object sender, MessagesAttributeChangedEventArgs e)
        {
            UpdateUnreadCounts(e.Email);
        }

        private void OnUnreadMessagesReceived(object sender, UnreadMessagesReceivedEventArgs e)
        {
            UpdateUnreadCounts(e.Email);
        }

        private async void UpdateUnreadCounts(EmailAddress email)
        {
            try
            {
                await UpdateMailboxItemUnreadCountAsync(email).ConfigureAwait(true);
                await UpdateContactsUnreadCountAsync().ConfigureAwait(true);
            }
            catch (ObjectDisposedException)
            { }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnAccountAdded(object sender, AccountEventArgs e)
        {
            UpdateAccountsList();
        }

        private void OnAccountUpdated(object sender, AccountEventArgs e)
        {
            UpdateAccountsList();
        }

        private async void UpdateAccountsList()
        {
            try
            {
                await DispatcherService.RunAsync(async () =>
                {
                    try
                    {
                        await UpdateAccountListAsync().ConfigureAwait(true);
                        await UpdateMailboxItemsUnreadCountAsync().ConfigureAwait(true);
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async void OnContactAdded(object sender, ContactAddedEventArgs e)
        {
            try
            {
                await DispatcherService.RunAsync(() =>
                {
                    ContactsModel.AddContact(new ContactItem(e.Contact));
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
                    ContactsModel.UpdateContact(new ContactItem(e.Contact));
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
                    ContactsModel.RemoveContactByEmail(e.ContactEmail);
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task UpdateMailboxItemUnreadCountAsync(EmailAddress email)
        {
            var rootItem = MailBoxesModel.GetRootItemByEmail(email);
            if (rootItem is null)
            {
                // TODO: investigate
                return;
            }

            var rootUnreadCount = 0;

            foreach (var folderItem in rootItem.Children)
            {
                var unreadCount = await folderItem.Folder.GetUnreadMessagesCountAsync(default).ConfigureAwait(true);
                await SetMailBoxItemUnreadCountAsync(folderItem, unreadCount).ConfigureAwait(true);
                rootUnreadCount += unreadCount;
            }
            await SetMailBoxItemUnreadCountAsync(rootItem, rootUnreadCount).ConfigureAwait(true);
        }

        private Task SetMailBoxItemUnreadCountAsync(MailBoxItem mailboxItem, int unreadCount)
        {
            return DispatcherService.RunAsync(() =>
            {
                mailboxItem.UnreadMessagesCount = unreadCount;
            });
        }

        private async Task UpdateAccountListAsync()
        {
            var accounts = await Core.GetCompositeAccountsAsync().ConfigureAwait(true);
            MailBoxesModel.SetAccounts(accounts);
        }

        private async Task UpdateContactsAsync()
        {
            if (ContactsModel != null)
            {
                var contacts = await Core.GetContactsAsync().ConfigureAwait(true);
                ContactsModel.SetContacts(contacts.Select(contact => new ContactItem(contact)));
            }
        }

        private async Task UpdateContactsUnreadCountAsync()
        {
            var counts = await Core.GetUnreadMessagesCountByContactAsync().ConfigureAwait(true);
            await DispatcherService.RunAsync(() =>
            {
                ContactsModel.SetUnreadCount(counts);
            }).ConfigureAwait(true);
        }

        public async Task RenameContactAsync(ContactItem contactItem)
        {
            if (contactItem != null)
            {
                await Core.SetContactNameAsync(contactItem.Email, contactItem.FullName).ConfigureAwait(true);
            }
        }

        public async Task SetContactAvatarAsync(ContactItem contactItem, byte[] avatarBytes, int avatarWidth, int avatarHeight, CancellationToken cancellationToken = default)
        {
            if (contactItem != null)
            {
                await Core.SetContactAvatarAsync(contactItem.Email, avatarBytes, avatarWidth, avatarHeight, cancellationToken).ConfigureAwait(true);
            }
        }

        private async Task RemoveContactAsync(ContactItem contactItem)
        {
            try
            {
                await Core.RemoveContactAsync(contactItem.Email).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public void InitializeNavPanelTabModel(ICommand contactClickCommand, ICommand renameContactCommand, ICommand changeContactAvatarCommand, ICommand mailBoxItemClick, ICommand mailBoxItemDrop)
        {
            ContactsModel contactsModel = CreateContactsModel(contactClickCommand, renameContactCommand, changeContactAvatarCommand);
            MailBoxesModel mailBoxesModel = new MailBoxesModel(mailBoxItemClick, mailBoxItemDrop);

            NavPanelTabModel = new NavPanelTabModel(contactsModel, mailBoxesModel);
        }

        private ContactsModel CreateContactsModel(ICommand contactClickCommand, ICommand renameContactCommand, ICommand changeContactAvatarCommand)
        {
            IExtendedComparer<ContactItem>[] contactSortingVariants = new IExtendedComparer<ContactItem>[]
            {
                new ByNameContactComparer(GetLocalizedString("OrderByNameContactComparerLabel")),
                new ByTimeContactComparer(GetLocalizedString("OrderByTimeContactComparerLabel")),
                new ByUnreadContactComparer(GetLocalizedString("OrderByUnreadContactComparerLabel")),
            };

            return new ContactsModel()
            {
                ContactClickCommand = contactClickCommand,
                RemoveContactCommand = RemoveContactCommand,
                ChangeContactAvatarCommand = changeContactAvatarCommand,
                RenameContactCommand = renameContactCommand,

                Contacts = new ManagedCollection<ContactItem>()
                {
                    FilterVariants = Array.Empty<IFilter<ContactItem>>(),
                    SortingVariants = contactSortingVariants,
                    ItemsFilter = null,
                    // TODO: TVM-363                     
                    ItemsComparer = contactSortingVariants.FirstOrDefault(variant => variant is ByTimeContactComparer),
                    SearchFilter = new SearchContactFilter()
                }
            };
        }

        public void OnShowAllMessages()
        {
            ContactsModel.SelectedContact = null;
            MailBoxesModel.SelectedItem = null;
        }

        public async void MailBoxItemDropMessages(MailBoxItem item)
        {
            try
            {
                var messages = DragAndDropService.GetDraggedMessages();
                await Core.MoveMessagesAsync(messages.Select(x => x.MessageData).ToList(), item.Folder, CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public bool MailBoxItem_IsDropMessagesAllowed(MailBoxItem item)
        {
            bool res = false;
            try
            {
                var messages = DragAndDropService.GetDraggedMessages();

                res = messages.Any(x => x.Email == item.Email);
            }
            catch (Exception e)
            {
                OnError(e);
            }

            return res;
        }

        public async Task<bool> IsAccountListEmptyAsync()
        {
            var accounts = await Core.GetCompositeAccountsAsync().ConfigureAwait(true);
            return !accounts.Any();
        }

        private void LogEnabledWarning()
        {
            var title = GetLocalizedString("LogEnabledWarningTitle");
            var solution = GetLocalizedString("DisableLogging");
            var loggingWarning = Problems.FirstOrDefault(x => { return x.Title == title && x.SolutionText == solution; });

            if (LocalSettingsService.LogLevel != LogLevel.None && loggingWarning is null)
            {
                Problems.Add(new Problem()
                {
                    ViewModel = this,
                    Title = title,
                    SolutionText = solution,
                    Email = null,
                    Message = GetLocalizedString("LogEnabledWarningMessage"),
                    ActionCommand = new RelayCommand<Problem>((p) =>
                    {
                        LocalSettingsService.LogLevel = LogLevel.None;
                        CloseProblem(p);
                    })
                });
            }
            else if (LocalSettingsService.LogLevel == LogLevel.None && loggingWarning != null)
            {
                CloseProblem(loggingWarning);
            }
        }

        private void LocalSettingsService_SettingChanged(object sender, SettingChangedEventArgs args)
        {
            try
            {
                if (args.Name == nameof(LocalSettingsService.LogLevel))
                {
                    LogEnabledWarning();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
