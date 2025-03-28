using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using EmailValidation;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Extensions;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class RemovableAttachment : DownloadableAttachment
    {
        public ICommand RemoveCommand { get; set; }
    }

    public static class RemovableAttachmentConvertor
    {
        public static RemovableAttachment ToRemovableAttachment(this AttachedFileInfo attachedFileInfo, ICommand deleteItemCommand, ICommand downloadCommand, ICommand openCommand)
        {
            if (attachedFileInfo is null)
            {
                throw new ArgumentNullException(nameof(attachedFileInfo));
            }

            if (deleteItemCommand is null)
            {
                throw new ArgumentNullException(nameof(deleteItemCommand));
            }

            if (downloadCommand is null)
            {
                throw new ArgumentNullException(nameof(downloadCommand));
            }

            if (openCommand is null)
            {
                throw new ArgumentNullException(nameof(openCommand));
            }

            return new RemovableAttachment
            {
                FileName = attachedFileInfo.Name,
                Data = attachedFileInfo.Data,
                RemoveCommand = deleteItemCommand,
                DownloadCommand = downloadCommand,
                OpenCommand = openCommand
            };
        }

        public static RemovableAttachment ToRemovableAttachment(this Attachment attachment, ICommand deleteItemCommand, ICommand downloadCommand, ICommand openCommand)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (deleteItemCommand is null)
            {
                throw new ArgumentNullException(nameof(deleteItemCommand));
            }

            if (downloadCommand is null)
            {
                throw new ArgumentNullException(nameof(downloadCommand));
            }

            if (openCommand is null)
            {
                throw new ArgumentNullException(nameof(openCommand));
            }

            return new RemovableAttachment
            {
                FileName = attachment.FileName,
                Data = attachment.Data,
                RemoveCommand = deleteItemCommand,
                DownloadCommand = downloadCommand,
                OpenCommand = openCommand
            };
        }

        public static Attachment ToAttachment(this Attachment attachment)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            return new Attachment
            {
                FileName = attachment.FileName,
                Data = attachment.Data
            };
        }
    }

    public class NewMessagePageViewModel : BaseMessageViewModel
    {
        private EmailAddress _from;
        public EmailAddress From
        {
            get { return _from; }
            set
            {
                if (_from != value)
                {
                    SetProperty(ref _from, value);
                    _ = UpdateDraftMessageAsync();
                }
            }
        }
        public ObservableCollection<ContactItem> To { get; } = new ObservableCollection<ContactItem>();
        public ObservableCollection<ContactItem> Copy { get; } = new ObservableCollection<ContactItem>();
        public ObservableCollection<ContactItem> HiddenCopy { get; } = new ObservableCollection<ContactItem>();

        private ContactItem _untokenizedContactTo;
        public ContactItem UntokenizedContactTo
        {
            get { return _untokenizedContactTo; }
            set
            {
                SetProperty(ref _untokenizedContactTo, value);
                SendMessageAndGoBackCommand.NotifyCanExecuteChanged();
            }
        }

        private string _subject = string.Empty;
        public string Subject
        {
            get { return _subject; }
            set { SetProperty(ref _subject, value); }
        }

        private string _textBody;
        public string TextBody
        {
            get { return _textBody; }
            set { SetProperty(ref _textBody, value); }
        }

        private string _htmlBody;
        public string HtmlBody
        {
            get { return _htmlBody; }
            set { SetProperty(ref _htmlBody, value); }
        }

        private bool _isEcnrypted;
        public bool IsEncrypted
        {
            get { return _isEcnrypted; }
            set { SetProperty(ref _isEcnrypted, value); }
        }

        private bool _isSigned;
        public bool IsSigned
        {
            get { return _isSigned; }
            set { SetProperty(ref _isSigned, value); }
        }

        private bool _isDecentralized;
        public bool IsDecentralized
        {
            get { return _isDecentralized; }
            set { SetProperty(ref _isDecentralized, value); }
        }

        private bool _isProton;
        public bool IsProton
        {
            get { return _isProton; }
            set { SetProperty(ref _isProton, value); }
        }

        public bool HasAttachments => Attachments.Any();

        public ObservableCollection<EmailAddress> FromList { get; } = new ObservableCollection<EmailAddress>();

        private bool _loadingContent;
        public bool LoadingContent
        {
            get { return _loadingContent; }
            set { SetProperty(ref _loadingContent, value); }
        }

        private bool _canSendMessage = true;
        public bool CanSendMessage
        {
            get
            {
                var errors = GetErrors(nameof(To));

                return _canSendMessage
                    && !errors.Any()
                    && (To.Count > 0 || UntokenizedContactTo != null);
            }
            set
            {
                SetProperty(ref _canSendMessage, value);
                SendMessageAndGoBackCommand.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<ContactItem> Contacts { get; } = new ObservableCollection<ContactItem>();

        public IRelayCommand SendMessageAndGoBackCommand { get; }

        public ICommand AttachFilesCommand => new AsyncRelayCommand<IFileOperationProvider>(PickAndAttachFilesAsync);

        public ICommand RemoveAttachmentCommand => new RelayCommand<RemovableAttachment>(RemoveAttachment);

        private async Task PickAndAttachFilesAsync(IFileOperationProvider fileOperationsProvider)
        {
            try
            {
                var loadedFiles = await fileOperationsProvider.LoadFilesAsync().ConfigureAwait(true);
                AttachPickedFiles(loadedFiles);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public NewMessagePageViewModel()
        {
            SendMessageAndGoBackCommand = new AsyncRelayCommand(SendMessageAndGoBackAsync, () => CanSendMessage);
            To.CollectionChanged += OnToCollectionChanged;
        }

        private void OnToCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SendMessageAndGoBackCommand.NotifyCanExecuteChanged();
        }

        private void RemoveAttachment(RemovableAttachment item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Attachments.Remove(item);
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                Attachments.CollectionChanged += NotifyHasAttachmentsChanged;

                await UpdateEmailAccountsListAsync().ConfigureAwait(true);

                await UpdateContactsAsync().ConfigureAwait(true);

                if (data is NewMessageData messageData)
                {
                    LoadingContent = true;
                    messageData.OnDataLoadedAsync(
                        MessageDataLoaded,
                        TaskScheduler.FromCurrentSynchronizationContext(),
                        Core.GetTextUtils());
                }

                if (!(data is DraftMessageData))
                {
                    if (From == null)
                    {
                        From = FromList.FirstOrDefault();
                    }
                }

                base.OnNavigatedTo(data);
            }
            catch (Exception ex)
            {
                if (data is ErrorReportNewMessageData errorData)
                {
                    // An error occurred while trying to send an email with a bug report.
                    // Try again using the "mailto:" protocol
                    try
                    {
                        var mailto = $"mailto:{errorData.To}?subject={errorData.Subject}&body={errorData.TextBody}";
                        await LauncherService.LaunchAsync(new Uri(mailto)).ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        ErrorHandler?.OnError(e, silent: true);
                    }
                }
                else
                {
                    OnError(ex);
                }
            }
        }

        private void MessageDataLoaded(NewMessageData messageData)
        {
            try
            {
                if (messageData != null)
                {
                    SetMessageParameters(messageData);
                }
            }
            finally
            {
                LoadingContent = false;
            }
        }

        private void SetMessageParameters(NewMessageData messageData)
        {
            From = GetFrom(messageData);
            To.SetItems(ParseContacts(messageData.To));
            Copy.SetItems(ParseContacts(messageData.Copy));
            HiddenCopy.SetItems(ParseContacts(messageData.HiddenCopy));
            Subject = messageData.Subject;
            TextBody = messageData.TextBody;

            if (messageData is DraftMessageData draftMessageData)
            {
                MessageInfo = draftMessageData.MessageInfo;

                SetAttachments(MessageInfo.Attachments);
            }
            else
            {
                if (messageData.Attachments != null)
                {
                    SetAttachments(messageData.Attachments);
                }
            }
        }

        private IEnumerable<ContactItem> ParseContacts(string text)
        {
            var emails = ParseEmails(text);

            foreach (var email in emails)
            {
                var existingContact = Contacts.FirstOrDefault(contact => contact.Email == email);
                yield return existingContact ?? new ContactItem(email);
            }
        }

        private EmailAddress GetFrom(NewMessageData messageData)
        {
            return FromList.FirstOrDefault((email) => StringHelper.AreEmailsEqual(email.Address, messageData.From.Address)) ?? FromList.First();
        }

        private async Task UpdateEmailAccountsListAsync()
        {
            var accounts = await Core.GetCompositeAccountsAsync().ConfigureAwait(true);
            FromList.SetItems(accounts.SelectMany(account => account.Addresses));
        }

        private async Task UpdateContactsAsync()
        {
            var contacts = await Core.GetContactsAsync().ConfigureAwait(true);
            Contacts.SetItems(contacts.Select(contact => new ContactItem(contact)));
        }

        private async Task SendMessageAndGoBackAsync()
        {
            try
            {
                CanSendMessage = false;

                if ((IsDecentralized || From.IsHybrid || IsEncrypted || IsSigned) && !await Core.GetSecurityManager().IsSeedPhraseInitializedAsync().ConfigureAwait(true))
                {
                    await MessageService.ShowNeedToCreateSeedPhraseMessageAsync().ConfigureAwait(true);

                    return;
                }

                await SendMessageAsync().ConfigureAwait(true);
                await GoBackAsync(false).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);

                CanSendMessage = true;
            }
        }

        private Task SendMessageAsync()
        {
            return Core.SendMessageAsync(CreateMessage(), IsEncrypted, IsSigned);
        }

        private Message CreateMessage()
        {
            var message = new Message();

            message.From.Add(From);
            message.TextBody = TextBody;
            message.HtmlBody = HtmlBody;

            message.To.AddRange(To.Select(contact => contact.ToEmailAddress()));
            if (UntokenizedContactTo != null && !message.To.Any(address => address.HasSameAddress(UntokenizedContactTo.Email)))
            {
                message.To.Add(UntokenizedContactTo.ToEmailAddress());
            }
            message.Cc.AddRange(Copy.Select(contact => contact.ToEmailAddress()));
            message.Bcc.AddRange(HiddenCopy.Select(contact => contact.ToEmailAddress()));

            message.Subject = Subject;

            message.Attachments.AddRange(Attachments.Select(attachment => attachment.ToAttachment()));

            if (MessageInfo != null)
            {
                message.Id = MessageInfo.MessageID;
                message.Folder = MessageInfo.Folder;
            }
            return message;
        }

        private static bool IsEmptyContentMessage(Message message)
        {
            var result = string.IsNullOrEmpty(message.Subject)
                         && string.IsNullOrEmpty(message.TextBody)
                         && string.IsNullOrEmpty(message.HtmlBody)
                         && !message.To.Any()
                         && !message.Cc.Any()
                         && !message.Bcc.Any()
                         && !message.Attachments.Any();

            return result;
        }

        private static IEnumerable<EmailAddress> ParseEmails(string emails)
        {
            if (emails != null)
            {
                char[] separators = { ',', ';' };
                return emails.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(email => EmailAddress.Parse(email)).Where(email => ValidateEmail(email.Address) == ValidationResult.Success);
            }

            return Enumerable.Empty<EmailAddress>();
        }

        public static ValidationResult ValidateEmail(string email, ValidationContext context = null)
        {
            return EmailValidator.Validate(email, allowTopLevelDomains: true) ? ValidationResult.Success : new ValidationResult(string.Empty);
        }

        private void AttachPickedFiles(IEnumerable<AttachedFileInfo> files)
        {
            var attachmentModelsToAdd = files?.Select(
                file => file.ToRemovableAttachment(RemoveAttachmentCommand, SaveAttachmentCommand, OpenAttachmentCommand));

            AddAttachments(attachmentModelsToAdd);
        }

        private void SetAttachments(IEnumerable<Attachment> files)
        {
            Attachments.Clear();

            var attachmentModelsToAdd = files?.Select(
                file => file.ToRemovableAttachment(RemoveAttachmentCommand, SaveAttachmentCommand, OpenAttachmentCommand));

            AddAttachments(attachmentModelsToAdd);
        }

        private void NotifyHasAttachmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasAttachments));
        }

        protected override async Task SaveDraftIfNeeded()
        {
            await UpdateDraftMessageAsync().ConfigureAwait(true);
        }

        private async Task<MessageInfo> CreateDraftMessageAsync(EmailAddress account, Message message)
        {
            if (!IsEmptyContentMessage(message))
            {
                message = await Core.CreateDraftMessageAsync(message).ConfigureAwait(true);
            }

            return new MessageInfo(account, message);
        }

        private async Task UpdateDraftMessageAsync()
        {
            if (MessageInfo?.Folder == null || MessageInfo?.Email != From)
            {
                MessageInfo = await CreateDraftMessageAsync(From, CreateMessage()).ConfigureAwait(true);
            }

            if (MessageInfo.Folder != null)
            {
                var message = CreateMessage();
                message.Id = MessageInfo.MessageID;
                message.Folder = MessageInfo.Folder;
                message.Date = DateTimeOffset.Now;

                message = await Core.UpdateDraftMessageAsync(MessageInfo.MessageID, message).ConfigureAwait(true);
                MessageInfo.MessageData = message;
            }
        }

        public void OnFromChanged()
        {
            // If the sender is a ProtonMail account, then the message encrypted and signed directly by Proton
            IsProton = Proton.Extensions.IsProton(From);
            if (IsProton)
            {
                IsSigned
                    = IsEncrypted
                    = IsDecentralized
                    = false;
            }
            else
            {
                // If the sender is a decentralized account, then the message is decentralized, encrypted, and signed by default
                IsSigned
                    = IsEncrypted
                    = IsDecentralized
                    = StringHelper.IsDecentralizedEmail(From);
            }
        }

        protected override async void ProcessMessage(LocalAIAgent agent)
        {
            try
            {
                if (!string.IsNullOrEmpty(TextBody))
                {
                    MessageInfo.AIAgentProcessedBody = GetLocalizedString("ThinkingMessage");

                    MessageInfo.MessageData.TextBody = TextBody;
                    await UpdateDraftMessageAsync().ConfigureAwait(true);

                    await AIAgentProcessMessageAsync(agent, MessageInfo).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
