// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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

using System;
using Eppie.App.Models;
using Eppie.App.Services;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.Helpers
{
    internal static class ViewModelExtensions
    {
        internal static void Initialize(this BaseViewModel viewModel)
        {
            if (Application.Current is App app)
            {
                viewModel.SetCoreProvider(() => app.Core);
                viewModel.SetAIServiceProvider(() => app.AIService);
                viewModel.SetNavigationService(app.NavigationService);
                viewModel.SetPendingMailtoService(app.PendingMailtoService);
                viewModel.SetLocalSettingsService(app.LocalSettingsService);
                viewModel.SetAuthProvider(app.AuthProvider);
                viewModel.SetProtonLoginHelper(app.ProtonLoginHelper);
                viewModel.SetLocalizationService(new LocalizationService(app.Host?.Services));
                viewModel.SetMessageService(new MessageService(() => App.XamlRoot));
                viewModel.SetErrorHandler(new ErrorHandler());
                viewModel.SetDispatcherService(new DispatcherService());
                viewModel.SetBrandService(new BrandLoader());
                viewModel.SetLauncherService(new LauncherService());
                viewModel.SetAppStoreService(new AppStoreService());
                viewModel.SetDragAndDropService(new DragAndDropService());
            }
        }
    }
}
