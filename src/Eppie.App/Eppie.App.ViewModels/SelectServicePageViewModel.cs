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
        Decentralized,
        Proton,
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
            if (SelectedService.Key == ServiceKey.LocalAIAgent)
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
                Services.Add(new ServiceInfo(ServiceKey.LocalAIAgent, GetLocalizedString("LocalAIAgentServiceTitle"), GetLocalizedString("LocalAIAgentServiceDescription")));
            }

            SelectedService = Services.First();
        }
    }
}
