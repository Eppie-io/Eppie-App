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
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Eppie.App.Helpers;
using Eppie.App.Logging;
using Eppie.App.Services;
using Eppie.App.ViewModels.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Eppie.App.Models;
using Eppie.App.Views;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core;
using Tuvi.Core.Entities;
using Tuvi.OAuth2;
using Windows.Globalization;
using Windows.Storage;
using Windows.System;

#if WINDOWS_UWP
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
#endif

namespace Eppie.App
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

        private const double DefaultScale = 1.0;
        private double? _manualScale;
        private bool _sizeHandlerAttached;

        public static ILoggerFactory LoggerFactory { get; private set; }
        private ILogger<App> Logger { get; set; }

        public INavigationService NavigationService { get; private set; }
        public ITuviMail Core { get; private set; }
        public ILocalSettingsService LocalSettingsService { get; private set; }
        public IPendingMailtoService PendingMailtoService { get; private set; }
        public AuthorizationProvider AuthProvider { get; private set; }
        private NotificationManager _notificationManager { get; set; }
        public IAIService AIService { get; private set; }

        public static Window MainWindow { get; private set; }
        public static XamlRoot XamlRoot => MainWindow?.Content?.XamlRoot;

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
            CreatePendingMailtoService();
        }

        private void CreatePendingMailtoService()
        {
            PendingMailtoService = new Services.PendingMailtoService();
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
            AuthProvider = AuthorizationFactory.GetAuthorizationProvider(Eppie.App.Authorization.AuthConfig.GetAuthorizationConfiguration());
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
            frame.Navigated += OnFrameNavigated;

            // ToDo: use nameof(Eppie.App.Views) and add dot(.) inside NavigationService
            NavigationService = new NavigationService(frame, "Eppie.App.Views.");

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

        private static readonly Action<ILogger, Exception> LogError =
            LoggerMessage.Define(
                LogLevel.Error,
                new EventId(0, nameof(OnError)),
                "An error has occurred"
            );

        private void OnError(Exception exception)
        {
            LogError(Logger, exception);
            _errorHandler?.OnError(exception, false);
        }


        private static readonly Action<ILogger, string, string, string, string, Exception> LogLaunchInfo =
            LoggerMessage.Define<string, string, string, string>(
                LogLevel.Information,
                new EventId(1, nameof(LogLaunchInformation)),
                "Launching the {AppName} app (version {AppVersion}; language {Language}) on {OSDescription} OS"
            );

        private void LogLaunchInformation()
        {
            BrandLoader brand = new BrandLoader();
            LogLaunchInfo(Logger, brand.GetName(), brand.GetAppVersion(), ApplicationLanguages.PrimaryLanguageOverride, RuntimeInformation.OSDescription, null);
        }

        private void LocalSettingsService_ThemeSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (args.Name == nameof(LocalSettingsService.Theme))
            {
                ApplyTheme();
            }
            else if (args.Name == nameof(LocalSettingsService.UIScale))
            {
                ApplyScale();
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

#if WINDOWS_UWP
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
#else
        private void CoreWindow_KeyDown(object sender, KeyRoutedEventArgs args)
        {
            var key = args.Key;
#endif
            if (key == VirtualKey.Escape || key == VirtualKey.GoBack || key == VirtualKey.GamepadB)
            {
                var frame = MainWindow.Content as Frame;
                if (frame != null)
                {
                    var page = frame.Content as BasePage<MainPageViewModel, BaseViewModel>;
                    if (page != null)
                    {
                        page.HandleBack();
                    }
                }

                args.Handled = true;
            }
        }

        #region Scale support
        protected static double ToDesiredScale(AppScale scale)
        {
            switch (scale)
            {
                case AppScale.Scale150:
                    return 1.5;
                case AppScale.Scale200:
                    return 2.0;
                case AppScale.Scale250:
                    return 2.5;
                case AppScale.Scale300:
                    return 3.0;
                case AppScale.Scale100:
                case AppScale.SystemDefault:
                default:
                    return DefaultScale;
            }
        }

        protected static double ComputeEffectiveScale(double desiredScale, double systemScale)
        {
            if (desiredScale <= 0)
            {
                desiredScale = DefaultScale;
            }

            if (systemScale <= 0)
            {
                systemScale = DefaultScale;
            }

            var s = desiredScale / systemScale;
            return s <= 0 ? DefaultScale : s;
        }

        protected static void ApplyContentScale(FrameworkElement content, double scale, Size clientSize)
        {
            if (content is null)
            {
                return;
            }

            if (scale <= 0)
            {
                scale = DefaultScale;
            }

            content.HorizontalAlignment = HorizontalAlignment.Left;
            content.VerticalAlignment = VerticalAlignment.Top;
            content.RenderTransformOrigin = new Point(0, 0);
            var st = content.RenderTransform as ScaleTransform ?? new ScaleTransform();
            st.ScaleX = scale;
            st.ScaleY = scale;
            content.RenderTransform = st;
            content.Width = clientSize.Width / scale;
            content.Height = clientSize.Height / scale;
            content.UpdateLayout();
        }

        protected static void ResetContentScale(FrameworkElement content)
        {
            if (content is null)
            {
                return;
            }

            content.RenderTransform = null;
            content.Width = double.NaN;
            content.Height = double.NaN;
            content.HorizontalAlignment = HorizontalAlignment.Stretch;
            content.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private static Page GetCurrentPage()
        {
            var frame = MainWindow?.Content as Frame;
            return frame?.Content as Page;
        }

        private void OnFrameNavigated(object sender, object e)
        {
            ApplyScale();
        }

        private void ApplyScale()
        {
            try
            {
                var page = GetCurrentPage();
                var scaleSetting = LocalSettingsService?.UIScale ?? AppScale.SystemDefault;

                if (scaleSetting == AppScale.SystemDefault)
                {
                    _manualScale = null;
                    var content = page?.Content as FrameworkElement;
                    if (content != null)
                    {
                        ResetContentScale(content);
                    }

                    DetachSizeChanged();
                    return;
                }

                EnsurePageReady(page, () =>
                {
                    var systemScale = GetSystemScale();
                    var desiredScale = ToDesiredScale(scaleSetting);
                    var effectiveScale = ComputeEffectiveScale(desiredScale, systemScale);
                    _manualScale = effectiveScale;

                    var content = page?.Content as FrameworkElement;
                    if (content != null)
                    {
                        var client = GetClientSize();
                        ApplyContentScale(content, effectiveScale, client);
                    }

                    AttachSizeChanged();
                });
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void UpdateScaledContentSize()
        {
            if (!_manualScale.HasValue)
            {
                return;
            }

            var page = GetCurrentPage();
            var content = page?.Content as FrameworkElement;
            if (content is null)
            {
                return;
            }

            var client = GetClientSize();
            var scale = _manualScale.Value <= 0 ? DefaultScale : _manualScale.Value;
            content.Width = client.Width / scale;
            content.Height = client.Height / scale;
            content.UpdateLayout();
        }

        private void AttachSizeChanged()
        {
            if (_sizeHandlerAttached)
            {
                return;
            }

            MainWindow.SizeChanged += OnWindowSizeChanged;
            _sizeHandlerAttached = true;
        }

        private void DetachSizeChanged()
        {
            if (!_sizeHandlerAttached)
            {
                return;
            }

            MainWindow.SizeChanged -= OnWindowSizeChanged;
            _sizeHandlerAttached = false;
        }

        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateScaledContentSize();
        }
        #endregion
    }
}
