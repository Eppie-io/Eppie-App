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
using Tuvi.App.ViewModels.Common;
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
        public IRelayCommand<Problem> ActionCommand { get; set; }
        public string SolutionText { get; set; }
        public static MainPageViewModel MainPageViewModel { get; set; }
        public IRelayCommand<Problem> CloseCommand => MainPageViewModel.CloseProblemCommand;

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

        private async void NavigateToProton(EmailAddress emailAddress)
        {
            try
            {
                var account = await Core.GetAccountAsync(emailAddress).ConfigureAwait(true);
                NavigationService?.Navigate(nameof(ProtonAccountSettingsPageViewModel), account);
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
            Problem.MainPageViewModel = this;
            try
            {
                base.OnNavigatedTo(data);

                await UpdateContactsAsync().ConfigureAwait(true);
                await UpdateAccountListAsync().ConfigureAwait(true);

                SubscribeOnCoreEvents();

                await UpdateContactsUnreadCountAsync().ConfigureAwait(true);

                // Test node URI
                const string downloadUrl = "https://testnet.eppie.io/api/DownloadBackupFunction?code=1&name=";
                // Local node URI
                //const string downloadUrl = "http://localhost:7071/api/DownloadBackupFunction?name=";

                await Core.RestoreFromBackupIfNeededAsync(new Uri(downloadUrl)).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
            UnsubscribeFromCoreEvents();
        }

        private void SubscribeOnCoreEvents()
        {
            Core.ExceptionOccurred += OnCoreException;
            Core.UnreadMessagesReceived += OnUnreadMessagesReceived;
            Core.MessageDeleted += OnMessageDeleted;
            Core.MessagesIsReadChanged += OnMessagesIsReadChanged;
            Core.AccountAdded += OnAccountAdded;
            Core.ContactAdded += OnContactAdded;
            Core.ContactChanged += OnContactChanged;
            Core.ContactDeleted += OnContactDeleted;
        }

        private void UnsubscribeFromCoreEvents()
        {
            Core.ExceptionOccurred -= OnCoreException;
            Core.UnreadMessagesReceived -= OnUnreadMessagesReceived;
            Core.MessageDeleted -= OnMessageDeleted;
            Core.MessagesIsReadChanged -= OnMessagesIsReadChanged;
            Core.AccountAdded -= OnAccountAdded;
            Core.ContactAdded -= OnContactAdded;
            Core.ContactChanged -= OnContactChanged;
            Core.ContactDeleted -= OnContactDeleted;
        }

        private async void OnCoreException(object sender, ExceptionEventArgs e)
        {
            await DispatcherService.RunAsync(() =>
            {
                if (e.Exception is AuthorizationException ex)
                {
                    Problems.Add(new Problem()
                    {
                        Title = GetLocalizedString("AuthorizationProblem"),
                        Message = ex.Message,
                        SolutionText = GetLocalizedString("GoToSettings"),
                        ActionCommand = new RelayCommand<Problem>((p) =>
                        {
                            NavigateToProton(ex.Email);
                            CloseProblem(p);
                        })
                    });
                    return;
                }
                else if (e.Exception is AuthenticationException ex2)
                {
                    Problems.Add(new Problem()
                    {
                        Title = GetLocalizedString("AuthenticationProblem"),
                        Message = ex2.Message,
                        SolutionText = GetLocalizedString("GoToSettings"),
                        ActionCommand = new RelayCommand<Problem>((p) =>
                        {
                            NavigateToProton(ex2.Email);
                            CloseProblem(p);
                        })
                    });
                    return;
                }

                //TODO: TVM-388 error messages disabled temporarily
                //OnError(e.Exception);
            });
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
                await UpdateMailboxItemsUnreadCountAsync(email).ConfigureAwait(true);
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

        private async void OnAccountAdded(object sender, AccountEventArgs e)
        {
            try
            {
                await DispatcherService.RunAsync(async () =>
                {
                    try
                    {
                        await UpdateAccountListAsync().ConfigureAwait(true);
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

        private async Task UpdateMailboxItemsUnreadCountAsync(EmailAddress email)
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
            foreach (var account in accounts)
            {
                await UpdateMailboxItemsUnreadCountAsync(account.Email).ConfigureAwait(true);
            }
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
                foreach (var p in counts)
                {
                    ContactsModel.SetUnreadCount(p.Key, p.Value);
                }
            }).ConfigureAwait(true);
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

        public void InitializeNavPanelTabModel(ICommand contactClickCommand, ICommand changeContactAvatarCommand, ICommand mailBoxItemClick)
        {
            ContactsModel contactsModel = CreateContactsModel(contactClickCommand, changeContactAvatarCommand);
            MailBoxesModel mailBoxesModel = new MailBoxesModel(mailBoxItemClick);

            NavPanelTabModel = new NavPanelTabModel(contactsModel, mailBoxesModel);
        }

        private ContactsModel CreateContactsModel(ICommand contactClickCommand, ICommand changeContactAvatarCommand)
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
    }
}
