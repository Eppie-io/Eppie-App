using System;
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


        public MailBoxesModel(ICommand itemClickCommand, ICommand itemDropCommand)
        {
            ItemClickCommand = itemClickCommand;
            ItemDropCommand = itemDropCommand;
        }


        public void SetAccounts(IReadOnlyList<CompositeAccount> accounts)
        {
            AccountList.Clear();
            AccountList.AddRange(accounts);

            RefreshFolderStructure();
        }

        private void RefreshFolderStructure()
        {
            Items.Clear();

            foreach (var accountData in AccountList)
            {
                var rootItem = GetRootItemByEmail(accountData.Email);
                if (rootItem == null)
                {
                    AddRootItem(accountData);
                }
                else
                {
                    foreach (var folder in accountData.FoldersStructure)
                    {
                        var folderItem = rootItem.Children.FirstOrDefault(item => item.Folder.HasSameName(folder));
                        if (folderItem == null)
                        {
                            AddFolderItem(rootItem, accountData.Email, folder);
                        }
                    }
                }
            }
        }

        private void AddRootItem(CompositeAccount account)
        {
            var rootItem = new MailBoxItem(account.Email, account.DefaultInboxFolder, account.Email.Address, true);
            foreach (var folder in account.FoldersStructure)
            {
                AddFolderItem(rootItem, account.Email, folder);
            }
            Items.Add(rootItem);
        }

        private void AddFolderItem(MailBoxItem rootItem, EmailAddress accountEmail, CompositeFolder folder)
        {
            var folderItem = new MailBoxItem(accountEmail, folder, folder.FullName, false);
            rootItem.Children.Add(folderItem);
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
