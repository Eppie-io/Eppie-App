using System;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ByNameContactComparer : ContactComparer
    {
        public ByNameContactComparer()
        {
        }
        public ByNameContactComparer(string label)
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

            var xName = string.IsNullOrEmpty(x.FullName) ? x.Email.Address : x.FullName;
            var yName = string.IsNullOrEmpty(y.FullName) ? y.Email.Address : y.FullName;

            var result = string.Compare(xName, yName, StringComparison.CurrentCultureIgnoreCase);
            if (result == 0)
            {
                result = StringHelper.CompareEmails(x.Email.Address, y.Email.Address);
            }

            return result;
        }
    }
}
