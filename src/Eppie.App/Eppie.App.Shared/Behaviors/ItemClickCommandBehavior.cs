using System.Linq;
using Microsoft.Xaml.Interactivity;
using Tuvi.App.Shared.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Behaviors
{
    public class ItemClickCommandBehavior : Behavior<ListViewBase>
    {
        private bool _isItemClickEnabledCachedValue;
        protected override void OnAttached()
        {
            _isItemClickEnabledCachedValue = AssociatedObject.IsItemClickEnabled;

            AssociatedObject.IsItemClickEnabled = true;
            AssociatedObject.ItemClick += OnItemClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ItemClick -= OnItemClick;
            AssociatedObject.IsItemClickEnabled = _isItemClickEnabledCachedValue;
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            // for Android and iOS (in UWP ClickedItem is String)
            var dependencyObject = e.ClickedItem as DependencyObject;
            if (dependencyObject == null && sender is ListViewBase listView)
            {
                // for UWP
                dependencyObject = listView.Items.OfType<ContentControl>().FirstOrDefault(navItem => (string)navItem.Content == (string)e.ClickedItem);
            }

            if (dependencyObject != null)
            {
                AttachedCommands.GetClickCommand(dependencyObject)?.Execute(null);
            }
        }
    }
}
