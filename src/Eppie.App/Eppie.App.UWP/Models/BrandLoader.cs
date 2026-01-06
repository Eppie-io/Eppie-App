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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Tuvi.App.ViewModels.Services;
using Windows.ApplicationModel;

namespace Eppie.App.Models
{
    class BrandInfo
    {
        public Dictionary<string, string> Values;
    }

    class Loader_Eppie : BrandInfo
    {
        public static readonly string[] NameIds = new string[] { "<NAMEID>", "<NAMEID>" };
        public Loader_Eppie()
        {
            Values = new Dictionary<string, string>()
            {
                {"AppName", "Eppie (preview)"},
                {"Support", "beta@eppie.io"},
                {"Homepage", "https://eppie.io"},
                {"License", "https://eppie.io"},
                {"DevelopmentSupport", "https://github.com/sponsors/Eppie-io"},
                {"TwitterHandle", "@EppieApp"},
                {"GitHub", "https://github.com/Eppie-io/Eppie-App"},
                {"Translation", "https://eppie.crowdin.com/eppie"}
            };
        }
    }

    class Loader_Eppie_Dev : BrandInfo
    {
        public static readonly string[] NameIds = new string[] { "<NAMEID>", "<NAMEID>" };
        public Loader_Eppie_Dev()
        {
            Values = new Dictionary<string, string>()
            {
                {"AppName", "Eppie (development)"},
                {"Support", "beta@eppie.io"},
                {"Homepage", "https://eppie.io"},
                {"License", "https://eppie.io"},
                {"DevelopmentSupport", "https://github.com/sponsors/Eppie-io"},
                {"TwitterHandle", "@EppieApp"},
                {"GitHub", "https://github.com/Eppie-io/Eppie-App"},
                {"Translation", "https://eppie.crowdin.com/eppie"}
            };
        }
    }

    internal class BrandLoader : IBrandService
    {
        private readonly BrandInfo _loader;
        internal BrandLoader()
        {
            if (Loader_Eppie.NameIds.Contains(Package.Current.Id.Name))
            {
                _loader = new Loader_Eppie();
            }
            else
            {
                _loader = new Loader_Eppie_Dev();
            }
        }

        internal string GetString(string key)
        {
            if (_loader.Values.TryGetValue(key, out var res))
            {
                return res;
            }
            else
            {
                Debug.Assert(false, $"Brand key '{key}' is not found");
                return default;
            }
        }

        public string GetName()
        {
            return GetString("AppName");
        }

        public string GetSupport()
        {
            return GetString("Support");
        }

        public string GetLicense()
        {
            return GetString("License");
        }

        public string GetHomepage()
        {
            return GetString("Homepage");
        }

        public string GetPublisherDisplayName()
        {
            return Package.Current.PublisherDisplayName;
        }

        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }

        public string GetPackageVersion()
        {
            return $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
        }

        public string GetFileVersion()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        }

        public string GetInformationalVersion()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        }

        public string GetAppVersion()
        {
            return GetInformationalVersion() ?? GetVersion() ?? GetPackageVersion();
        }

        public string GetDevelopmentSupport()
        {
            return GetString("DevelopmentSupport");
        }

        public string GetTwitterHandle()
        {
            return GetString("TwitterHandle");
        }

        public string GetGitHub()
        {
            return GetString("GitHub");
        }

        public string GetTranslation()
        {
            return GetString("Translation");
        }
    }
}
