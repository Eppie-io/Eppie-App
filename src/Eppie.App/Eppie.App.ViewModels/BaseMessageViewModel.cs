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
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Eppie.App.ViewModels.Helpers;
using Eppie.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels
{
    public class BaseMessageViewModel : PageViewModel
    {
        private MessageInfo _messageInfo;
        public MessageInfo MessageInfo
        {
            get { return _messageInfo; }
            set { SetProperty(ref _messageInfo, value); }
        }

        private bool _isLocalAIEnabled;
        public bool IsLocalAIEnabled
        {
            get => _isLocalAIEnabled;
            private set => SetProperty(ref _isLocalAIEnabled, value);
        }

        public ObservableCollection<Attachment> Attachments { get; } = new ObservableCollection<Attachment>();

        public string AttachmentsHeader => Attachments.Count == 0 ? string.Empty : string.Format(
            CultureInfo.CurrentCulture,
            GetLocalizedString("AttachmentsHeader"),
            FileSizeFormatter.Format(Attachments.Sum(attachment => attachment.Data.LongLength)));

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
            Attachments.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(AttachmentsHeader));
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
            IsLocalAIEnabled = await AIService.IsEnabledAsync().ConfigureAwait(true);
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

        private async Task OpenAttachmentFileAsync(Tuple<Attachment, IFileOperationProvider> pair)
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

                await fileOperationsProvider.SaveToTempFileAndOpenAsync(attachment.Data, attachment.FileName).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
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

        protected virtual Task SaveDraftIfNeededAsync()
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
                    await SaveDraftIfNeededAsync().ConfigureAwait(true);
                }

                NavigationService?.GoBack();
            }
            catch (Exception e)
            {
                CanGoBack = true;
                OnError(e);
            }
        }

        public override async Task CreateAIAgentsMenuAsync(Action<string, Action> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var agents = await AIService.GetAgentsAsync().ConfigureAwait(true);
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
