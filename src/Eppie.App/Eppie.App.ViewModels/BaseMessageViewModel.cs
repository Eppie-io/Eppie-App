using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class BaseMessageViewModel : BaseViewModel
    {
        private MessageInfo _messageInfo;
        public MessageInfo MessageInfo
        {
            get { return _messageInfo; }
            set
            {
                if (SetProperty(ref _messageInfo, value))
                {
                    SubscribeOnAttachmentsChange(_messageInfo);
                }
            }
        }

        public bool IsLocalAIAvailable => AIService.IsAvailable();

        private bool _isLocalAIEnabled;
        public bool IsLocalAIEnabled
        {
            get => _isLocalAIEnabled;
            private set => SetProperty(ref _isLocalAIEnabled, value);
        }

        public ObservableCollection<Attachment> Attachments { get; } = new ObservableCollection<Attachment>();

        private bool _canDelete = true;
        public bool CanDelete
        {
            get { return _canDelete; }
            set
            {
                SetProperty(ref _canDelete, value);
                DeleteMessageAndGoBackCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _canGoBack = true;
        public bool CanGoBack
        {
            get { return _canGoBack; }
            set
            {
                SetProperty(ref _canGoBack, value);
                GoBackCommand.NotifyCanExecuteChanged();
            }
        }

        public IRelayCommand GoBackCommand { get; }

        public IRelayCommand DeleteMessageAndGoBackCommand { get; }

        public ICommand SaveAttachmentCommand => new AsyncRelayCommand<Tuple<Attachment, IFileOperationProvider>>(SaveAttachmentFileAsync);

        public ICommand OpenAttachmentCommand => new AsyncRelayCommand<Tuple<Attachment, IFileOperationProvider>>(OpenAttachmentFileAsync);

        public BaseMessageViewModel()
        {
            GoBackCommand = new AsyncRelayCommand(() => GoBackAsync(true), () => CanGoBack);
            DeleteMessageAndGoBackCommand = new AsyncRelayCommand(DeleteMessageAndGoBackAsync, () => CanDelete);
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                await InitializeAIButtonAsync().ConfigureAwait(true);

                base.OnNavigatedTo(data);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task InitializeAIButtonAsync()
        {
            IsLocalAIEnabled = await AIService.IsEnabledAsync();
        }

        protected async Task DeleteMessageAndGoBackAsync()
        {
            try
            {
                CanDelete = false;

                if (MessageInfo?.Folder != null)
                {
                    await Core.DeleteMessagesAsync(new List<Message>() { MessageInfo.MessageData }).ConfigureAwait(true);
                }

                await GoBackAsync(false).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                CanDelete = true;
                OnError(e);
            }
        }

        private void SubscribeOnAttachmentsChange(MessageInfo messageInfo)
        {
            messageInfo.PropertyChanged += NotifyOnAttachmentsChange;
        }

        private void NotifyOnAttachmentsChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MessageInfo.Attachments))
            {
                OnPropertyChanged(nameof(Attachments));
            }
        }

        private Task SaveAttachmentFileAsync(Tuple<Attachment, IFileOperationProvider> pair)
        {
            try
            {
                Attachment attachment = pair.Item1;
                IFileOperationProvider fileOperationsProvider = pair.Item2;

                if (fileOperationsProvider is null)
                {
                    throw new ArgumentNullException(nameof(pair), nameof(fileOperationsProvider));
                }

                if (attachment is null)
                {
                    throw new ArgumentNullException(nameof(pair), nameof(attachment));
                }

                return fileOperationsProvider.SaveToFileAsync(attachment.Data, attachment.FileName);
            }
            catch (Exception e)
            {
                OnError(e);
            }

            return Task.CompletedTask;
        }

        private Task OpenAttachmentFileAsync(Tuple<Attachment, IFileOperationProvider> pair)
        {
            try
            {
                Attachment attachment = pair.Item1;
                IFileOperationProvider fileOperationsProvider = pair.Item2;

                if (fileOperationsProvider is null)
                {
                    throw new ArgumentNullException(nameof(pair), nameof(fileOperationsProvider));
                }

                if (attachment is null)
                {
                    throw new ArgumentNullException(nameof(pair), nameof(attachment));
                }

                return fileOperationsProvider.SaveToTempFileAndOpenAsync(attachment.Data, attachment.FileName);
            }
            catch (Exception e)
            {
                OnError(e);
            }

            return Task.CompletedTask;
        }

        protected void AddAttachments(IEnumerable<Attachment> attachments)
        {
            if (attachments is null)
            {
                throw new ArgumentNullException(nameof(attachments));
            }

            foreach (var attachment in attachments)
            {
                Attachments.Add(attachment);
            }
        }

        protected virtual Task SaveDraftIfNeeded()
        {
            return Task.CompletedTask;
        }

        protected async Task GoBackAsync(bool saveDraftIfNeeded)
        {
            try
            {
                CanGoBack = false;

                if (saveDraftIfNeeded)
                {
                    await SaveDraftIfNeeded().ConfigureAwait(true);
                }

                NavigationService?.GoBackOrNavigate(nameof(MainPageViewModel));
            }
            catch (Exception e)
            {
                CanGoBack = true;
                OnError(e);
            }
        }

        public override async Task CreateAIAgentsMenuAsync(Action<string, Action> action)
        {
            var agents = await AIService.GetAgentsAsync();
            foreach (var agent in agents)
            {
                action(agent.Name, () => ProcessMessage(agent));
            }
        }

        protected virtual async void ProcessMessage(LocalAIAgent agent)
        {
            try
            {
                await AIAgentProcessMessageAsync(agent, MessageInfo).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
