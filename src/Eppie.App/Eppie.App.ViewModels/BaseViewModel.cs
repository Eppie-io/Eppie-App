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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BackupServiceClientLibrary;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eppie.App.ViewModels.Services;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class BaseViewModel : ObservableValidator
    {
        protected IErrorHandler ErrorHandler { get; private set; }
        public void SetErrorHandler(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
            ErrorHandler?.SetMessageService(MessageService);
        }

        protected Tuvi.Core.ITuviMail Core { get { return CoreProvider(); } }
        protected Func<Tuvi.Core.ITuviMail> CoreProvider { get; private set; }
        public void SetCoreProvider(Func<Tuvi.Core.ITuviMail> coreProvider)
        {
            CoreProvider = coreProvider;
        }

        protected IAIService AIService { get { return AIServiceProvider(); } }
        private Func<IAIService> AIServiceProvider { get; set; }
        public void SetAIServiceProvider(Func<IAIService> provider)
        {
            AIServiceProvider = provider;
        }

        protected INavigationService NavigationService { get; private set; }
        public void SetNavigationService(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        protected ILocalSettingsService LocalSettingsService { get; private set; }
        public void SetLocalSettingsService(ILocalSettingsService localSettingsService)
        {
            LocalSettingsService = localSettingsService;
        }

        protected ILocalizationService LocalizationService { get; set; }
        public void SetLocalizationService(ILocalizationService localizationService)
        {
            LocalizationService = localizationService;
        }

        protected ITuviMailMessageService MessageService { get; private set; }
        public void SetMessageService(ITuviMailMessageService messageService)
        {
            MessageService = messageService;
            ErrorHandler?.SetMessageService(MessageService);
        }

        protected ILauncherService LauncherService { get; private set; }
        public void SetLauncherService(ILauncherService launcherService)
        {
            LauncherService = launcherService;
        }

        public ICommand SupportDevelopmentCommand => new AsyncRelayCommand(SupportDevelopmentAsync);
        public ICommand PgpKeysCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(PgpKeysPageViewModel)));

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

        private async void UpdateSupportDevelopmentButton()
        {
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
            catch (Exception e)
            {
                OnError(e);
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

        public virtual void OnError(Exception e)
        {
            ErrorHandler?.OnError(e, false);
        }

        protected string GetLocalizedString(string resource)
        {
            return LocalizationService?.GetString(resource) ?? string.Empty;
        }

        protected IDispatcherService DispatcherService { get; private set; }
        public void SetDispatcherService(IDispatcherService dispatcherService)
        {
            DispatcherService = dispatcherService;
        }

        protected IBrandService BrandService { get; private set; }
        public void SetBrandService(IBrandService brandService)
        {
            BrandService = brandService;
        }

        protected IAppStoreService AppStoreService { get; private set; }
        public void SetAppStoreService(IAppStoreService appStoreService)
        {
            AppStoreService = appStoreService;
        }

        protected IDragAndDropService DragAndDropService { get; private set; }
        public void SetDragAndDropService(IDragAndDropService dragAndDropService)
        {
            DragAndDropService = dragAndDropService;
        }

        protected async Task BackupIfNeededAsync()
        {
            var accounts = await Core.GetAccountsAsync().ConfigureAwait(true);
            var isSeedInitialized = await Core.GetSecurityManager().IsSeedPhraseInitializedAsync().ConfigureAwait(true);
            if (accounts.Count >= 0 && isSeedInitialized)
            {
                // Test node URI
                const string uploadUrl = "https://testnet.eppie.io/api/UploadBackupFunction?code=1";
                // Local node URI
                //const string uploadUrl = "http://localhost:7071/api/UploadBackupFunction";

                var fingerprint = Core.GetBackupManager().GetBackupKeyFingerprint();

                using (var backup = new MemoryStream())
                using (var deatachedSignatureData = new MemoryStream())
                using (var publicKey = new MemoryStream())
                {
                    await Core.GetBackupManager().CreateBackupAsync(backup).ConfigureAwait(true);
                    await Core.GetBackupManager().CreateDetachedSignatureDataAsync(backup, deatachedSignatureData, publicKey).ConfigureAwait(true);

                    await BackupServiceClient.UploadAsync(new Uri(uploadUrl), fingerprint, publicKey, deatachedSignatureData, backup).ConfigureAwait(true);
                }
            }
        }

        protected void NavigateToMailboxSettingsPage(Account account, bool isReloginNeeded)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (account.Email.IsDecentralized)
            {
                NavigationService?.Navigate(nameof(DecentralizedAddressSettingsPageViewModel), account);
            }
            else if (Proton.Extensions.IsProton(account.Email))
            {
                if (isReloginNeeded)
                {
                    NavigationService?.Navigate(nameof(ProtonAddressSettingsPageViewModel), new ProtonAddressSettingsPageViewModel.NeedReloginData { Account = account });
                }
                else
                {
                    NavigationService?.Navigate(nameof(ProtonAddressSettingsPageViewModel), account);
                }
            }
            else
            {
                if (isReloginNeeded)
                {
                    NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), new EmailAddressSettingsPageViewModel.NeedReloginData { Account = account });
                }
                else
                {
                    NavigationService?.Navigate(nameof(EmailAddressSettingsPageViewModel), account);
                }
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
    }
}
