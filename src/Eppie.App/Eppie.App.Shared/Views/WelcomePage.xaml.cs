using Tuvi.App.Shared.Models;
using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class WelcomePageBase : BasePage<WelcomePageViewModel, BaseViewModel>
    {
    }

    public sealed partial class WelcomePage : WelcomePageBase
    {
        public string LicenseLink
        {
            get
            {
                var brand = new BrandLoader();
                return brand.GetLicenseLink();
            }
        }

        public WelcomePage()
        {
            this.InitializeComponent();
        }
    }
}
