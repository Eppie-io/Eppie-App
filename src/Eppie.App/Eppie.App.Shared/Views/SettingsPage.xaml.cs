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

namespace Tuvi.App.Shared.Views
{
    // ToDo: Issue #840 - It can be deleted
    public partial class SettingsPageBase : BasePage<SettingsPageViewModel, BaseViewModel>
    {
    }

    // ToDo: Issue #840 - It can be deleted
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
            ViewModel.ChangeLanguage(language);
        }
    }
}
