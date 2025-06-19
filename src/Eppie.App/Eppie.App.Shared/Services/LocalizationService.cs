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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Windows.Globalization;

namespace Tuvi.App.Shared.Services
{
    public class LocalizationService : Tuvi.App.ViewModels.Services.ILocalizationService
    {
        public IReadOnlyList<string> ManifestLanguages => GetManifestLanguages();

        private IServiceProvider ServiceProvider { get; }

        public LocalizationService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public string GetString(string resource)
        {
            return Eppie.App.UI.Resources.StringProvider.GetInstance().GetString(resource);
        }

        private IReadOnlyList<string> GetManifestLanguages()
        {
#if WINDOWS10_0_19041_0_OR_GREATER || WINDOWS_UWP
            return ApplicationLanguages.ManifestLanguages;
#else
            var localizationService = ServiceProvider.GetService<Uno.Extensions.Localization.ILocalizationService>();
            return localizationService.SupportedCultures.Select(culture => culture.Name).ToList();
#endif
        }
    }
}
