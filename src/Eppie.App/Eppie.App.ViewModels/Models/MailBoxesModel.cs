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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class MailBoxesModel : ObservableObject, IControlModel
    {
        public List<CompositeAccount> AccountList { get; } = new List<CompositeAccount>();

        private ObservableCollection<MailBoxItem> _items = new ObservableCollection<MailBoxItem>();
        public ObservableCollection<MailBoxItem> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        private MailBoxItem _selectedItem;
        public MailBoxItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private ICommand _itemClickCommand;
        public ICommand ItemClickCommand
        {
            get { return _itemClickCommand; }
            set { SetProperty(ref _itemClickCommand, value); }
        }

        private ICommand _itemDropCommand;
        public ICommand ItemDropCommand
        {
            get { return _itemDropCommand; }
            set { SetProperty(ref _itemDropCommand, value); }
        }

        private ICommand _newFolderCommand;
        public ICommand NewFolderCommand
        {
            get { return _newFolderCommand; }
            set { SetProperty(ref _newFolderCommand, value); }
        }

        private ICommand _mailboxSettingsCommand;
        public ICommand MailboxSettingsCommand
        {
            get { return _mailboxSettingsCommand; }
            set { SetProperty(ref _mailboxSettingsCommand, value); }
        }

        private ICommand _removeMailboxCommand;
        public ICommand RemoveMailboxCommand
        {
            get { return _removeMailboxCommand; }
            set { SetProperty(ref _removeMailboxCommand, value); }
        }

        private ICommand _renameFolderCommand;
        public ICommand RenameFolderCommand
        {
            get { return _renameFolderCommand; }
            set { SetProperty(ref _renameFolderCommand, value); }
        }

        private ICommand _deleteFolderCommand;
        public ICommand DeleteFolderCommand
        {
            get { return _deleteFolderCommand; }
            set { SetProperty(ref _deleteFolderCommand, value); }
        }


        public MailBoxesModel(ICommand itemClickCommand, ICommand itemDropCommand)
        {
            ItemClickCommand = itemClickCommand;
            ItemDropCommand = itemDropCommand;
        }


        public void SetAccounts(IReadOnlyList<CompositeAccount> accounts)
        {
            EmailAddress prevSelectedRootEmail = null;
            if (SelectedItem != null)
            {
                prevSelectedRootEmail = SelectedItem.Email;
            }

            AccountList.Clear();
            AccountList.AddRange(accounts);

            RefreshFolderStructure();

            if (prevSelectedRootEmail != null)
            {
                SelectedItem = GetRootItemByEmail(prevSelectedRootEmail);
            }
        }

        private void RefreshFolderStructure()
        {
            var newItems = new ObservableCollection<MailBoxItem>();

            foreach (var accountData in AccountList)
            {
                var rootItem = new MailBoxItem(accountData.Email, accountData.DefaultInboxFolder, accountData.Email.DisplayAddress, true);

                foreach (var folder in accountData.FoldersStructure)
                {
                    var folderItem = new MailBoxItem(accountData.Email, folder, folder.FullName, false);
                    rootItem.Children.Add(folderItem);
                }

                newItems.Add(rootItem);
            }

            Items = newItems;
        }

        public MailBoxItem GetRootItemByEmail(EmailAddress email)
        {
            return Items.FirstOrDefault(item => item.Email.HasSameAddress(email));
        }

        public void ItemDrop(MailBoxItem targetMailBoxItem)
        {
            ItemDropCommand.Execute(targetMailBoxItem);
        }

        public bool IsDropAllowed(MailBoxItem hoveredMailBoxItem)
        {
            return ItemDropCommand.CanExecute(hoveredMailBoxItem);
        }
    }
}
