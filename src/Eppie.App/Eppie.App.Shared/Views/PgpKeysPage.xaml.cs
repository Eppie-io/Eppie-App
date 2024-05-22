using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class PgpKeysPageBase : BasePage<PgpKeysPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class PgpKeysPage : PgpKeysPageBase
    {
        public PgpKeysPage()
        {
            InitializeComponent();
        }
    }
}
