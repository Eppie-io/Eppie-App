using System;
using System.Collections.Generic;

namespace Tuvi.App.ViewModels
{
    public class DescOrderByDateMessageComparer : IExtendedComparer<MessageInfo>
    {
        public int Compare(MessageInfo x, MessageInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                int result = -DateTimeOffset.Compare(x.MessageData.Date, y.MessageData.Date);
                if (result == 0)
                {
                    result = -Comparer<uint>.Default.Compare(x.MessageID, y.MessageID);
                }

                return result;
            }
        }

        public bool Equals(MessageInfo x, MessageInfo y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(MessageInfo obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
