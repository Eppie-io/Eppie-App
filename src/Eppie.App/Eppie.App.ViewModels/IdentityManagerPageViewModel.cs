using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class IdentityManagerPageViewModel : BaseViewModel
    {
        public ObservableCollection<Account> EmailAccounts { get; } = new ObservableCollection<Account>();
        public ObservableCollection<LocalAIAgent> AIAgents { get; } = new ObservableCollection<LocalAIAgent>();

        public ICommand EditAccountCommand => new RelayCommand<object>(EditAccountInfo);
        public ICommand EditAIAgentCommand => new RelayCommand<object>(EditAIAgentInfo);

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                await UpdateAccountsAsync();
                UpdateAIAgents();

                Core.AccountAdded += Core_AccountAdded;
                Core.AccountDeleted += Core_AccountDeleted;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public override void OnNavigatedFrom()
        {
            Core.AccountAdded -= Core_AccountAdded;
            Core.AccountDeleted -= Core_AccountDeleted;
        }

        private async Task UpdateAccountsAsync()
        {
            List<Account> accounts = await Core.GetAccountsAsync().ConfigureAwait(true);

            EmailAccounts.Clear();
            foreach (Account account in accounts)
            {
                EmailAccounts.Add(account);
            }
        }

        private void Core_AccountAdded(object sender, AccountEventArgs e)
        {
            DispatcherService.RunAsync(() =>
            {
                try
                {
                    EmailAccounts.Add(e.Account);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            });
        }

        private void Core_AccountDeleted(object sender, AccountEventArgs e)
        {
            DispatcherService.RunAsync(() =>
            {
                try
                {
                    EmailAccounts.Remove(e.Account);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            });
        }

        private void EditAccountInfo(object item)
        {
            if (item is Account account)
            {
                NavigateToMailboxSettingsPage(account, isReloginNeeded: false);
            }
        }

        private void UpdateAIAgents()
        {
            var agents = AIService.GetAgents();

            AIAgents.Clear();
            foreach (LocalAIAgent agent in agents)
            {
                AIAgents.Add(agent);
            }
        }

        private void EditAIAgentInfo(object item)
        {
            if (item is LocalAIAgent agent)
            {
                NavigationService?.Navigate(nameof(LocalAIAgentSettingsPageViewModel), agent);
            }
        }
    }
}
