using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Common
{
    public static class UITools
    {
        private static readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

        public static async Task ShowInfoMessageAsync(string title, string message, string closeButtonText)
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
                    CloseButtonText = closeButtonText
                };

                await messageDialog.ShowAsync();
            }
            finally
            {
                _mutex.Release();
            }            
        }

        public static async Task<bool> ShowDialogAsync(string title, string message, string acceptButtonText, string rejectButtonText)
        {
            await _mutex.WaitAsync().ConfigureAwait(true);

            try
            {
                var messageDialog = new ContentDialog()
                {
                    Content = message,
                    Title = title,
                    PrimaryButtonText = acceptButtonText,
                    CloseButtonText = rejectButtonText
                };

                var result = await messageDialog.ShowAsync();

                return result == ContentDialogResult.Primary;                
            }
            finally
            {
                _mutex.Release();
            }            
        }

        public static async Task<bool> ShowErrorMessageAsync(string title, string message, string acceptButtonText, string rejectButtonText)
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
                    CloseButtonText = rejectButtonText
                };

                var result = await messageDialog.ShowAsync();

                return result == ContentDialogResult.Primary;
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}
