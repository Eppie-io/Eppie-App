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
    public class AddressItem
    {
        internal Account Account { get; }

        public string Address => Account?.Email?.DisplayAddress;
        public string DisplayName => GetDisplayName(Account?.Email);

        public ImageInfo AvatarInfo { get; internal set; }

        public AddressItem(Account account)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }

        private static string GetDisplayName(EmailAddress email)
        {
            if (email is null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            if (!string.IsNullOrWhiteSpace(email.Name))
            {
                return email.Name;
            }

            string displayAddress = email.Address;
            int idx = displayAddress?.IndexOfAny(new char[] { '+', '@' }) ?? 0;

            if (idx >= 0)
            {
                displayAddress = displayAddress.Substring(0, idx);
            }

            return displayAddress;
        }
    }
}
