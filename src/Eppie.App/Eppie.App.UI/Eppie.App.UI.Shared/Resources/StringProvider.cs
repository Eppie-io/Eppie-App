using Windows.ApplicationModel.Resources;

namespace Eppie.App.UI.Resources
{
    public class StringProvider
    {
        private readonly ResourceLoader _resourceLoader;

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
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
        }

        private StringProvider(string name)
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse(name);
        }
    }
}
