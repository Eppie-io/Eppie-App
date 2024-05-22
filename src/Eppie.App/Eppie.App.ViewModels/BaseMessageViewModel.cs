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

        public ICommand SaveAttachmentCommand => new AsyncRelayCommand<Tuple<IFileOperationProvider, Attachment>>(SaveAttachmentFileAsync);

        public ICommand OpenAttachmentCommand => new AsyncRelayCommand<Tuple<IFileOperationProvider, Attachment>>(OpenAttachmentFileAsync);

        public BaseMessageViewModel()
        {
            GoBackCommand = new AsyncRelayCommand(() => GoBackAsync(true), () => CanGoBack);
            DeleteMessageAndGoBackCommand = new AsyncRelayCommand(DeleteMessageAndGoBackAsync, () => CanDelete);
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

        private Task SaveAttachmentFileAsync(Tuple<IFileOperationProvider, Attachment> pair)
        {
            try
            {
                var fileOperationsProvider = pair.Item1;
                var attachment = pair.Item2;

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

        private Task OpenAttachmentFileAsync(Tuple<IFileOperationProvider, Attachment> pair)
        {
            try
            {
                var fileOperationsProvider = pair.Item1;
                var attachment = pair.Item2;

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
    }
}
