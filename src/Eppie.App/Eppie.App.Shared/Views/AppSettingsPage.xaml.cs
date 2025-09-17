// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

// ToDo: change namespace
namespace Tuvi.App.Shared.Views
{
    // ToDo: Issue #840 - Rename SettingsPageViewModel or replace it with a new one
    public partial class AppSettingsPageBase : BasePage<SettingsPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class AppSettingsPage : AppSettingsPageBase
    {
        public AppSettingsPage()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ThemeCombobox.SelectedIndex = 0;
            InitLanguage();
        }

        private void InitLanguage()
        {
            LanguageSelector.InitSelection((Application.Current as Eppie.App.Shared.App).LocalSettingsService.Language);
        }

        private void OnLanguageChanged(object sender, string language)
        {
            ViewModel.ChangeLanguage(language);
        }
    }
}
