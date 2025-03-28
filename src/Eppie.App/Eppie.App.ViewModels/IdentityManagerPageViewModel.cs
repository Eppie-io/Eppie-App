using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public bool IsLocalAIAvailable => AIService.IsAvailable();

        public ICommand EditAccountCommand => new RelayCommand<object>(EditAccountInfo);
        public ICommand EditAIAgentCommand => new RelayCommand<object>(EditAIAgentInfo);

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                await UpdateAccountsAsync().ConfigureAwait(true);
                await UpdateAIAgentsAsync().ConfigureAwait(true);

                Core.AccountAdded += Core_AccountAdded;
                Core.AccountDeleted += Core_AccountDeleted;

                AIService.AgentAdded += AIService_AgentAdded;
                AIService.AgentDeleted += AIService_AgentDeleted;
                AIService.AgentUpdated += AIService_AgentUpdated;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public override void OnNavigatedFrom()
        {
            AIService.AgentAdded -= AIService_AgentAdded;
            AIService.AgentDeleted -= AIService_AgentDeleted;
            AIService.AgentDeleted -= AIService_AgentUpdated;

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

        private void AIService_AgentAdded(object sender, LocalAIAgentEventArgs e)
        {
            DispatcherService.RunAsync(() =>
            {
                try
                {
                    AIAgents.Add(e.AIAgent);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            });
        }

        private void AIService_AgentDeleted(object sender, LocalAIAgentEventArgs e)
        {
            DispatcherService.RunAsync(() =>
            {
                try
                {
                    AIAgents.Remove(e.AIAgent);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            });
        }

        private void AIService_AgentUpdated(object sender, LocalAIAgentEventArgs e)
        {
            DispatcherService.RunAsync(() =>
            {
                try
                {
                    var index = AIAgents.IndexOf(AIAgents.FirstOrDefault(agent => agent.Id == e.AIAgent.Id));
                    if (index != -1)
                    {
                        AIAgents[index] = e.AIAgent;
                    }
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

        private async Task UpdateAIAgentsAsync()
        {
            var agents = await AIService.GetAgentsAsync().ConfigureAwait(true);

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
