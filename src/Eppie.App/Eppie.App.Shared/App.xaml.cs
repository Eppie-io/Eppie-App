﻿// ---------------------------------------------------------------------------- //
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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Eppie.App.Shared.Helpers;
using Eppie.App.Shared.Logging;
using Eppie.App.Shared.Services;
using Eppie.App.ViewModels.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tuvi.App.Shared.Models;
using Tuvi.App.Shared.Services;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core;
using Tuvi.Core.Entities;
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

        // The minimum window size is the size of the Xbox app, excluding the TV-safe zone.
        // https://learn.microsoft.com/en-us/windows/apps/design/devices/designing-for-tv#scale-factor-and-adaptive-layout
        // https://learn.microsoft.com/en-us/windows/apps/design/devices/designing-for-tv#tv-safe-area
        // (1920 x 1080 pixels) in 200% scale excluding (48px,27px,48px,27px) area
        private const int MinWidth = 864;       // 1920 / 2 - 48 - 48 = 864
        private const int MinHeight = 486;      // 1080 / 2 - 27 - 27 = 486

        public static ILoggerFactory LoggerFactory { get; private set; }
        private ILogger<App> Logger { get; set; }

        public INavigationService NavigationService { get; private set; }
        public ITuviMail Core { get; private set; }
        public ILocalSettingsService LocalSettingsService { get; private set; }
        public AuthorizationProvider AuthProvider { get; private set; }
        private NotificationManager _notificationManager { get; set; }
        public IAIService AIService { get; private set; }

        public static Window MainWindow { get; private set; }
        public XamlRoot XamlRoot => MainWindow?.Content?.XamlRoot;

        private ErrorHandler _errorHandler;

        public IHost Host { get; private set; }

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                InitializeSettings();
                InitializeLogger();
                LogLaunchInformation();

                BuildHost();

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

        private void InitializeSettings()
        {
            LocalSettingsService = new LocalSettingsService();
            ApplicationLanguages.PrimaryLanguageOverride = LocalSettingsService.Language;

            LocalSettingsService.SettingChanged += LocalSettingsService_ThemeSettingChanged;
        }

        private void InitializeLogger()
        {
            Serilog.Core.LoggingLevelSwitch logLevelSwitch = new Serilog.Core.LoggingLevelSwitch(LocalSettingsService.LogLevel.ToLogEventLevel());

            LocalSettingsService.SettingChanged += (sender, args) =>
            {
                if (args.Name == nameof(LocalSettingsService.LogLevel))
                {
                    logLevelSwitch.MinimumLevel = LocalSettingsService.LogLevel.ToLogEventLevel();
                }
            };

            LoggerFactory = new Serilog.LoggerConfiguration().AddLogging()
                                                             .UseLogLevelSwitch(logLevelSwitch)
                                                             .CreateLoggerFactory();
            Logger = LoggerFactory.CreateLogger<App>();
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

            CreateAIService();
        }

        private void CreateAIService()
        {
            if (AIService != null)
            {
                AIService.ExceptionOccurred -= AIService_ExceptionOccurred;
            }

            AIService = new AIService(Core);
            AIService.ExceptionOccurred += AIService_ExceptionOccurred;
        }

        private void AIService_ExceptionOccurred(object sender, ExceptionEventArgs e)
        {
            OnError(e.Exception);
        }

        private void BuildHost()
        {
            IHostBuilder builder = EppieHost.CreateBuilder(LoggerFactory);

#if !WINDOWS_UWP // ToDo: UWP project needs to be migrated to net9.0
            Host = builder.Build();
#endif
        }

        private void CreateAuth()
        {
            AuthProvider = AuthorizationFactory.GetAuthorizationProvider(Tuvi.App.Shared.Authorization.AuthConfig.GetAuthorizationConfiguration());
        }

        private void CreateCore()
        {
            ITokenRefresher tokenRefresher = AuthorizationFactory.GetTokenRefresher(AuthProvider);
            Core = ComponentBuilder.Components.CreateTuviMailCore(Path.Combine(DataFolder, DataBaseFileName),
                                                                  new Tuvi.Core.ImplementationDetailsProvider("Tuvi seed", "Tuvi.Package", "backup@system.service.tuvi.com"),
                                                                  tokenRefresher,
                                                                  LoggerFactory);
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

            frame.RequestedTheme = ToElementTheme(LocalSettingsService.Theme);

            ConfigurePreferredMinimumSize();

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
            Logger?.LogError(exception, "An error has occurred");
            _errorHandler?.OnError(exception, false);
        }

        private void LogLaunchInformation()
        {
            BrandLoader brand = new BrandLoader();
            Logger?.LogInformation("Launching the {AppName} app (version {AppVersion}; language {Language}) on {OSDescription} OS",
                                   brand.GetName(),
                                   brand.GetAppVersion(),
                                   ApplicationLanguages.PrimaryLanguageOverride,
                                   RuntimeInformation.OSDescription);
        }

        private void LocalSettingsService_ThemeSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (args.Name == nameof(LocalSettingsService.Theme))
            {
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            if (MainWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ToElementTheme(LocalSettingsService.Theme);
            }
        }

        private static ElementTheme ToElementTheme(Tuvi.App.ViewModels.Services.AppTheme theme)
        {
            switch (theme)
            {
                case Tuvi.App.ViewModels.Services.AppTheme.Light:
                    return ElementTheme.Light;
                case Tuvi.App.ViewModels.Services.AppTheme.Dark:
                    return ElementTheme.Dark;
                default:
                    return ElementTheme.Default;
            }
        }
    }
}
