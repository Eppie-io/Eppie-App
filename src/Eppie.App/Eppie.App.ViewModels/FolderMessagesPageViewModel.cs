using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class FolderMessagesPageViewModel : MessagesViewModel
    {
        private EmailAddress _email;
        public EmailAddress Email
        {
            get { return _email; }
            set
            {
                SetProperty(ref _email, value);
                OnPropertyChanged(nameof(FolderPath));
            }
        }

        private CompositeFolder _folder;
        public CompositeFolder Folder
        {
            get { return _folder; }
            set
            {
                SetProperty(ref _folder, value);
                OnPropertyChanged(nameof(FolderPath));
            }
        }

        public string FolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(Email?.Address))
                {
                    return "";
                }
                else if (string.IsNullOrEmpty(Folder?.FullName))
                {
                    return Email.Address;
                }
                else
                {
                    return $"{Email.Address}/{Folder?.FullName}";
                }
            }
        }

        public override void OnNavigatedTo(object data)
        {
            if (data is MailBoxItem mailBoxItem)
            {
                Email = mailBoxItem.Email;
                Folder = mailBoxItem.Folder;
            }

            base.OnNavigatedTo(data);
        }

        protected override IEnumerable<MessageInfo> SelectAppropriateMessagesFrom(List<ReceivedMessageInfo> receivedMessages)
        {
            return receivedMessages.Where(m => m.Email.HasSameAddress(Email) && m.Folder.HasSameName(Folder.FullName))
                                   .Select(m => new MessageInfo(m.Email, m.Message));
        }

        protected override Task<IReadOnlyList<Message>> LoadMoreMessagesAsync(int count, Message lastMessage, CancellationToken cancellationToken)
        {
            return Core.GetFolderEarlierMessagesAsync(Folder, count, lastMessage, cancellationToken);
        }

        protected override async Task RefreshMessagesAsync()
        {
            await Core.CheckForNewMessagesInFolderAsync(Folder, CancellationTokenSource.Token).ConfigureAwait(true);
        }
    }
}
