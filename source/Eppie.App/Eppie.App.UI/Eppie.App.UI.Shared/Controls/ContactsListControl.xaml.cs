using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class ContactsListControl : UserControl
    {
        public ContactsModel ContactsModel
        {
            get { return (ContactsModel)GetValue(ContactsModelProperty); }
            set { SetValue(ContactsModelProperty, value); }
        }
        public static readonly DependencyProperty ContactsModelProperty =
            DependencyProperty.Register(nameof(ContactsModel), typeof(ContactsModel), typeof(ContactsListControl), new PropertyMetadata(null));

        public ContactsListControl()
        {
            this.InitializeComponent();
        }

        private void ChangeContactAvatarMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                ContactsModel?.ChangeContactAvatarCommand?.Execute(contactItem);
            }
        }

        private void RemoveContactMenuItemClick(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is ContactItem contactItem)
            {
                ContactsModel?.RemoveContactCommand?.Execute(contactItem);
            }
        }
    }
}
