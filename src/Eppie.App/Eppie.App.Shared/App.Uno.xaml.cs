// ---------------------------------------------------------------------------- //
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
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                MainWindow = new Window();

                var brand = new BrandLoader();
                MainWindow.Title = brand.GetName();

#if DEBUG
                MainWindow.UseStudio();
#endif
                MainWindow.SetWindowIcon();

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
