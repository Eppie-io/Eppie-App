using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Behaviors
{
    public class TreeViewItemInvokeBehavior : Behavior<TreeView>
    {
        public ICommand InvokeCommand
        {
            get { return (ICommand)GetValue(InvokeCommandProperty); }
            set { SetValue(InvokeCommandProperty, value); }
        }

        public static readonly DependencyProperty InvokeCommandProperty =
            DependencyProperty.Register(nameof(InvokeCommand), typeof(ICommand), typeof(TreeViewItemInvokeBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
            AssociatedObject.ItemInvoked -= OnItemInvoked;
            // End
            AssociatedObject.ItemInvoked += OnItemInvoked;
        }

        protected override void OnDetaching()
        {
            //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
            if (AssociatedObject.Parent == null)
            // End
            {
                AssociatedObject.ItemInvoked -= OnItemInvoked;
            }
        }

        private void OnItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            InvokeCommand?.Execute(args?.InvokedItem);
        }
    }
}
