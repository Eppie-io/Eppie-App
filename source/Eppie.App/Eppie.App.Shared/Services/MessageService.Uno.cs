#if !WINDOWS_UWP

using Tuvi.App.ViewModels.Services;
using Microsoft.UI.Xaml;

namespace Tuvi.App.Shared.Services
{
    public partial class MessageService : ITuviMailMessageService
    {
        private INavigationService GetNavigationService()
        {
            return (Application.Current as Eppie.App.App).NavigationService;
        }
    }
}

#endif
