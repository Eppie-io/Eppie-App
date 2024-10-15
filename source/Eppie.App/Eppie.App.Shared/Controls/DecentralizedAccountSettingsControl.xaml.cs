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
    public sealed partial class DecentralizedAccountSettingsControl : UserControl
    {
        public DecentralizedAccountSettingsModel AccountSettingsModel
        {
            get { return (DecentralizedAccountSettingsModel)GetValue(AccountSettingsModelProperty); }
            set { SetValue(AccountSettingsModelProperty, value); }
        }
        public static readonly DependencyProperty AccountSettingsModelProperty =
            DependencyProperty.Register(nameof(AccountSettingsModel), typeof(DecentralizedAccountSettingsModel), typeof(DecentralizedAccountSettingsControl), new PropertyMetadata(null));

        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(DecentralizedAccountSettingsControl), new PropertyMetadata(false));

        public DecentralizedAccountSettingsControl()
        {
            this.InitializeComponent();
        }
    }
}
