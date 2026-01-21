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
        public ObservableCollection<AddressItem> Recipients { get; } = new ObservableCollection<AddressItem>();
        public ObservableCollection<AddressItem> SuitableContacts { get; } = new ObservableCollection<AddressItem>();
        public IList<AddressItem> SenderAddresses { get; } = new List<AddressItem>();
        public IList<AddressItem> EppieAddresses { get; } = new List<AddressItem>();

        private int _senderAddressIndex;
        public int SenderAddressIndex
        {
            get { return _senderAddressIndex; }
            set
            {
                SetProperty(ref _senderAddressIndex, value);
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

        public bool IsAnyRecipient => Recipients.Count > 0;
        public bool CanInvite => IsAnyRecipient && SenderAddressIndex != -1 && EppieAddressIndex != -1;

        public ICommand SendInviteCommand => new RelayCommand(SendInvite, () => CanInvite);
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

        public void OnRecipientRemoved(AddressItem item)
        {
            // ToDo: update Recipients

            // ToDo: Test code should be removed
            Recipients.Remove(item);
            OnPropertyChanged(nameof(IsAnyRecipient));
            OnPropertyChanged(nameof(CanInvite));
        }

        public void OnContactQuerySubmitted(AddressItem queryItem, string queryText)
        {
            // ToDo: update Recipients

            // ToDo: Test code should be removed
            if (queryItem != null)
            {
                Recipients.Add(queryItem);
            }
            else if (!string.IsNullOrEmpty(queryText))
            {
                Recipients.Add(CreateFakeAddressItem(queryText, null));
            }

            OnPropertyChanged(nameof(IsAnyRecipient));
            OnPropertyChanged(nameof(CanInvite));
        }

        public void OnContactQueryChanged(string queryText)
        {
            // ToDo: update SuitableContacts

            // ToDo: Test code should be removed
            if (SuitableContacts.Count < 10)
            {
                SuitableContacts.Add(CreateFakeAddressItem("eva@gmail.com", "Eva"));
            }
        }

        // ToDo: Test code should be removed
        private void InitFakeData()
        {
            var from = new List<AddressItem>()
            {
                CreateFakeAddressItem("bob@gmail.com","Bob"),
                CreateFakeAddressItem("alice@gmail.com","Alice"),
                CreateFakeAddressItem("very-very-very-huge-and-vast-mail-address@gmail.com","VeryVeryVeryHugeAndVastName VeryVeryVeryHugeAndVastSurname"),
            };

            Copy(from, SenderAddresses);
            Copy(from, SuitableContacts);
            Copy(from, Recipients);
            Copy(from, EppieAddresses);

            SenderAddressIndex = 1;
            EppieAddressIndex = 2;

            void Copy(ICollection<AddressItem> source, ICollection<AddressItem> target)
            {
                target.Clear();

                foreach (var item in source)
                {
                    target.Add(item);
                }
            }

            OnPropertyChanged(nameof(IsAnyRecipient));
            OnPropertyChanged(nameof(CanInvite));
        }

        // ToDo: Test code should be removed
        private static AddressItem CreateFakeAddressItem(string address, string name)
        {
            return new AddressItem(new Tuvi.Core.Entities.Account() { Email = new Core.Entities.EmailAddress(address, name) });
        }
    }
}
