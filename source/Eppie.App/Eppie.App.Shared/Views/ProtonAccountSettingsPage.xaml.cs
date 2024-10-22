using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class ProtonAccountSettingsPageBase : BasePage<ProtonAccountSettingsPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class ProtonAccountSettingsPage : ProtonAccountSettingsPageBase
    {
        public ProtonAccountSettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
