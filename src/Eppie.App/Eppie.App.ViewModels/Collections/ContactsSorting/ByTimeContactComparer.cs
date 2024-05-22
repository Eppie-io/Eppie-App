using System;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ByTimeContactComparer : ContactComparer
    {
        public ByTimeContactComparer()
        {
        }
        public ByTimeContactComparer(string label)
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

            var result = DateTimeOffset.Compare(y.LastMessageData.Date, x.LastMessageData.Date);
            if (result == 0)
            {
                result = StringHelper.CompareEmails(x.Email.Address, y.Email.Address);
            }

            return result;
        }
    }
}
