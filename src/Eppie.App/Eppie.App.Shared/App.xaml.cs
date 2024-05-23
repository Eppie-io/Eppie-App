using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Tuvi.App.Helpers;
using Tuvi.App.Shared.Models;
using Tuvi.App.Shared.Services;
using Tuvi.App.Shared.Views;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using Tuvi.OAuth2;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace Tuvi.App.Shared
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static string DataBaseFileName = "TuviMail.db";
        public Tuvi.Core.ITuviMail Core { get; private set; }

        public AuthorizationProvider AuthProvider { get; private set; }

        public INavigationService NavigationService { get; private set; }
        public ILocalSettingsService LocalSettingsService { get; private set; }

        private NotificationManager _notificationManager { get; set; }

        private ErrorHandler _errorHandler;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeLogging();

            InitializeComponent();

            Suspending += OnSuspending;
            UnhandledException += CurrentApp_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            CreateAuth();
            CreateCore();

            LocalSettingsService = new LocalSettingsService();
            ApplicationLanguages.PrimaryLanguageOverride = LocalSettingsService.Language;
        }

        private void CreateAuth()
        {
            AuthProvider = AuthorizationFactory.GetAuthorizationProvider(Authorization.AuthConfig.GetAuthorizationConfiguration());
        }

        private void CreateCore()
        {
            var tokenRefresher = AuthorizationFactory.GetTokenRefresher(AuthProvider);
            Core = ComponentBuilder.Components.CreateTuviMailCore(Path.Combine(DataHelper.GetAppDataPath(), DataBaseFileName), new Core.ImplementationDetailsProvider("Tuvi seed", "Tuvi.Package", "backup@system.service.tuvi.com"), tokenRefresher);
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

        private async void OnWipeAllDataNeeded(object sender, EventArgs e)
        {
            try
            {
                // Dispose should be before await method 
                DisposeCore();
                CreateCore();
                await RemoveTempFilesAsync().ConfigureAwait(true);
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

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Windows.UI.Xaml.Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = CreateRootFrame();

                if (e?.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                Windows.UI.Xaml.Window.Current.Content = rootFrame;
            }

            if (e?.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    var resourceLoader = ResourceLoader.GetForCurrentView();

                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    // rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    // does database exist
                    if (await Core.IsFirstApplicationStartAsync().ConfigureAwait(true))
                    {
                        rootFrame.Navigate(typeof(WelcomePage));
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(PasswordPage), PasswordActions.EnterPassword);
                    }
                }
                // Ensure the current window is active
                Windows.UI.Xaml.Window.Current.Activate();
            }
        }

        private Frame CreateRootFrame()
        {
            var frame = new Frame();
            frame.NavigationFailed += OnNavigationFailed;

            NavigationService = new NavigationService(frame, "Tuvi.App.Shared.Views.");

            _errorHandler = new ErrorHandler();
            _errorHandler.SetMessageService(new MessageService());

            return frame;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private void CurrentApp_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
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

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(Path.Combine(DataHelper.GetAppDataPath(), "Logs\\log-.txt"), rollingInterval: RollingInterval.Day).CreateLogger();
                builder.AddDebug();
                builder.AddSerilog();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Debug);
                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
            Tuvi.Core.Logging.LoggingExtension.LoggerFactory = factory;

#if HAS_UNO
            global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
        }
    }
}
