using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class DecentralizedAccountSettingsPageBase : BasePage<DecentralizedAccountSettingsPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class DecentralizedAccountSettingsPage : DecentralizedAccountSettingsPageBase
    {
        public DecentralizedAccountSettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
