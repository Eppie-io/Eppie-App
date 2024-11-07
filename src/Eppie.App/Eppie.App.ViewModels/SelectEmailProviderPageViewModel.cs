using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.OAuth2;

namespace Tuvi.App.ViewModels
{
    public enum EmailProviderKey
    {
        Other,
        Google,
        Outlook,
        Decentralized,
        Proton
    }

    public class EmailProviderInfo
    {
        public EmailProviderKey Key { get; }

        public string Name { get; }

        public string Description { get; }


        public EmailProviderInfo(EmailProviderKey key, string name, string description)
        {
            Key = key;
            Name = name;
            Description = description;
        }
    }

    public class SelectEmailProviderPageViewModel : BaseViewModel
    {
        public ObservableCollection<EmailProviderInfo> EmailProviders { get; } = new ObservableCollection<EmailProviderInfo>();

        private EmailProviderInfo _selectedProvider;
        public EmailProviderInfo SelectedProvider
        {
            get { return _selectedProvider; }
            set { SetProperty(ref _selectedProvider, value); }
        }

        public ICommand GoBackCommand => new RelayCommand(() => NavigationService?.GoBackOrNavigate(nameof(MainPageViewModel)));

        public ICommand ConnectServiceCommand => new RelayCommand(DoConnectServiceAction);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public SelectEmailProviderPageViewModel()
        {
            FillProvidersList();
        }

        private void DoConnectServiceAction()
        {
            if (SelectedProvider is null)
            {
                return;
            }
            if (SelectedProvider.Key == EmailProviderKey.Decentralized)
            {
                NavigationService?.Navigate(nameof(DecentralizedAccountSettingsPageViewModel));
                return;
            }
            if (SelectedProvider.Key == EmailProviderKey.Proton)
            {
                NavigationService?.Navigate(nameof(ProtonAccountSettingsPageViewModel));
                return;
            }

            var mailServerId = MailService.Unknown;

            switch (SelectedProvider.Key)
            {
                case EmailProviderKey.Google:
                    mailServerId = MailService.Gmail;
                    break;
                case EmailProviderKey.Outlook:
                    mailServerId = MailService.Outlook;
                    break;
                case EmailProviderKey.Other:
                    mailServerId = MailService.Unknown;
                    break;
                default:
                    mailServerId = MailService.Unknown;
                    break;
            }

            NavigationService?.Navigate(nameof(AccountSettingsPageViewModel), mailServerId);
        }


        private void FillProvidersList()
        {
            //TODO: TVM-544 Move strings to resources

            EmailProviders.Add(new EmailProviderInfo(EmailProviderKey.Google, "Gmail", "gmail.com"));
            EmailProviders.Add(new EmailProviderInfo(EmailProviderKey.Outlook, "Outlook", "outlook.com hotmail.com"));
            EmailProviders.Add(new EmailProviderInfo(EmailProviderKey.Proton, "ProtonMail", "proton.me protonmail.com proton.local"));

            EmailProviders.Add(new EmailProviderInfo(EmailProviderKey.Other, "Other email account", "IMAP, SMTP"));

            EmailProviders.Add(new EmailProviderInfo(EmailProviderKey.Decentralized, "Decentralized account (coming soon)", "please join the waiting list"));

            SelectedProvider = EmailProviders.First();
        }
    }
}
