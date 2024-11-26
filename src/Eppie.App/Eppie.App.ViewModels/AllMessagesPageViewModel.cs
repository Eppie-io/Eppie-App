using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class AllMessagesPageViewModel : MessagesViewModel
    {
        public class NavigationData
        {
            public IErrorHandler ErrorHandler { get; set; }
        }

        private IErrorHandler PageErrorHandler { get; set; }

        public override void OnNavigatedTo(object data)
        {
            if (data is NavigationData navigationData)
            {
                PageErrorHandler = navigationData.ErrorHandler;
            }

            base.OnNavigatedTo(data);
        }

        public override void OnError(Exception e)
        {
            PageErrorHandler?.OnError(e, false);
        }

        protected override IEnumerable<MessageInfo> SelectAppropriateMessagesFrom(List<ReceivedMessageInfo> receivedMessages)
        {
            return receivedMessages.Where(m => m.Folder.IsInbox)
                                   .Select(m => new MessageInfo(m.Email, m.Message));
        }

        protected override Task<IReadOnlyList<Message>> LoadMoreMessagesAsync(int count, Message lastMessage, CancellationToken cancellationToken)
        {
            return Core.GetAllEarlierMessagesAsync(count, lastMessage, cancellationToken);
        }

        protected override async Task RefreshMessagesAsync()
        {
            try
            {
                await Core.CheckForNewInboxMessagesAsync(CancellationTokenSource.Token).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
