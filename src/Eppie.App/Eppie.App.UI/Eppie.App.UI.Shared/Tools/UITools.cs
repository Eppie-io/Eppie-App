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
using Eppie.App.UI.Controls;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Common
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
                            Text = message,
                            IsTextSelectionEnabled = true
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

        public static async Task ShowPopupAsync<TPage>(XamlRoot root)
            where TPage : Page, IPopupPage
        {
            await _mutex.WaitAsync().ConfigureAwait(true);
            try
            {
                var dialog = new ContentDialog()
                {
                    XamlRoot = root,
                };

                var popupControl = new Eppie.App.UI.Controls.PopupHostControl
                {
                    PageType = typeof(TPage),
                };

                popupControl.CloseRequested += (s, e) => dialog.Hide();
                dialog.Content = popupControl;

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

        public static async Task<Action> ShowAuthenticationDialogAsync(string title, string content, string closeButtonText, XamlRoot root, Action onClose = null)
        {
            if (title is null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            await _mutex.WaitAsync().ConfigureAwait(true);

            var dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                CloseButtonText = closeButtonText,
                XamlRoot = root
            };

            if (onClose != null)
            {
                dialog.CloseButtonClick += (s, e) => onClose();
            }

            try
            {
                _ = dialog.ShowAsync();

                Action close = () =>
                {
                    dialog.Hide();
                    _mutex.Release();
                };

                dialog.Closed += (s, e) => close();

                return close;
            }
            catch
            {
                _mutex.Release();
                throw;
            }
        }

        public static async Task ShowRenameContactDialogAsync(
             string title,
             string primaryButtonText,
             string closeButtonText,
             string textBoxHeader,
             string initialText,
             XamlRoot root,
             Action<string> onRename)
        {
            if (onRename is null)
            {
                throw new ArgumentNullException(nameof(onRename));
            }

            await _mutex.WaitAsync().ConfigureAwait(true);
            try
            {
                var dialog = new ContentDialog()
                {
                    Title = title,
                    PrimaryButtonText = primaryButtonText,
                    CloseButtonText = closeButtonText,
                    XamlRoot = root
                };

                var stackPanel = new StackPanel { Spacing = 8 };
                var textBox = new TextBox { Header = textBoxHeader, Text = initialText ?? string.Empty, AcceptsReturn = false };
                textBox.SelectAll();
                stackPanel.Children.Add(textBox);
                dialog.Content = stackPanel;

                textBox.KeyDown += (s, e) =>
                {
                    if (e.Key == Windows.System.VirtualKey.Enter)
                    {
                        onRename(textBox.Text);
                        dialog.Hide();
                    }
                    else if (e.Key == Windows.System.VirtualKey.Escape)
                    {
                        dialog.Hide();
                    }
                };

                dialog.PrimaryButtonClick += (s, e) =>
                {
                    onRename(textBox.Text);
                };

                await dialog.ShowAsync();
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}
