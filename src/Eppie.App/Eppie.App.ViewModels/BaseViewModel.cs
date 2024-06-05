using System;
using System.IO;
using System.Threading.Tasks;
using BackupServiceClientLibrary;
using CommunityToolkit.Mvvm.ComponentModel;
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

        protected ILocalizationService LocalizationService { get; private set; }
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

        public void OnError(Exception e, bool silent)
        {
            ErrorHandler?.OnError(e, silent);
        }

        public void OnError(Exception e)
        {
            if (e is AuthenticationException exception)
            {
                // It looks like the authentication data is out of date, you need to ask the user to authenticate again.
                AskUserRelogin(exception.Email);
            }
            else if (e is AuthorizationException exception2)
            {
                // It looks like the authentication data is out of date, you need to ask the user to authenticate again.
                AskUserRelogin(exception2.Email);
            }
            else
            {
                ErrorHandler?.OnError(e, false);
            }
        }

        private async void AskUserRelogin(EmailAddress email)
        {
            try
            {
                var account = await Core.GetAccountAsync(email).ConfigureAwait(true);
                NavigationService?.Navigate(nameof(AccountSettingsPageViewModel), new AccountSettingsPageViewModel.NeedReloginData { Account = account });
            }
            catch (Exception e)
            {
                OnError(e);
            }
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

    }
}
