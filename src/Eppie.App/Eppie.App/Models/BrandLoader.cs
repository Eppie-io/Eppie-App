using System.Diagnostics;
using System.Reflection;
using Tuvi.App.ViewModels.Services;
using Uno.Extensions.Specialized;

namespace Tuvi.App.Shared.Models
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
                {"DevelopmentSupport", "https://github.com/sponsors/Eppie-io"}
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
                {"DevelopmentSupport", "https://github.com/sponsors/Eppie-io"}
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

        public string GetLicenseLink()
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
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        }

        public string GetInformationalVersion()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public string GetAppVersion()
        {
            return GetInformationalVersion() ?? GetVersion() ?? GetPackageVersion();
        }

        public string GetDevelopmentSupport()
        {
            return GetString("DevelopmentSupport");
        }
    }
}
