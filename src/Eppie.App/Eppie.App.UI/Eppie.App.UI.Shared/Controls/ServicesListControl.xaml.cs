using System.Collections.ObjectModel;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class ServicesListControl : UserControl
    {
        public ObservableCollection<ServiceInfo> Items
        {
            get { return (ObservableCollection<ServiceInfo>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<ServiceInfo>), typeof(ServicesListControl), new PropertyMetadata(null));

        public ServiceInfo SelectedItem
        {
            get { return (ServiceInfo)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(ServiceInfo), typeof(ServicesListControl), new PropertyMetadata(null));

        public ServicesListControl()
        {
            this.InitializeComponent();
        }
    }
}
