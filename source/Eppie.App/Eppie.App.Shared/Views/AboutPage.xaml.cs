using Tuvi.App.ViewModels;
using Windows.ApplicationModel;

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

        public override void AfterDataContextChanged()
        {
            ViewModel.Version = string.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
        }
    }
}
