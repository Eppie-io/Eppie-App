#if WINDOWS_UWP

using Tuvi.App.ViewModels.Services;
using Windows.UI.Xaml;

namespace Tuvi.App.Shared.Services
{
    public partial class MessageService : ITuviMailMessageService
    {
        private INavigationService GetNavigationService()
        {
            return (Application.Current as Eppie.App.UWP.App).NavigationService;
        }
    }
}

#endif
