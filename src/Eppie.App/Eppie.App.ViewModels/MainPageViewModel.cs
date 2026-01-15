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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Messages;
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
        public MailBoxesModel MailBoxesModel { get; private set; }

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

                    var request = new RequestSelectedContactMessage();
                    WeakReferenceMessenger.Default.Send(request);
                    var selectedContact = request.Response;

                    if (selectedContact?.Email != null)
                    {
                        messageData = new SelectedContactNewMessageData(
                            selectedContact.LastMessageData.AccountEmail,
                            selectedContact.Email);
                    }
                    else if (MailBoxesModel?.SelectedItem != null)
                    {
                        messageData = new SelectedAccountNewMessageData(MailBoxesModel.SelectedItem.Email);
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

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                base.OnNavigatedTo(data);

                await UpdateAccountListAsync().ConfigureAwait(true);

                SubscribeEvents();

                UpdateUnreadCounts();

                // Test node URI
                const string downloadUrl = "https://testnet.eppie.io/api/DownloadBackupFunction?code=1&name=";
                // Local node URI
                //const string downloadUrl = "http://localhost:7071/api/DownloadBackupFunction?name=";

                await Core.RestoreFromBackupIfNeededAsync(new Uri(downloadUrl)).ConfigureAwait(true);

                // Startup auto-popups: show at most one per run
                _ = ShowOneStartupPopupIfNeededAsync();

                LogEnabledWarning();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private static bool _startupPopupAttempted;
        private async Task ShowOneStartupPopupIfNeededAsync()
        {
            if (_startupPopupAttempted)
            {
                return;
            }

            _startupPopupAttempted = true;

            await UpdateSupportDevelopmentButtonAsync().ConfigureAwait(true);

            //1) WhatsNew has priority
            if (await ShowWhatsNewIfNeededAsync().ConfigureAwait(true))
            {
                return;
            }

            //2) Show rate-app prompt if eligible
            if (await ShowRateAppIfNeededAsync().ConfigureAwait(true))
            {
                return;
            }

            //3) Show Support Development if eligible
            if (await ShowSupportDevelopmentIfNeededAsync().ConfigureAwait(true))
            {
                return;
            }
        }

        private async Task<bool> ShowWhatsNewIfNeededAsync()
        {
            const string WhatsNewCurrentId = "2025-11-10"; // Update when WhatsNew content changes
            if (!string.Equals(LocalSettingsService.LastShownWhatsNewId, WhatsNewCurrentId, StringComparison.Ordinal))
            {
                await MessageService.ShowWhatsNewDialogAsync(
                    version: BrandService.GetAppVersion(),
                    isStorePaymentProcessor: IsStorePaymentProcessor,
                    isSupportDevelopmentButtonVisible: IsSupportDevelopmentButtonVisible,
                    price: SupportDevelopmentPrice,
                    supportDevelopmentCommand: SupportDevelopmentCommand,
                    twitterUrl: BrandService.GetGitHub()
                ).ConfigureAwait(true);

                LocalSettingsService.LastShownWhatsNewId = WhatsNewCurrentId;

                return true;
            }

            return false;
        }

        private async Task<bool> ShowRateAppIfNeededAsync()
        {
            const int ReviewRequestsThreshold = 5;
            const int ReviewRequestsDisabled = -1;

            var count = LocalSettingsService.RequestReviewCount;

            if (count > ReviewRequestsThreshold)
            {
                LocalSettingsService.RequestReviewCount = ReviewRequestsDisabled;

                if (await MessageService.ShowRequestReviewMessageAsync().ConfigureAwait(true))
                {
                    try
                    {
                        await AppStoreService.RequestReviewAsync().ConfigureAwait(true);
                    }
                    catch (NotImplementedException)
                    {
                        await LauncherService.LaunchAsync(new Uri(BrandService.GetGitHub())).ConfigureAwait(true);
                    }
                }

                return true;
            }
            else if (count != ReviewRequestsDisabled)
            {
                LocalSettingsService.RequestReviewCount++;
            }

            return false;
        }

        private async Task<bool> ShowSupportDevelopmentIfNeededAsync()
        {
            const int DevelopmentSupportThreshold = 5;

            var count = LocalSettingsService.DevelopmentSupportRequestCount;

            if (count >= DevelopmentSupportThreshold && IsSupportDevelopmentButtonVisible)
            {
                LocalSettingsService.DevelopmentSupportRequestCount = 0;

                await MessageService.ShowSupportDevelopmentDialogAsync(
                    isStorePaymentProcessor: IsStorePaymentProcessor,
                    price: SupportDevelopmentPrice,
                    supportDevelopmentCommand: SupportDevelopmentCommand
                ).ConfigureAwait(true);

                return true;
            }
            else
            {
                LocalSettingsService.DevelopmentSupportRequestCount++;
            }

            return false;
        }

        private async void UpdateUnreadCounts()
        {
            try
            {
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

        public async void UpdateAccountsList()
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

        public void InitializeMailboxModel(ICommand mailBoxItemClick, ICommand mailBoxItemDrop)
        {
            MailBoxesModel = new MailBoxesModel(mailBoxItemClick, mailBoxItemDrop);
        }

        public void OnShowAllMessages()
        {
            WeakReferenceMessenger.Default.Send(new ClearSelectedContactMessage());
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

        public bool MailBoxItemIsDropMessagesAllowed(MailBoxItem item)
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

        private void ShowLanguageChangeWarning()
        {
            var title = GetLocalizedString("WarningProblemTitle");
            var solution = GetLocalizedString("RestartApp");

            var messageTemplate = GetLocalizedString("RestartApplication");
            var brandName = BrandService.GetName();
            var message = string.Format(CultureInfo.CurrentCulture, messageTemplate, brandName);

            var existing = Problems.FirstOrDefault(x => x.Title == title && x.SolutionText == solution && x.Email is null);
            if (existing is null)
            {
                Problems.Add(new Problem()
                {
                    ViewModel = this,
                    Title = title,
                    SolutionText = solution,
                    Email = null,
                    Message = message,
                    ActionCommand = new RelayCommand<Problem>((p) =>
                    {
                        CloseProblem(p);
                        NavigationService.ExitApplication();
                    })
                });
            }
            else
            {
                existing.Message = message;
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

                if (args.Name == nameof(LocalSettingsService.Language))
                {
                    ShowLanguageChangeWarning();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
