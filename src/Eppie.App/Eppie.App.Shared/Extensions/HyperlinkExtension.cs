using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Extensions
{
	public static class HyperlinkExtension
    {
        public static string GetLink(DependencyObject obj)
        {
            return (string)obj?.GetValue(LinkProperty);
        }
        public static void SetLink(DependencyObject obj, string value)
        {
            obj?.SetValue(LinkProperty, value);
        }
        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.RegisterAttached("Link", typeof(string), typeof(HyperlinkExtension), new PropertyMetadata("", OnLinkPropertyChanged));
        private static void OnLinkPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyperlinkButton hyperlinkButton)
            {
                hyperlinkButton.NavigateUri = new Uri(e.NewValue as string);
            }
        }
    }
}
