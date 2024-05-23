using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class IdentityManagerPageBase : BasePage<IdentityManagerPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class IdentityManagerPage : IdentityManagerPageBase
    {
        public ICommand ConnectServiceCommand => new RelayCommand(() => Frame?.Navigate(typeof(SelectEmailProviderPage)));

        public IdentityManagerPage()
        {
            this.InitializeComponent();
        }
    }
}
