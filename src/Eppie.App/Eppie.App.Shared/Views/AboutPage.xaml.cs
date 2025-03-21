using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class AboutPageBase : BasePage<AboutPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class AboutPage : AboutPageBase
    {
        public AboutPage()
        {
            this.InitializeComponent();
        }
    }
}
