using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class PasswordPageBase : BasePage<PasswordPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class PasswordPage : PasswordPageBase
    {
        public PasswordPage()
        {
            this.InitializeComponent();
        }
    }
}
