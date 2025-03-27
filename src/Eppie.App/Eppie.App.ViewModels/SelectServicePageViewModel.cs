using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.OAuth2;

namespace Tuvi.App.ViewModels
{
    public enum ServiceKey
    {
        Other,
        Google,
        Outlook,
        Decentralized,
        Proton,
        LoacalAIAgent
    }

    public class ServiceInfo
    {
        public ServiceKey Key { get; }

        public string Name { get; }

        public string Description { get; }


        public ServiceInfo(ServiceKey key, string name, string description)
        {
            Key = key;
            Name = name;
            Description = description;
        }
    }

    public class SelectServicePageViewModel : BaseViewModel
    {
        public ObservableCollection<ServiceInfo> Services { get; } = new ObservableCollection<ServiceInfo>();

        private ServiceInfo _selectedService;
        public ServiceInfo SelectedService
        {
            get { return _selectedService; }
            set { SetProperty(ref _selectedService, value); }
        }

        public ICommand GoBackCommand => new RelayCommand(() => NavigationService?.GoBackOrNavigate(nameof(MainPageViewModel)));

        public ICommand ConnectServiceCommand => new RelayCommand(DoConnectServiceAction);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        override public void OnNavigatedTo(object data)
        {
            base.OnNavigatedTo(data);
            FillServicesList();
        }

        private void DoConnectServiceAction()
        {
            if (SelectedService is null)
            {
                return;
            }
            if (SelectedService.Key == ServiceKey.Decentralized)
            {
                NavigationService?.Navigate(nameof(DecentralizedAccountSettingsPageViewModel));
                return;
            }
            if (SelectedService.Key == ServiceKey.Proton)
            {
                NavigationService?.Navigate(nameof(ProtonAccountSettingsPageViewModel));
                return;
            }
            if (SelectedService.Key == ServiceKey.LoacalAIAgent)
            {
                NavigationService?.Navigate(nameof(LocalAIAgentSettingsPageViewModel));
                return;
            }

            var mailServerId = MailService.Unknown;

            switch (SelectedService.Key)
            {
                case ServiceKey.Google:
                    mailServerId = MailService.Gmail;
                    break;
                case ServiceKey.Outlook:
                    mailServerId = MailService.Outlook;
                    break;
                case ServiceKey.Other:
                    mailServerId = MailService.Unknown;
                    break;
                default:
                    mailServerId = MailService.Unknown;
                    break;
            }

            NavigationService?.Navigate(nameof(AccountSettingsPageViewModel), mailServerId);
        }


        private void FillServicesList()
        {
            Services.Add(new ServiceInfo(ServiceKey.Google, "Gmail", "gmail.com"));
            Services.Add(new ServiceInfo(ServiceKey.Outlook, "Outlook", "outlook.com hotmail.com"));
            Services.Add(new ServiceInfo(ServiceKey.Proton, "Proton Mail (Beta)", "proton.me protonmail.com proton.local"));
            Services.Add(new ServiceInfo(ServiceKey.Other, GetLocalizedString("OtherEmailAccountServiceTitle"), GetLocalizedString("OtherEmailAccountServiceDescription")));
            Services.Add(new ServiceInfo(ServiceKey.Decentralized, GetLocalizedString("DecentralizedAccountServiceTitle"), GetLocalizedString("DecentralizedAccountServiceDescription")));

            if (AIService.IsAvailable())
            {
                Services.Add(new ServiceInfo(ServiceKey.LoacalAIAgent, GetLocalizedString("LoacalAIAgentServiceTitle"), GetLocalizedString("LoacalAIAgentServiceDescription")));
            }

            SelectedService = Services.First();
        }
    }
}
