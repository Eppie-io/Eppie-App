using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class PgpKeyPageBase : BasePage<PgpKeyPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class PgpKeyPage : PgpKeyPageBase
    {
        public PgpKeyPage()
        {
            this.InitializeComponent();
        }
    }
}
