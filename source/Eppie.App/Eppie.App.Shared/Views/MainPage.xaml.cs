#if WINDOWS_UWP
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Shared.Views
{
    public sealed partial class MainPage : Page
    {

        internal string AppVersion => GetAppVersion();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private static string GetAppVersion()
        {
            PackageVersion version = Package.Current.Id.Version;
            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
