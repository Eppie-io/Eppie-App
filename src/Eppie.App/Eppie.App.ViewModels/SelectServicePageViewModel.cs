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
        Proton,
        Eppie,
        BitcoinMail,
        EthereumMail,
        LocalAIAgent
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

            switch (SelectedService.Key)
            {
                case ServiceKey.Proton:
                    NavigationService?.Navigate(nameof(ProtonAddressSettingsPageViewModel));
                    return;
                case ServiceKey.LocalAIAgent:
                    NavigationService?.Navigate(nameof(LocalAIAgentSettingsPageViewModel));
                    return;
                case ServiceKey.Eppie:
                    NavigationService?.Navigate(nameof(EppieAddressSettingsPageViewModel));
                    return;
                case ServiceKey.BitcoinMail:
                    NavigationService?.Navigate(nameof(BitcoinAddressSettingsPageViewModel));
                    return;
                case ServiceKey.EthereumMail:
                    NavigationService?.Navigate(nameof(EthereumAddressSettingsPageViewModel));
                    return;
                case ServiceKey.Google:
                    NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), MailService.Gmail);
                    return;
                case ServiceKey.Outlook:
                    NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), MailService.Outlook);
                    return;
                case ServiceKey.Other:
                default:
                    NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), MailService.Unknown);
                    return;
            }
        }

        private void FillServicesList()
        {
            Services.Add(new ServiceInfo(ServiceKey.Google, "Gmail", "gmail.com"));
            Services.Add(new ServiceInfo(ServiceKey.Outlook, "Outlook", "outlook.com hotmail.com"));
            Services.Add(new ServiceInfo(ServiceKey.Proton, "Proton Mail (Beta)", "proton.me protonmail.com proton.local"));
            Services.Add(new ServiceInfo(ServiceKey.Other, GetLocalizedString("OtherEmailAccountServiceTitle"), GetLocalizedString("OtherEmailAccountServiceDescription")));
            Services.Add(new ServiceInfo(ServiceKey.Eppie, GetLocalizedString("DecentralizedAccountServiceTitle"), GetLocalizedString("DecentralizedAccountServiceDescription")));
            Services.Add(new ServiceInfo(ServiceKey.BitcoinMail, "Bitcoin mail (coming soon)", "Testnet"));
            Services.Add(new ServiceInfo(ServiceKey.EthereumMail, "Ethereum mail (coming soon)", "Testnet"));

            if (AIService.IsAvailable())
            {
                Services.Add(new ServiceInfo(ServiceKey.LocalAIAgent, GetLocalizedString("LocalAIAgentServiceTitle"), GetLocalizedString("LocalAIAgentServiceDescription")));
            }

            SelectedService = Services.First();
        }
    }
}
