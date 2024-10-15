using System;
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
    public sealed partial class EmailProvidersListControl : UserControl
    {
        public ObservableCollection<EmailProviderInfo> Items
        {
            get { return (ObservableCollection<EmailProviderInfo>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<EmailProviderInfo>), typeof(EmailProvidersListControl), new PropertyMetadata(null));
        
        public EmailProviderInfo SelectedItem
        {
            get { return (EmailProviderInfo)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(EmailProviderInfo), typeof(EmailProvidersListControl), new PropertyMetadata(null));
        
        public EmailProvidersListControl()
        {
            this.InitializeComponent();
        }
    }
}
