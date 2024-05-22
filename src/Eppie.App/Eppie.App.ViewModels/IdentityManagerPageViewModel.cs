using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using Tuvi.Core.Entities;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

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
                if (StringHelper.IsDecentralizedEmail(account.Email))
                {
                    NavigationService?.Navigate(nameof(DecentralizedAccountSettingsPageViewModel), account);
                }
                else if(Proton.Extensions.IsProton(account.Email))
                {
                    NavigationService?.Navigate(nameof(ProtonAccountSettingsPageViewModel), account);
                }
                else
                {
                    NavigationService?.Navigate(nameof(AccountSettingsPageViewModel), account);
                }
            }
        }
    }
}
