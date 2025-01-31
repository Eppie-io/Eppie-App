using System;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Tuvi.App.Shared.Extensions
{
    public static partial class WebViewExtension
    {
        public static bool GetIsScriptEnabled(DependencyObject obj)
        {
            return (bool)obj?.GetValue(IsScriptEnabledProperty);
        }

        public static void SetIsScriptEnabled(DependencyObject obj, bool value)
        {
            obj?.SetValue(IsScriptEnabledProperty, value);
        }

        public static readonly DependencyProperty IsScriptEnabledProperty =
            DependencyProperty.RegisterAttached("IsScriptEnabled", typeof(bool), typeof(WebViewExtension), new PropertyMetadata(true, OnIsScriptEnabledPropertyChanged));

        private static void OnIsScriptEnabledPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool value)
            {
                EnableScripts(dependencyObject, value);
            }
        }

        public static string GetStringSource(DependencyObject obj)
        {
            return (string)obj?.GetValue(StringSourceProperty);
        }

        public static void SetStringSource(DependencyObject obj, string value)
        {
            obj?.SetValue(StringSourceProperty, value);
        }

        public static readonly DependencyProperty StringSourceProperty =
            DependencyProperty.RegisterAttached("StringSource", typeof(string), typeof(WebViewExtension), new PropertyMetadata("", OnStringSourcePropertyChanged));

        private static void OnStringSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            string value = args.NewValue as string ?? string.Empty;
            NavigateToString(dependencyObject, value);
        }
    }
}
