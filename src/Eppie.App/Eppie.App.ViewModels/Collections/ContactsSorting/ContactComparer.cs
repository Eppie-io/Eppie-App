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

namespace Tuvi.App.ViewModels
{
    public abstract class ContactComparer : IExtendedComparer<ContactItem>
    {
        public string Label { get; set; } = "";

        public abstract int Compare(ContactItem x, ContactItem y);

        public bool Equals(ContactItem x, ContactItem y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(ContactItem obj)
        {
            return obj?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}

