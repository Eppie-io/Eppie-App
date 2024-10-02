#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Extensions
{
	public static class WebViewExtension
	{
        public static string GetStringSourceWithDisabledJavaScript(DependencyObject obj)
        {
            return (string)obj?.GetValue(StringSourceWithDisabledJavaScriptProperty);
        }

        public static void SetStringSourceWithDisabledJavaScript(DependencyObject obj, string value)
        {
            obj?.SetValue(StringSourceWithDisabledJavaScriptProperty, value);
        }

        public static readonly DependencyProperty StringSourceWithDisabledJavaScriptProperty =
            DependencyProperty.RegisterAttached("StringSourceWithDisabledJavaScript", typeof(string), typeof(WebViewExtension), new PropertyMetadata("", OnStringSourceWithDisabledJavaScriptPropertyChanged));
        private static void OnStringSourceWithDisabledJavaScriptPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#if WINDOWS_UWP
            if (d is WebView webView)
            {
                webView.Settings.IsJavaScriptEnabled = false;

                webView.NavigateToString(e.NewValue as string ?? "");
            }
#else
            throw new NotImplementedException();
#endif
        }
    }
}
