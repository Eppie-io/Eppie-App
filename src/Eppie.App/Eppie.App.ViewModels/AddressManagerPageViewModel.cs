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
using System.Linq;
using System.Threading.Tasks;
using Tuvi.Core.Entities;
using Tuvi.OAuth2;

namespace Tuvi.App.ViewModels
{
    public class AddressManagerPageViewModel : BaseViewModel
    {
        public ObservableCollection<AddressGroupItem> GroupItems { get; } = new ObservableCollection<AddressGroupItem>();

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                await UpdateAccountsAsync().ConfigureAwait(true);

                // Todo: We could ignore these events if the mailbox settings pages were opened inside
                // the same `contentFrame` frame as `AddressManagerPage` page (look at file: MainPage.xaml; line 243)
                // Now NavigationService use main frame, so we need to handle events.
                Core.AccountAdded += OnAccountAdded;
                Core.AccountDeleted += OnAccountDeleted;
                Core.AccountUpdated += OnAccountUpdated;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public override void OnNavigatedFrom()
        {
            Core.AccountAdded -= OnAccountAdded;
            Core.AccountDeleted -= OnAccountDeleted;
            Core.AccountUpdated -= OnAccountUpdated;
        }

        public void NavigateToAddressSettingsPage(AddressItem address)
        {
            if (address?.Account != null)
            {
                NavigateToMailboxSettingsPage(address.Account, isReloginNeeded: false);
            }
        }

        public async void NavigateToAddAddressPage(AddressGroupType type)
        {
            try
            {
                switch (type)
                {
                    case AddressGroupType.Proton:
                        await MessageService.ShowProtonConnectAddressDialogAsync().ConfigureAwait(true);
                        return;
                    case AddressGroupType.Eppie:
                        NavigationService?.Navigate(nameof(EppieAddressSettingsPageViewModel));
                        return;
                    case AddressGroupType.Bitcoin:
                        NavigationService?.Navigate(nameof(BitcoinAddressSettingsPageViewModel));
                        return;
                    case AddressGroupType.Ethereum:
                        NavigationService?.Navigate(nameof(EthereumAddressSettingsPageViewModel));
                        return;
                    case AddressGroupType.Gmail:
                        NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), MailService.Gmail);
                        return;
                    case AddressGroupType.Outlook:
                        NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), MailService.Outlook);
                        return;
                    case AddressGroupType.OtherEmail:
                    default:
                        NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), MailService.Unknown);
                        return;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async void OnAccountAdded(object sender, AccountEventArgs e)
        {
            try
            {
                AddressGroupItem group = FindGroup(e.Account);

                if (group != null)
                {
                    await DispatcherService.RunAsync(() =>
                    {
                        AddAccount(group, e.Account);

                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async void OnAccountDeleted(object sender, AccountEventArgs e)
        {
            try
            {
                AddressGroupItem group = FindGroup(e.Account);
                AddressItem deletedItem = FindAddress(group, e.Account);

                if (group != null && deletedItem != null)
                {
                    await DispatcherService.RunAsync(() =>
                    {
                        group.Items.Remove(deletedItem);
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async void OnAccountUpdated(object sender, AccountEventArgs e)
        {
            try
            {
                AddressGroupItem group = FindGroup(e.Account);
                AddressItem oldItem = FindAddress(group, e.Account);

                if (group != null && oldItem != null)
                {
                    int index = group.Items.IndexOf(oldItem);
                    if (index >= 0)
                    {
                        await DispatcherService.RunAsync(() =>
                        {
                            group.Items[index] = new AddressItem(e.Account);
                        }).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task UpdateAccountsAsync()
        {
            List<Account> accounts = await Core.GetAccountsAsync().ConfigureAwait(true);

            GroupItems.Clear();

            IEnumerable<AddressGroupItem> items = Builder.Create()
                                                         .CreateAddressGroup(AddressGroupType.Eppie, true)
                                                         .CreateAddressGroup(AddressGroupType.Bitcoin, true)
                                                         .CreateAddressGroup(AddressGroupType.Ethereum, true)
                                                         .CreateAddressGroup(AddressGroupType.Gmail, false)
                                                         .CreateAddressGroup(AddressGroupType.OtherEmail, false)
                                                         .CreateAddressGroup(AddressGroupType.Outlook, false)
                                                         .CreateAddressGroup(AddressGroupType.Proton, false)
                                                         .ConfigureAddressesFromAccounts(accounts)
                                                         .Build();

            foreach (AddressGroupItem item in items)
            {
                GroupItems.Add(item);
            }
        }

        private AddressGroupItem FindGroup(Account account)
        {
            AddressGroupType type = GetAddressGroupType(account);
            return GroupItems.FirstOrDefault(item => item.Type == type);
        }

        private static AddressItem FindAddress(AddressGroupItem group, Account account)
        {
            if (group is null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return group.Items.FirstOrDefault(item => item.Account.Id == account.Id);
        }

        private static void AddAccount(AddressGroupItem group, Account account)
        {
            group?.AddItem(new AddressItem(account));
        }

        private static AddressGroupType GetAddressGroupType(Account account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            switch (account.Type)
            {
                case MailBoxType.Email:

                    if (account.AuthData.Type == AuthenticationType.Basic)
                    {
                        return AddressGroupType.OtherEmail;
                    }

                    if (account.AuthData.Type == AuthenticationType.OAuth2)
                    {
                        return account.IsGmail() ? AddressGroupType.Gmail :
                               account.IsOutlook() ? AddressGroupType.Outlook :
                               throw new NotSupportedException();
                    }

                    break;
                case MailBoxType.Dec:
                    return account.Email.Network == NetworkType.Eppie ? AddressGroupType.Eppie :
                           account.Email.Network == NetworkType.Bitcoin ? AddressGroupType.Bitcoin :
                           account.Email.Network == NetworkType.Ethereum ? AddressGroupType.Ethereum :
                           throw new NotSupportedException();
                case MailBoxType.Hybrid:
                    return AddressGroupType.Eppie;
                case MailBoxType.Proton:
                    return AddressGroupType.Proton;
            }

            throw new NotSupportedException();
        }

        private class Builder
        {
            private readonly Dictionary<AddressGroupType, AddressGroupItem> _groups = new Dictionary<AddressGroupType, AddressGroupItem>();

            private Builder()
            { }

            public static Builder Create()
            {
                return new Builder();
            }

            public Builder CreateAddressGroup(AddressGroupType type, bool isTestnet)
            {
                if (!_groups.ContainsKey(type))
                {
                    _groups.Add(type, new AddressGroupItem(type, GetGroupTitle(type), isTestnet));
                }

                return this;
            }

            public Builder CreateAddress(AddressGroupType type, Account account)
            {
                if (_groups.TryGetValue(type, out AddressGroupItem groupItem))
                {
                    AddAccount(groupItem, account);
                }

                return this;
            }

            public Builder ConfigureAddressesFromAccounts(List<Account> accounts)
            {
                foreach (var account in accounts)
                {
                    CreateAddress(GetAddressGroupType(account), account);
                }

                return this;
            }

            public IEnumerable<AddressGroupItem> Build()
            {
                foreach (var group in _groups.Values)
                {
                    group.SortItems();
                }

                IEnumerable<AddressGroupItem> items = _groups.Values.OrderByDescending(item => item.Items?.Count ?? 0)
                                                                    .ThenBy(item => item.GroupTitle);

                if (items.Any())
                {
                    items.Last().IsLastGroup = true;
                }

                return items;
            }

            private static string GetGroupTitle(AddressGroupType type)
            {
                // ToDo: strings that does not require localization should be moved to some service
                // (look at IBrandService, StaticStringResources)
                switch (type)
                {
                    case AddressGroupType.Gmail:
                        return nameof(AddressGroupType.Gmail);
                    case AddressGroupType.Outlook:
                        return nameof(AddressGroupType.Outlook);
                    case AddressGroupType.Proton:
                        return nameof(AddressGroupType.Proton);
                    case AddressGroupType.Eppie:
                        return nameof(AddressGroupType.Eppie);
                    case AddressGroupType.Bitcoin:
                        return nameof(AddressGroupType.Bitcoin);
                    case AddressGroupType.Ethereum:
                        return nameof(AddressGroupType.Ethereum);
                    case AddressGroupType.OtherEmail:
                        // ToDo: Localize
                        return "Other Email";
                    default:
                        return "Unknown";
                }
            }
        }
    }
}
