using Windows.Storage;


namespace Tuvi.App.Helpers
{
    public static class DataHelper
    {
        public static string GetAppDataPath()
        {
            return ApplicationData.Current.LocalFolder.Path;
        }
    }
}
