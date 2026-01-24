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

#if WINDOWS_UWP

using Eppie.App.Authorization;
using Eppie.App.Views;
using Microsoft.Services.Store.Engagement;
using System;
using System.Linq;
using Tuvi.App.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Eppie.App
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
                Frame rootFrame = EnsureRootFrame();

                if (e.PrelaunchActivated == false)
                {
                    if (rootFrame.Content is null)
                    {
                        await NavigateToStartPage(rootFrame).ConfigureAwait(true);
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

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            try
            {
                Frame rootFrame = EnsureRootFrame();

                if (args is ToastNotificationActivatedEventArgs)
                {
                    var toastActivationArgs = args as ToastNotificationActivatedEventArgs;

                    StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
                    string originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(
                        toastActivationArgs.Argument);
                }

                if (args is ProtocolActivatedEventArgs protocolArgs)
                {
                    HandleProtocolActivation(protocolArgs);
                }

                if (rootFrame.Content is null)
                {
                    await NavigateToStartPage(rootFrame).ConfigureAwait(true);
                }

                // Ensure the current window is active
                Window.Current.Activate();

                base.OnActivated(args);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private Frame EnsureRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            MainWindow = Window.Current;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame is null)
            {
                rootFrame = CreateRootFrame();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
                Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            }

            return rootFrame;
        }

        private async System.Threading.Tasks.Task NavigateToStartPage(Frame rootFrame)
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

        private void HandleProtocolActivation(ProtocolActivatedEventArgs args)
        {
            if (args.Uri.Scheme == AuthConfig.UriScheme)
            {
                ProtocolAuthenticationBroker.CompleteAuthentication(args.Uri);
            }
            else if (string.Equals(args.Uri.Scheme, Tuvi.App.ViewModels.Helpers.MailtoUriParser.MailtoScheme, StringComparison.OrdinalIgnoreCase))
            {
                HandleMailtoActivation(args.Uri);
            }
        }

        private async void HandleMailtoActivation(Uri mailtoUri)
        {
            try
            {
                // Get the default email account
                var accounts = await Core.GetAccountsAsync().ConfigureAwait(true);
                var defaultAccount = accounts.FirstOrDefault();

                if (defaultAccount is null)
                {
                    // No accounts configured, just open the app normally
                    return;
                }

                // Create message data from mailto URI
                var messageData = Tuvi.App.ViewModels.Common.MailtoMessageData.FromMailtoUri(
                    mailtoUri, 
                    defaultAccount.Email);

                // Navigate to the new message page with the pre-filled data
                NavigationService.Navigate("NewMessagePageViewModel", messageData);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void ConfigurePreferredMinimumSize()
        {
            // https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.applicationview.setpreferredminsize?view=winrt-26100#remarks
            // The largest allowed minimum size is 500 x 500 effective pixels.
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(MinWidth, MinHeight));
        }

        protected Size GetClientSize()
        {
            try
            {
                var view = ApplicationView.GetForCurrentView();
                var vb = view.VisibleBounds;
                if (vb.Width > 0 && vb.Height > 0)
                {
                    return new Size(vb.Width, vb.Height);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            var b = Window.Current.Bounds;
            return new Size(b.Width, b.Height);
        }

        protected double GetSystemScale()
        {
            try
            {
                var di = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
                var scale = di.RawPixelsPerViewPixel;
                return scale > 0 ? scale : DefaultScale;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            return DefaultScale;
        }

        private void EnsurePageReady(Page page, Action whenReady)
        {
            whenReady?.Invoke();
        }
    }
}

#endif
