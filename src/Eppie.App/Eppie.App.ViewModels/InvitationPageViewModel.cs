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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Tuvi.App.ViewModels
{

    public class InvitationPageViewModel : BaseViewModel
    {
        public ObservableCollection<AddressItem> InvitedContacts { get; } = new ObservableCollection<AddressItem>();
        public ObservableCollection<AddressItem> SuggestedContacts { get; } = new ObservableCollection<AddressItem>();
        public IList<AddressItem> FromAddresses { get; } = new List<AddressItem>();
        public IList<AddressItem> EppieAddresses { get; } = new List<AddressItem>();
        public bool IsAnyoneInvited => InvitedContacts.Count > 0;

        private int _fromAddressIndex;
        public int FromAddressIndex
        {
            get { return _fromAddressIndex; }
            set
            {
                SetProperty(ref _fromAddressIndex, value);
            }
        }

        private int _eppieAddressIndex;
        public int EppieAddressIndex
        {
            get { return _eppieAddressIndex; }
            set
            {
                SetProperty(ref _eppieAddressIndex, value);
            }
        }

        public ICommand SendInviteCommand => new RelayCommand(SendInvite);
        public Action ClosePopupAction { get; set; }


        public InvitationPageViewModel() : base()
        {
            InitFakeData();
        }

        public override void OnNavigatedTo(object data)
        {
            base.OnNavigatedTo(data);
        }

        private void SendInvite()
        {
            ClosePopupAction?.Invoke();
            // ToDo: send invite
        }

        public void OnInvitedAddressRemoved(AddressItem item)
        {
            // ToDo: update InvitedContacts

            // ToDo: Test code should be removed
            InvitedContacts.Remove(item);
            OnPropertyChanged(nameof(IsAnyoneInvited));
        }

        public void OnContactQuerySubmitted(AddressItem queryItem, string queryText)
        {
            // ToDo: update InvitedContacts

            // ToDo: Test code should be removed
            if (queryItem != null)
            {
                InvitedContacts.Add(queryItem);
            }
            else if (!string.IsNullOrEmpty(queryText))
            {
                InvitedContacts.Add(CreateFakeAddressItem(queryText, null));
            }

            OnPropertyChanged(nameof(IsAnyoneInvited));
        }

        public void OnContactQueryChanged(string queryText)
        {
            // ToDo: update SuggestedContacts

            // ToDo: Test code should be removed
            if (SuggestedContacts.Count < 10)
            {
                SuggestedContacts.Add(CreateFakeAddressItem("eva@gmail.com", "Eva"));
            }
        }

        // ToDo: Test code should be removed
        private void InitFakeData()
        {
            var from = new List<AddressItem>()
            {
                CreateFakeAddressItem("bob@gmail.com","Bob"),
                CreateFakeAddressItem("alice@gmail.com","Alice"),
                CreateFakeAddressItem("very-very-very-huge-and-vast-mail-address@gmail.com","VeryVeryVeryHugeAndVastName"),
            };

            Copy(from, FromAddresses);
            Copy(from, SuggestedContacts);
            Copy(from, InvitedContacts);
            Copy(from, EppieAddresses);

            FromAddressIndex = 1;
            EppieAddressIndex = 2;

            void Copy(ICollection<AddressItem> source, ICollection<AddressItem> target)
            {
                target.Clear();

                foreach (var item in source)
                {
                    target.Add(item);
                }
            }

            OnPropertyChanged(nameof(IsAnyoneInvited));
        }

        // ToDo: Test code should be removed
        private static AddressItem CreateFakeAddressItem(string address, string name)
        {
            return new AddressItem(new Tuvi.Core.Entities.Account() { Email = new Core.Entities.EmailAddress(address, name) });
        }
    }
}
