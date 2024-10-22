using Tuvi.App.Shared.Models;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

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
