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

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuvi.App.ViewModels
{
    public enum AddressGroupType
    {
        OtherEmail,
        Gmail,
        Outlook,
        Proton,
        Eppie,
        Bitcoin,
        Ethereum,
    }

    public class AddressGroupItem : ObservableObject
    {

        public AddressGroupType Type { get; }
        public string GroupTitle { get; }
        public bool IsTestnet { get; }

        public ObservableCollection<AddressItem> Items { get; } = new ObservableCollection<AddressItem>();
        public bool IsAnyAddress => Items != null && Items.Count > 0;

        public bool IsLastGroup { get; internal set; }

        internal AddressGroupItem(AddressGroupType type, string groupTitle, bool isTestnet)
        {
            GroupTitle = groupTitle;
            IsTestnet = isTestnet;
            Type = type;

            Items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsAnyAddress));
        }

        internal void AddItem(AddressItem item)
        {
            Items.Add(item);
        }

        internal void SortItems()
        {
            var sortedList = Items.OrderBy(item => item.DisplayName).ThenBy(item => item.Address).ToList();
            Items.Clear();
            foreach (var sortedItem in sortedList)
            {
                AddItem(sortedItem);
            }
        }
    }
}
