using System.Windows.Input;
using Windows.UI.Xaml;

namespace Tuvi.App.Shared.Behaviors
{
    public class CopyToClipboardBehavior : ClipboardBehaviorBase
    {
        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register(nameof(CopyCommand), typeof(ICommand), typeof(CopyToClipboardBehavior), new PropertyMetadata(null));

        public ICommand CopyCommand
        {
            get { return (ICommand)GetValue(CopyCommandProperty); }
            set { SetValue(CopyCommandProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Click += CopyOnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= CopyOnClick;
        }

        private void CopyOnClick(object sender, RoutedEventArgs e)
        {
            CopyCommand?.Execute(this);
        }
    }
}
