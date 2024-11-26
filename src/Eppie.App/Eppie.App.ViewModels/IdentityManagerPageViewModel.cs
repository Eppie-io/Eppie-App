using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class IdentityManagerPageViewModel : BaseViewModel
    {
        public ObservableCollection<Account> EmailAccounts { get; } = new ObservableCollection<Account>();

        public ICommand EditAccountCommand => new RelayCommand<object>(EditAccountInfo);

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                List<Account> accounts = await Core.GetAccountsAsync().ConfigureAwait(true);

                EmailAccounts.Clear();
                foreach (Account account in accounts)
                {
                    EmailAccounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void EditAccountInfo(object item)
        {
            if (item is Account account)
            {
                NavigateToMailboxSettingsPage(account, isReloginNeeded: false);
            }
        }
    }
}
