using Tuvi.App.Shared.Models;
using Tuvi.App.Shared.Services;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core;
using Tuvi.OAuth2;

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
        private static string DataBaseFileName = "TuviMail.db";

        public INavigationService NavigationService { get; private set; }
        public ITuviMail Core { get; private set; }
        public ILocalSettingsService LocalSettingsService { get; private set; }
        public AuthorizationProvider AuthProvider { get; private set; }
        private NotificationManager _notificationManager { get; set; }

        private ErrorHandler _errorHandler;
    }
}
