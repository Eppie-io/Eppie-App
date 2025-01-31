#if WINDOWS_UWP

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Extensions
{
    public static partial class WebViewExtension
    {
        private static void EnableScripts(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject is WebView webView)
            {
                webView.Settings.IsJavaScriptEnabled = value;
            }
        }

        private static void NavigateToString(DependencyObject dependencyObject, string value)
        {
            if (dependencyObject is WebView webView)
            {
                webView.NavigateToString(value);
            }
        }
    }
}

#endif
