using System;
using System.IO;
using System.Threading.Tasks;
using Eppie.App.Shared.Services;
using Tuvi.App.Shared.Models;
using Tuvi.App.Shared.Services;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core;
using Tuvi.OAuth2;
using Windows.Globalization;
using Windows.Storage;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.Shared
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static readonly string DataBaseFileName = "TuviMail.db";

        private static readonly string DataFolder = ApplicationData.Current.LocalFolder.Path;

        public INavigationService NavigationService { get; private set; }
        public ITuviMail Core { get; private set; }
        public ILocalSettingsService LocalSettingsService { get; private set; }
        public AuthorizationProvider AuthProvider { get; private set; }
        private NotificationManager _notificationManager { get; set; }
        public AIService AIService { get; private set; }

        public static Window MainWindow { get; private set; }
        public XamlRoot XamlRoot => MainWindow?.Content?.XamlRoot;

        private ErrorHandler _errorHandler;


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                InitializeLogging();
                InitializeComponent();
                SubscribeToEvents();
                ConfigureServices();
                InitializeNotifications();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private static void InitializeLogging()
        {
            // ToDo: Add logging
        }

        private void SubscribeToEvents()
        {
            SubscribeToPlatformSpecificEvents();

            UnhandledException += CurrentApp_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void ConfigureServices()
        {
            CreateAuth();
            CreateCore();

            LocalSettingsService = new LocalSettingsService();
            ApplicationLanguages.PrimaryLanguageOverride = LocalSettingsService.Language;
            CreateAIService();
        }

        private async void CreateAIService()
        {
            try
            {
                AIService = new AIService(Core);
                await AIService.LoadModelIfEnabled();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void CreateAuth()
        {
            AuthProvider = AuthorizationFactory.GetAuthorizationProvider(Tuvi.App.Shared.Authorization.AuthConfig.GetAuthorizationConfiguration());
        }

        private void CreateCore()
        {
            var tokenRefresher = AuthorizationFactory.GetTokenRefresher(AuthProvider);
            Core = ComponentBuilder.Components.CreateTuviMailCore(Path.Combine(DataFolder, DataBaseFileName), new Tuvi.Core.ImplementationDetailsProvider("Tuvi seed", "Tuvi.Package", "backup@system.service.tuvi.com"), tokenRefresher);
            _notificationManager = new NotificationManager(Core, OnError);
            Core.WipeAllDataNeeded += OnWipeAllDataNeeded;
        }

        private void DisposeCore()
        {
            Core.WipeAllDataNeeded -= OnWipeAllDataNeeded;
            if (Core is IDisposable d)
            {
                d.Dispose();
            }
        }

        private Frame CreateRootFrame()
        {
            var frame = new Frame();
            frame.NavigationFailed += OnNavigationFailed;

            // ToDo: use nameof(Tuvi.App.Shared.Views) and add dot(.) inside NavigationService
            NavigationService = new NavigationService(frame, "Tuvi.App.Shared.Views.");

            _errorHandler = new ErrorHandler();
            _errorHandler.SetMessageService(new MessageService(() => XamlRoot));

            return frame;
        }

        private async void OnWipeAllDataNeeded(object sender, EventArgs e)
        {
            try
            {
                // Dispose should be before await method 
                DisposeCore();
                CreateCore();
                await RemoveTempFilesAsync().ConfigureAwait(true);

                CreateAIService();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task RemoveTempFilesAsync()
        {
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            foreach (var item in await tempFolder.GetItemsAsync())
            {
                try
                {
                    await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }

#if WINDOWS_UWP
        private void CurrentApp_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
#else
        private void CurrentApp_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
#endif
        {
            try
            {
                e.Handled = true;
                OnError(e.Exception);
            }
            catch { }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                e.SetObserved();
                OnError(e.Exception);
            }
            catch { }
        }

        private void OnError(Exception exception)
        {
            _errorHandler?.OnError(exception, false);
        }
    }
}
