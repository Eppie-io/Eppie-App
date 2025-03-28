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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Common;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class DownloadableAttachment : Attachment
    {
        public ICommand DownloadCommand { get; set; }
        public ICommand OpenCommand { get; set; }
    }

    public static class DownloadableAttachmentConvertor
    {
        public static DownloadableAttachment ToDownloadableAttachment(this Attachment attachment, ICommand downloadCommand, ICommand openCommand)
        {
            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (downloadCommand is null)
            {
                throw new ArgumentNullException(nameof(downloadCommand));
            }

            if (openCommand is null)
            {
                throw new ArgumentNullException(nameof(openCommand));
            }

            return new DownloadableAttachment
            {
                FileName = attachment.FileName,
                Data = attachment.Data,
                DownloadCommand = downloadCommand,
                OpenCommand = openCommand
            };
        }
    }

    public class MessagePageViewModel : BaseMessageViewModel
    {
        private bool _loadingContent;
        public bool LoadingContent
        {
            get { return _loadingContent; }
            set { SetProperty(ref _loadingContent, value); }
        }

        private Task<MessageInfo> _messageLoadTask;

        public ICommand ReplyCommand => new RelayCommand(Reply);
        public ICommand ReplyCommandAll => new RelayCommand(ReplyAll);
        public ICommand ForwardCommand => new RelayCommand(Forward);
        public ICommand MarkMessageAsReadCommand => new AsyncRelayCommand(MarkMessageAsReadAsync);
        public ICommand MarkMessageAsUnReadCommand => new AsyncRelayCommand(MarkMessageAsUnReadAsync);
        public ICommand FlagMessageCommand => new AsyncRelayCommand(FlagMessageAsync);
        public ICommand UnflagMessageCommand => new AsyncRelayCommand(UnflagMessageAsync);

        public override void OnNavigatedTo(object data)
        {
            try
            {
                if (data is MessageInfo messageInfo)
                {
                    MessageInfo = messageInfo;

                    _messageLoadTask = GetMessageBodyAsync();
                }
            }
            catch (MessageIsNotExistException)
            {
                GoBackCommand.Execute(null);
            }

            base.OnNavigatedTo(data);
        }

        private async Task SetupMessageAsync()
        {
            SetAttachments();

            if (!MessageInfo.IsMarkedAsRead)
            {
                await MarkMessageAsReadAsync().ConfigureAwait(true);
            }
        }

        private void Reply()
        {
            if (MessageInfo != null)
            {
                NavigationService?.Navigate(nameof(NewMessagePageViewModel), new ReplyMessageData(MessageInfo, GetLocalizedString("MessageHeadLines"), Core.GetTextUtils(), _messageLoadTask));
            }
        }

        private void ReplyAll()
        {
            if (MessageInfo != null)
            {
                NavigationService?.Navigate(nameof(NewMessagePageViewModel), new ReplyAllMessageData(MessageInfo, GetLocalizedString("MessageHeadLines"), Core.GetTextUtils(), _messageLoadTask));
            }
        }

        private void Forward()
        {
            if (MessageInfo != null)
            {
                NavigationService?.Navigate(nameof(NewMessagePageViewModel), new ForwardMessageData(MessageInfo, GetLocalizedString("MessageHeadLines"), Core.GetTextUtils(), _messageLoadTask));
            }
        }

        private Task MarkMessageAsReadAsync()
        {
            return ApplyMessageCommandAsync(MessageInfo, m => m.MarkAsRead(), Core.MarkMessagesAsReadAsync);
        }

        private Task MarkMessageAsUnReadAsync()
        {
            return ApplyMessageCommandAsync(MessageInfo, m => m.MarkAsUnread(), Core.MarkMessagesAsUnReadAsync);
        }

        private Task FlagMessageAsync()
        {
            return ApplyMessageCommandAsync(MessageInfo, m => m.Flag(), Core.FlagMessagesAsync);
        }

        private Task UnflagMessageAsync()
        {
            return ApplyMessageCommandAsync(MessageInfo, m => m.Unflag(), Core.UnflagMessagesAsync);
        }

        private async Task ApplyMessageCommandAsync(MessageInfo message,
                                                    Action<MessageInfo> quickAction,
                                                    Func<IEnumerable<Message>, CancellationToken, Task> command)
        {
            try
            {
                quickAction(MessageInfo);
                await command(new List<Message>() { MessageInfo.MessageData }, default).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task<MessageInfo> GetMessageBodyAsync()
        {
            LoadingContent = true;

            try
            {
                MessageInfo.MessageData = await Core.GetMessageBodyHighPriorityAsync(MessageInfo.MessageData).ConfigureAwait(true);

                await SetupMessageAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                LoadingContent = false;
            }

            return MessageInfo;
        }

        private void SetAttachments()
        {
            Attachments.Clear();

            var attachments = MessageInfo.Attachments
                    .Select(attachment => attachment.ToDownloadableAttachment(SaveAttachmentCommand, OpenAttachmentCommand))
                    .ToList<Attachment>();

            AddAttachments(attachments);
        }
    }
}
