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

#pragma warning disable CA1309 // Use ordinal string comparison
            // This comparison is intended to be culture-aware because the list is shown to the user.
            var result = string.Compare(xName, yName, StringComparison.CurrentCultureIgnoreCase);
#pragma warning restore CA1309
            if (result == 0)
            {
                result = StringHelper.CompareEmails(x.Email.Address, y.Email.Address);
            }

            return result;
        }
    }
}
