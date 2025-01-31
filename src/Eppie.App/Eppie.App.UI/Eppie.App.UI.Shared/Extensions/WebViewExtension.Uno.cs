#if !WINDOWS_UWP

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Extensions
{
    public static partial class WebViewExtension
    {
        private static async void EnableScripts(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject is WebView2 webView)
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.Settings.IsScriptEnabled = value;
            }
        }

        private static async void NavigateToString(DependencyObject dependencyObject, string value)
        {
            if (dependencyObject is WebView2 webView)
            {
                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString(value);
            }
        }
    }
}

#endif
