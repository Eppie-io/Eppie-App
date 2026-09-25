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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Eppie.App.ViewModels.Services;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;
using Tuvi.Core.Web.BackupService.Client;
using Tuvi.OAuth2;
using Tuvi.Proton;

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

        protected Tuvi.Core.ITuviMail Core => CoreProvider?.Invoke();
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

        protected AuthorizationProvider AuthProvider { get; private set; }
        public void SetAuthProvider(AuthorizationProvider authProvider)
        {
            AuthProvider = authProvider;
        }

        protected IProtonLoginHelper ProtonLoginHelper { get; private set; }
        public void SetProtonLoginHelper(IProtonLoginHelper protonLoginHelper)
        {
            ProtonLoginHelper = protonLoginHelper;
        }

        protected INavigationService NavigationService { get; private set; }
        public void SetNavigationService(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        protected IPendingMailtoService PendingMailtoService { get; private set; }
        public void SetPendingMailtoService(IPendingMailtoService pendingMailtoService)
        {
            PendingMailtoService = pendingMailtoService;
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

        public bool IsLocalAIAvailable => AIService.IsAvailable();

        public virtual void OnError(Exception e)
        {
            ErrorHandler?.OnError(e, false);
        }

        protected string GetLocalizedString(string resource)
        {
            return LocalizationService?.GetString(resource) ?? string.Empty;
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
            else if (account.Email.IsProton())
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

        protected async Task BackupIfNeededAsync()
        {
            bool isSeedInitialized = await Core.GetSecurityManager().IsSeedPhraseInitializedAsync().ConfigureAwait(true);

            if (isSeedInitialized)
            {
                // Test node URI
                const string uploadUrl = "https://testnet.eppie.io/api/UploadBackupFunction?code=1";
                // Local node URI
                //const string uploadUrl = "http://localhost:7071/api/UploadBackupFunction";

                string fingerprint = Core.GetBackupManager().GetBackupKeyFingerprint();

                using (var backup = new MemoryStream())
                using (var detachedSignatureData = new MemoryStream())
                using (var publicKey = new MemoryStream())
                {
                    await Core.GetBackupManager().CreateBackupAsync(backup).ConfigureAwait(true);
                    await Core.GetBackupManager().CreateDetachedSignatureDataAsync(backup, detachedSignatureData, publicKey).ConfigureAwait(true);

                    await BackupServiceClient.UploadAsync(new Uri(uploadUrl), fingerprint, publicKey, detachedSignatureData, backup).ConfigureAwait(true);
                }
            }
        }

        protected async Task ProcessAccountDataAsync(Account account, CancellationToken cancellationToken = default)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            bool existAccount = await Core.ExistsAccountWithEmailAddressAsync(account.Email, cancellationToken).ConfigureAwait(true);

            if (!existAccount)
            {
                await Core.AddAccountAsync(account, cancellationToken).ConfigureAwait(true);
            }
            else
            {
                await Core.UpdateAccountAsync(account, cancellationToken).ConfigureAwait(true);
            }

            await BackupIfNeededAsync().ConfigureAwait(true);
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
    }
}
