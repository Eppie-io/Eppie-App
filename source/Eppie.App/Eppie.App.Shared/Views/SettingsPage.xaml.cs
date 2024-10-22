using System.Collections.Generic;
using NBitcoin;
using Newtonsoft.Json.Linq;
using Tuvi.App.Shared.Models;
using Tuvi.App.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            LanguageSelector.InitSelection((Application.Current as App).LocalSettingsService.Language);
        }

        private void OnLanguageChanged(object sender, string language)
        {
            var message = ResourceLoader.GetForCurrentView().GetString("RestartApplication");
            ViewModel.ChangeLanguage(language, message);
        }
    }
}
