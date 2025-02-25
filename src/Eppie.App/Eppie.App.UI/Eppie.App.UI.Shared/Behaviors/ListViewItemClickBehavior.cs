using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class ListViewItemClickBehavior : Behavior<ListViewBase>
    {
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(ListViewItemClickBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
            AssociatedObject.ItemClick -= OnItemClick;
            // End
            AssociatedObject.ItemClick += OnItemClick;
        }

        protected override void OnDetaching()
        {
            //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
            if (AssociatedObject.Parent == null)
            // End
            {
                AssociatedObject.ItemClick -= OnItemClick;
            }
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (ClickCommand != null && ClickCommand.CanExecute(e?.ClickedItem))
            {
                ClickCommand.Execute(e?.ClickedItem);
            }
        }
    }
}
