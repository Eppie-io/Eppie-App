using Tuvi.App.ViewModels;
using Windows.ApplicationModel.Resources;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Views
{
    public partial class SettingsPageBase : BasePage<SettingsPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class SettingsPage : SettingsPageBase
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitLanguage();
        }

        private void InitLanguage()
        {
            LanguageSelector.InitSelection((Application.Current as Eppie.App.Shared.App).LocalSettingsService.Language);
        }

        private void OnLanguageChanged(object sender, string language)
        {
            var message = ResourceLoader.GetForCurrentView().GetString("RestartApplication");
            ViewModel.ChangeLanguage(language, message);
        }
    }
}
