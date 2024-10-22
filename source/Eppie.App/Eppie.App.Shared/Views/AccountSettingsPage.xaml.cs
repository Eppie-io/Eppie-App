using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

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

            ViewModel.SetAuthProvider((Application.Current as Eppie.App.Shared.App)?.AuthProvider);
        }
    }
}
