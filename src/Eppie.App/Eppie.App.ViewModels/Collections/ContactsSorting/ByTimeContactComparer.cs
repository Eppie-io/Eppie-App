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

            var xDate = x.LastMessageData?.Date ?? DateTimeOffset.MinValue;
            var yDate = y.LastMessageData?.Date ?? DateTimeOffset.MinValue;

            var result = DateTimeOffset.Compare(yDate, xDate);
            if (result == 0)
            {
                result = StringHelper.CompareEmails(x.Email.Address, y.Email.Address);
            }

            return result;
        }
    }
}
