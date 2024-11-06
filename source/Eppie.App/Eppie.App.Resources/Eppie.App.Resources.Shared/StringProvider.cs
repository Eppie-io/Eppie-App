using Windows.ApplicationModel.Resources;

namespace Eppie.App.Resources
{
    public class StringProvider
    {
        public static readonly string DefaultResourcesLibraryName = "/Eppie.App.Resources";
        private static readonly string DefaultResources = $"{DefaultResourcesLibraryName}/Resources";

        private ResourceLoader _resourceLoader;

        public static StringProvider GetInstance()
        {
            return new StringProvider();
        }

        public static StringProvider GetInstance(string name)
        {
            return new StringProvider(name);
        }

        public string GetString(string resource)
        {
            return _resourceLoader.GetString(resource);
        }

        private StringProvider()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse(DefaultResources);
        }

        private StringProvider(string name)
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse($"{DefaultResourcesLibraryName}{name}");
        }
    }
}
