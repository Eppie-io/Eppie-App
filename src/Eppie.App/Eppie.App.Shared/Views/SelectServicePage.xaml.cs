using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class SelectServicePageBase : BasePage<SelectServicePageViewModel, BaseViewModel>
    {
    }

    public sealed partial class SelectServicePage : SelectServicePageBase
    {
        public SelectServicePage()
        {
            this.InitializeComponent();
        }
    }
}
