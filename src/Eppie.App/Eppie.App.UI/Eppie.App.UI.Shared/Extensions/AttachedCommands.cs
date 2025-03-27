using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Tuvi.App.Shared.Extensions
{
    public static class AttachedCommands
    {
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.RegisterAttached("ClickCommand", typeof(ICommand), typeof(AttachedCommands), new PropertyMetadata(null));

        public static void SetClickCommand(DependencyObject d, ICommand value)
        {
            d?.SetValue(ClickCommandProperty, value);
        }

        public static ICommand GetClickCommand(DependencyObject d)
        {
            return (ICommand)d?.GetValue(ClickCommandProperty);
        }
    }
}
