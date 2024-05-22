using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

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
