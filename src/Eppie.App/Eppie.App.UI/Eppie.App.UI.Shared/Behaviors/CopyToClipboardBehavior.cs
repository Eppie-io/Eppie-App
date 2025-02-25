using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else 
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.UI.Behaviors
{
    // ToDo: remove it [Eppie-io/Eppie-App#561]
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
