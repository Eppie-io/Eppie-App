using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tuvi.App.ViewModels.Services;
using Uno.Extensions.Specialized;
using Windows.ApplicationModel;

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
                {"License", "https://eppie.io"}
            };
        }
    }

    internal class BrandLoader : IBrandService
    {
        private BrandInfo _loader;
        internal BrandLoader()
        {
            if (Loader_Eppie.NameIds.Contains(Package.Current.Id.Name))
            {
                _loader = new Loader_Eppie();
            }
            else
            {
                _loader = new Loader_Eppie();
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
    }
}
