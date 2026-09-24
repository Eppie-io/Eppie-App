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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class PageViewModel : BaseViewModel
    {
        public string TwitterPostLink
        {
            get
            {
                var twitterHandle = BrandService.GetTwitterHandle();
                var githubUrl = BrandService.GetGitHub();
                var text = GetLocalizedString("WhatsNewTwitPostText");
                text = string.Format(CultureInfo.InvariantCulture, text, twitterHandle);

                var encodedText = Uri.EscapeDataString(text);
                var encodedGithubUrl = Uri.EscapeDataString(githubUrl);

                return $"https://twitter.com/intent/tweet?text={encodedText}&url={encodedGithubUrl}";
            }
        }

        public ICommand SupportDevelopmentCommand => new AsyncRelayCommand(SupportDevelopmentAsync);
        public ICommand OpenAllPgpKeysCommand => new RelayCommand(() => NavigationService?.NavigateToListPgpKeys());

        private bool _isStorePaymentProcessor = true;
        public bool IsStorePaymentProcessor
        {
            get => _isStorePaymentProcessor;
            private set
            {
                _isStorePaymentProcessor = value;
                OnPropertyChanged(nameof(IsStorePaymentProcessor));
            }
        }

        private string _supportDevelopmentPrice;
        public string SupportDevelopmentPrice
        {
            get => _supportDevelopmentPrice;
            private set
            {
                _supportDevelopmentPrice = value;
                OnPropertyChanged(nameof(SupportDevelopmentPrice));
            }
        }

        private bool _isSupportDevelopmentButtonVisible;
        public bool IsSupportDevelopmentButtonVisible
        {
            get => _isSupportDevelopmentButtonVisible;
            private set
            {
                _isSupportDevelopmentButtonVisible = value;
                OnPropertyChanged(nameof(IsSupportDevelopmentButtonVisible));
            }
        }

        public bool IsPreviewAvailable { get; }

        private async void UpdateSupportDevelopmentButton()
        {
            try
            {
                await UpdateSupportDevelopmentButtonAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        protected async Task UpdateSupportDevelopmentButtonAsync()
        {
            if (string.IsNullOrWhiteSpace(BrandService.GetDevelopmentSupport()))
            {
                IsSupportDevelopmentButtonVisible = false;
                return;
            }

            try
            {
                IsSupportDevelopmentButtonVisible = !await AppStoreService.IsSubscriptionEnabledAsync().ConfigureAwait(true);

                if (IsSupportDevelopmentButtonVisible)
                {
                    SupportDevelopmentPrice = await AppStoreService.GetSubscriptionPriceAsync().ConfigureAwait(true);
                }
            }
            catch (NotImplementedException)
            {
                IsSupportDevelopmentButtonVisible = true;
                IsStorePaymentProcessor = false;
                SupportDevelopmentPrice = "$3";
            }
        }

        private async Task SupportDevelopmentAsync()
        {
            try
            {
                await AppStoreService.BuySubscriptionAsync().ConfigureAwait(true);
                UpdateSupportDevelopmentButton();
            }
            catch
            {
                await LauncherService.LaunchAsync(new Uri(BrandService.GetDevelopmentSupport())).ConfigureAwait(true);
            }
        }

        virtual public void OnNavigatedTo(object data)
        {
            UpdateSupportDevelopmentButton();
        }

        virtual public void OnNavigatedFrom()
        {
        }

        protected async Task<Account> CreateDecentralizedAccountAsync(NetworkType networkType, CancellationToken cancellationToken)
        {
            var (publicKey, accountIndex) = await Core.GetSecurityManager()
                .GetNextDecAccountPublicKeyAsync(networkType, cancellationToken)
                .ConfigureAwait(true);

            var email = EmailAddress.CreateDecentralizedAddress(networkType, publicKey);

            return new Account()
            {
                Email = email,
                IsBackupAccountSettingsEnabled = true,
                IsBackupAccountMessagesEnabled = true,
                Type = MailBoxType.Dec,
                DecentralizedAccountIndex = accountIndex,
                IsMessageFooterEnabled = false
            };
        }

        protected void NavigateToMailboxSettingsPage(Account account, bool isReloginNeeded)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (account.Email.IsDecentralized)
            {
                if (account.Email.Network == NetworkType.Eppie)
                {
                    NavigationService?.NavigateToEppieAddressSettings(account);
                }
                else if (account.Email.Network == NetworkType.Bitcoin)
                {
                    NavigationService?.NavigateToBitcoinAddressSettings(account);
                }
                else if (account.Email.Network == NetworkType.Ethereum)
                {
                    NavigationService?.NavigateToEthereumAddressSettings(account);
                }
            }
            else if (Proton.Extensions.IsProton(account.Email))
            {
                if (isReloginNeeded)
                {
                    _ = MessageService.ShowProtonConnectAddressDialogAsync(account);
                }
                else
                {
                    NavigationService?.NavigateToProtonAddressSettings(account);
                }
            }
            else
            {
                NavigationService?.NavigateToEmailAddressSettings(account, isReloginNeeded);
            }
        }

        protected async Task AIAgentProcessMessageAsync(LocalAIAgent agent, MessageInfo message)
        {
            if (agent is null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var text = message.HasTextBody ? message.MessageTextBody : Core.GetTextUtils().GetTextFromHtml(message.MessageHtmlBody);

            message.AIAgentProcessedBody = GetLocalizedString("ThinkingMessage");
            var thinking = true;

            message.AIAgentProcessedBody = await AIService.ProcessTextAsync
            (
                agent,
                text,
                CancellationToken.None,
                textPart => DispatcherService.RunAsync(() =>
                {
                    if (thinking)
                    {
                        message.AIAgentProcessedBody = string.Empty;
                        thinking = false;
                    }
                    message.AIAgentProcessedBody += textPart;
                })
            ).ConfigureAwait(true);

            try
            {
                await Core.UpdateMessageProcessingResultAsync(message.MessageData, message.AIAgentProcessedBody).ConfigureAwait(true);
            }
            catch (MessageIsNotExistException)
            {
                // Message is deleted
            }
        }

        public virtual Task CreateAIAgentsMenuAsync(Action<string, Action<IList<object>>> action)
        {
            return Task.CompletedTask;
        }

        public virtual Task CreateAIAgentsMenuAsync(Action<string, Action> action)
        {
            return Task.CompletedTask;
        }

        public async void ShowPreview()
        {
            try
            {
                // ToDo: Here you can add a preview of your UI controls.
                // And change the `IsPreviewAvailable` property to true.

                await MessageService.ShowInvitationDialogAsync();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
