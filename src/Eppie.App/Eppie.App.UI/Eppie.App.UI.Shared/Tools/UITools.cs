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

using System;
using System.Threading;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Common
{
    public static class UITools
    {
        private static readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

        public static async Task ShowInfoMessageAsync(string title, string message, string closeButtonText, XamlRoot root)
        {
            await _mutex.WaitAsync().ConfigureAwait(true);

            try
            {
                var messageDialog = new ContentDialog()
                {
                    Content = new ScrollViewer()
                    {
                        Content = new TextBlock()
                        {
                            Margin = new Thickness(0, 0, 20, 0),
                            TextWrapping = TextWrapping.WrapWholeWords,
                            Text = message
                        }
                    },
                    Title = title,
                    CloseButtonText = closeButtonText,
                    XamlRoot = root
                };

                await messageDialog.ShowAsync();
            }
            finally
            {
                _mutex.Release();
            }
        }

        public static async Task<bool> ShowDialogAsync(string title, string message, string acceptButtonText, string rejectButtonText, XamlRoot root)
        {
            await _mutex.WaitAsync().ConfigureAwait(true);

            try
            {
                var messageDialog = new ContentDialog()
                {
                    Content = message,
                    Title = title,
                    PrimaryButtonText = acceptButtonText,
                    CloseButtonText = rejectButtonText,
                    XamlRoot = root
                };

                var result = await messageDialog.ShowAsync();

                return result == ContentDialogResult.Primary;
            }
            finally
            {
                _mutex.Release();
            }
        }

        public static async Task<bool> ShowErrorMessageAsync(string title, string message, string acceptButtonText, string rejectButtonText, XamlRoot root)
        {
            await _mutex.WaitAsync().ConfigureAwait(true);

            try
            {
                var messageDialog = new ContentDialog()
                {
                    Content = new ScrollViewer()
                    {
                        Content = new TextBlock()
                        {
                            Margin = new Thickness(0, 0, 20, 0),
                            TextWrapping = TextWrapping.WrapWholeWords,
                            Text = message
                        }
                    },
                    Title = title,
                    PrimaryButtonText = acceptButtonText,
                    CloseButtonText = rejectButtonText,
                    XamlRoot = root
                };

                var result = await messageDialog.ShowAsync();

                return result == ContentDialogResult.Primary;
            }
            finally
            {
                _mutex.Release();
            }
        }

        public static async Task ShowWhatsNewDialogAsync(string version,
                                                         bool isStorePaymentProcessor,
                                                         bool isSupportDevelopmentButtonVisible,
                                                         string price,
                                                         System.Windows.Input.ICommand supportDevelopmentCommand,
                                                         string twitterUrl,
                                                         XamlRoot root)
        {
            await _mutex.WaitAsync().ConfigureAwait(true);
            try
            {
                var dialog = new ContentDialog()
                {
                    XamlRoot = root
                };

                var whatsNew = new Eppie.App.UI.Controls.WhatsNewControl
                {
                    Version = version,
                    IsStorePaymentProcessor = isStorePaymentProcessor,
                    IsSupportDevelopmentButtonVisible = isSupportDevelopmentButtonVisible,
                    Price = price,
                    SupportDevelopmentCommand = supportDevelopmentCommand,
                    TwitterUrl = twitterUrl,
                    Margin = new Thickness(-8)
                };

                whatsNew.CloseRequested += (s, e) => dialog.Hide();
                dialog.Content = whatsNew;

                await dialog.ShowAsync();
            }
            finally
            {
                _mutex.Release();
            }
        }

        public static async Task ShowSupportDevelopmentDialogAsync(bool isStorePaymentProcessor,
                                                                   string price,
                                                                   System.Windows.Input.ICommand supportDevelopmentCommand,
                                                                   XamlRoot root)
        {
            await _mutex.WaitAsync().ConfigureAwait(true);
            try
            {
                var dialog = new ContentDialog()
                {
                    XamlRoot = root
                };

                var supportDevelopment = new Eppie.App.UI.Controls.SupportDevelopmentControl
                {
                    IsStorePaymentProcessor = isStorePaymentProcessor,
                    Price = price,
                    SupportDevelopmentCommand = supportDevelopmentCommand,
                    IsIconVisible = true,
                    IsCloseButtonVisible = true,
                    Margin = new Thickness(-8)
                };

                supportDevelopment.CloseRequested += (s, e) => dialog.Hide();
                dialog.Content = supportDevelopment;

                await dialog.ShowAsync();
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}
