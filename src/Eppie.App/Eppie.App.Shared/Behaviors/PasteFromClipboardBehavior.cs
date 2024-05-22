using System.Windows.Input;
using Windows.UI.Xaml;

namespace Tuvi.App.Shared.Behaviors
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
