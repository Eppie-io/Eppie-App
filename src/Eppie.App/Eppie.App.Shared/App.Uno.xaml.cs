#if !WINDOWS_UWP

using Tuvi.App.Shared.Models;
using Tuvi.App.Shared.Services;
using Tuvi.App.Shared.Views;
using Tuvi.App.ViewModels;
using Uno.Resizetizer;

namespace Eppie.App.Shared
{
    public partial class App : Application
    {
        protected Window MainWindow { get; private set; }
        protected IHost Host { get; private set; }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                var builder = this.CreateBuilder(args)
                    .Configure(host => host
#if DEBUG
                        // Switch to Development environment when running in DEBUG
                        .UseEnvironment(Environments.Development)
#endif
                        .UseLogging(configure: (context, logBuilder) =>
                        {
                            // Configure log levels for different categories of logging
                            logBuilder
                                .SetMinimumLevel(
                                    context.HostingEnvironment.IsDevelopment() ?
                                        LogLevel.Information :
                                        LogLevel.Warning)

                                // Default filters for core Uno Platform namespaces
                                .CoreLogLevel(LogLevel.Warning);

                            // Uno Platform namespace filter groups
                            // Uncomment individual methods to see more detailed logging
                            //// Generic Xaml events
                            //logBuilder.XamlLogLevel(LogLevel.Debug);
                            //// Layout specific messages
                            //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                            //// Storage messages
                            //logBuilder.StorageLogLevel(LogLevel.Debug);
                            //// Binding related messages
                            //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                            //// Binder memory references tracking
                            //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                            //// DevServer and HotReload related
                            //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                            //// Debug JS interop
                            //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

                        }, enableUnoLogging: true)
                        .UseSerilog(consoleLoggingEnabled: true, fileLoggingEnabled: true)
                        .UseConfiguration(configure: configBuilder =>
                            configBuilder
                                .EmbeddedSource<App>()
                                .Section<AppConfig>()
                        )
                        // Enable localization (see appsettings.json for supported languages)
                        .UseLocalization()
                        .ConfigureServices((context, services) =>
                        {
                            // TODO: Register your services
                            //services.AddSingleton<IMyService, MyService>();
                        })
                    );
                MainWindow = builder.Window;

#if WINDOWS
                var brand = new BrandLoader();
                MainWindow.Title = brand.GetName();
#endif

#if DEBUG
                MainWindow.UseStudio();
#endif
                MainWindow.SetWindowIcon();

                Host = builder.Build();

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (MainWindow.Content is not Frame rootFrame)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = CreateRootFrame();

                    // Place the frame in the current Window
                    MainWindow.Content = rootFrame;
                }

                if (rootFrame.Content == null)
                {
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
                MainWindow.Activate();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
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

        private void SubscribeToPlatformSpecificEvents()
        {

        }

        private void InitializeNotifications()
        {

        }
    }
}

#endif
