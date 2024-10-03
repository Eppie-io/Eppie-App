using Tuvi.App.ViewModels.Services;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else 
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.Shared
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public INavigationService NavigationService { get; private set; }
    }
}
