#if WINDOWS_UWP

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Tuvi.App.Shared.Models;
using Tuvi.App.Shared.Services;
using Tuvi.App.Shared.Views;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using Tuvi.OAuth2;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Services.Store.Engagement;

namespace Eppie.App.Shared
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    rootFrame = CreateRootFrame();

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }

                if (e.PrelaunchActivated == false)
                {
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
                    Window.Current.Activate();
                }
            }
            catch (Exception exception)
            {
                OnError(exception);
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

        private Frame CreateRootFrame()
        {
            var frame = new Frame();
            frame.NavigationFailed += OnNavigationFailed;

            // ToDo: use nameof(Tuvi.App.Shared.Views) and add dot(.) inside NavigationService
            NavigationService = new NavigationService(frame, "Tuvi.App.Shared.Views.");

            _errorHandler = new ErrorHandler();
            _errorHandler.SetMessageService(new MessageService());

            return frame;
        }

        private void SubscribeToPlatformSpecificEvents()
        {
            Suspending += OnSuspending;
        }

        private async void InitializeNotifications()
        {
            try
            {
                StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
                await engagementManager.RegisterNotificationChannelAsync();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            try
            {
                if (args is ToastNotificationActivatedEventArgs)
                {
                    var toastActivationArgs = args as ToastNotificationActivatedEventArgs;

                    StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
                    string originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(
                        toastActivationArgs.Argument);
                }

                base.OnActivated(args);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}

#endif
