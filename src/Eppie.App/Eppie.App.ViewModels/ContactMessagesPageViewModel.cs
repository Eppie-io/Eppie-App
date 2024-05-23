using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ContactMessagesPageViewModel : MessagesViewModel
    {
        private ContactItem _contactItem;
        public ContactItem ContactItem
        {
            get { return _contactItem; }
            set
            {
                SetProperty(ref _contactItem, value);
            }
        }

        public override void OnNavigatedTo(object data)
        {
            if (data is ContactItem contactItem)
            {
                ContactItem = contactItem;
            }

            base.OnNavigatedTo(data);
        }

        protected override IEnumerable<MessageInfo> SelectAppropriateMessagesFrom(List<ReceivedMessageInfo> receivedMessages)
        {
            if (ContactItem?.Email != null)
            {
                return receivedMessages.Where(m => m.Message != null
                                                && m.Message.IsFromCorrespondenceWithContact(m.Email, ContactItem.Email)
                                                && !m.Folder.IsJunk
                                                && !m.Folder.IsTrash)
                                       .Select(m => new MessageInfo(m.Email, m.Message));
            }
            else
            {
                return receivedMessages.Select(m => new MessageInfo(m.Email, m.Message));
            }
        }

        protected override Task<IReadOnlyList<Message>> LoadMoreMessagesAsync(int count, Message lastMessage, CancellationToken cancellationToken)
        {
            return Core.GetContactEarlierMessagesAsync(ContactItem.Email, count, lastMessage, cancellationToken);
        }
    }
}
