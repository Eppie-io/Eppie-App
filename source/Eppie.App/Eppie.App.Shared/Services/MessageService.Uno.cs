#if !WINDOWS_UWP

using Microsoft.UI.Xaml;
using Tuvi.App.ViewModels.Services;

namespace Tuvi.App.Shared.Services
{
    public partial class MessageService : ITuviMailMessageService
    {
        private INavigationService GetNavigationService()
        {
            return (Application.Current as Eppie.App.Shared.App).NavigationService;
        }
    }
}

#endif
