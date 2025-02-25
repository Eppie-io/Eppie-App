using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else 
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class PasteFromClipboardBehavior : ClipboardBehaviorBase
    {
        public static readonly DependencyProperty PasteCommandProperty =
            DependencyProperty.Register(nameof(PasteCommand), typeof(ICommand), typeof(PasteFromClipboardBehavior), new PropertyMetadata(null));

        public ICommand PasteCommand
        {
            get { return (ICommand)GetValue(PasteCommandProperty); }
            set { SetValue(PasteCommandProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Click += PasteOnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= PasteOnClick;
        }

        private void PasteOnClick(object sender, RoutedEventArgs e)
        {
            PasteCommand?.Execute(this);
        }
    }
}
