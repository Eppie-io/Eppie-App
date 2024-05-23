using System;

namespace Tuvi.App.ViewModels
{
    public class ByUnreadContactComparer : ByTimeContactComparer
    {
        public ByUnreadContactComparer()
        {
        }
        public ByUnreadContactComparer(string label)
        {
            Label = label;
        }

        public override int Compare(ContactItem x, ContactItem y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }
            if (y is null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (x.UnreadMessagesCount == 0 && y.UnreadMessagesCount > 0)
            {
                return 1;
            }
            else if (x.UnreadMessagesCount > 0 && y.UnreadMessagesCount == 0)
            {
                return -1;
            }
            else
            {
                return base.Compare(x, y);
            }
        }
    }
}
