using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class SelectEmailProviderPageBase : BasePage<SelectEmailProviderPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class SelectEmailProviderPage : SelectEmailProviderPageBase
    {
        public SelectEmailProviderPage()
        {
            this.InitializeComponent();
        }
    }
}
