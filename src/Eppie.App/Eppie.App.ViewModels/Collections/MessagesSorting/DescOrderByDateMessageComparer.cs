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
