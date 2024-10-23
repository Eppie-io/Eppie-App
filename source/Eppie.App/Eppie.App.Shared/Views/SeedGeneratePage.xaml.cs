using Tuvi.App.Shared.Models;
using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class SeedGeneratePageBase : BasePage<SeedGeneratePageViewModel, BaseViewModel>
    {
    }

    public sealed partial class SeedGeneratePage : SeedGeneratePageBase
    {
        public SeedGeneratePage()
        {
            this.InitializeComponent();
        }
    }
}
