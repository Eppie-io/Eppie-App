using Tuvi.App.ViewModels;
using Windows.UI.Xaml;

namespace Tuvi.App.Shared.Views
{
    public partial class AccountSettingsPageBase : BasePage<AccountSettingsPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class AccountSettingsPage : AccountSettingsPageBase
    {
        public AccountSettingsPage()
        {
            this.InitializeComponent();

            ViewModel.SetAuthProvider((Application.Current as App)?.AuthProvider);
        }
    }
}
