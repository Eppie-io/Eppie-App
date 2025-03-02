using System;
using System.IO;
using System.Threading.Tasks;
using BackupServiceClientLibrary;
using CommunityToolkit.Mvvm.ComponentModel;
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

        public void SetCore(Func<Tuvi.Core.ITuviMail> coreProvider)
        {
            CoreProvider = coreProvider;
        }

        private Func<Tuvi.Core.ITuviMail> CoreProvider { get; set; }

        protected Tuvi.Core.ITuviMail Core { get { return CoreProvider(); } }

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

        protected IAIService AIService { get; private set; }
        public void SetAIService(IAIService aiService)
        {
            AIService = aiService;
        }

        private ILocalizationService LocalizationService { get; set; }
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

        virtual public void OnNavigatedTo(object data)
        {
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
            return LocalizationService?.GetString(resource) ?? "";
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

        protected IPurchaseService PurchaseService { get; private set; }
        public void SetPurchaseService(IPurchaseService purchaseService)
        {
            PurchaseService = purchaseService;
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
            if (StringHelper.IsDecentralizedEmail(account.Email))
            {
                NavigationService?.Navigate(nameof(DecentralizedAccountSettingsPageViewModel), account);
            }
            else if (Proton.Extensions.IsProton(account.Email))
            {
                if (isReloginNeeded)
                {
                    NavigationService?.Navigate(nameof(ProtonAccountSettingsPageViewModel), new ProtonAccountSettingsPageViewModel.NeedReloginData { Account = account });
                }
                else
                {
                    NavigationService?.Navigate(nameof(ProtonAccountSettingsPageViewModel), account);
                }
            }
            else
            {
                if (isReloginNeeded)
                {
                    NavigationService?.Navigate(nameof(AccountSettingsPageViewModel), new AccountSettingsPageViewModel.NeedReloginData { Account = account });
                }
                else
                {
                    NavigationService?.Navigate(nameof(AccountSettingsPageViewModel), account);
                }
            }
        }
    }
}
