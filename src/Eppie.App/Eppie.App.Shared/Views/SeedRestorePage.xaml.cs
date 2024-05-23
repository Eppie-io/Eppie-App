using Tuvi.App.Shared.Models;
using Tuvi.App.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Views
{
    public partial class SeedRestorePageBase : BasePage<SeedRestorePageViewModel, BaseViewModel>
    {
    }

    public sealed partial class SeedRestorePage : SeedRestorePageBase
    {   
        public SeedRestorePage()
        {
            this.InitializeComponent();
        }

        private void OnChangingSeed(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ViewModel.OnSeedPhraseChanging(sender.Text);
        }
    }
}
