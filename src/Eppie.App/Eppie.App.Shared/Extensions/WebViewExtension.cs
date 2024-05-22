using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Extensions
{
	public static class WebViewExtension
	{
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
        private static void OnStringSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView webView)
            {
                webView.NavigateToString(e.NewValue as string ?? "");
            }
        }
    }
}
