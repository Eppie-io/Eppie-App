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

using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuvi.App.ViewModels
{
    public class NavPanelTabModel : ObservableObject
    {
        private ContactsModel _contactsModel;
        public ContactsModel ContactsModel
        {
            get => _contactsModel;
            set
            {
                SetProperty(ref _contactsModel, value);
                OnPropertyChanged(nameof(ItemModels));
            }
        }

        private MailBoxesModel _mailBoxesModel;
        public MailBoxesModel MailBoxesModel
        {
            get => _mailBoxesModel;
            set
            {
                SetProperty(ref _mailBoxesModel, value);
                OnPropertyChanged(nameof(ItemModels));
            }
        }

        private IControlModel _selectedItemModel;
        public IControlModel SelectedItemModel
        {
            get => _selectedItemModel;
            set => SetProperty(ref _selectedItemModel, value);
        }

        public List<IControlModel> ItemModels
        {
            get => new List<IControlModel>() { ContactsModel, MailBoxesModel };
        }


        public NavPanelTabModel(ContactsModel contactsModel, MailBoxesModel mailBoxesModel)
        {
            ContactsModel = contactsModel;
            MailBoxesModel = mailBoxesModel;

            SelectedItemModel = ContactsModel;
        }
    }
}
