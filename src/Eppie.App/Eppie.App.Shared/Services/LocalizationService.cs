using Tuvi.App.ViewModels.Services;
using Windows.ApplicationModel.Resources;

namespace Tuvi.App.Shared.Services
{
    public class LocalizationService : ILocalizationService
    {
        public string GetString(string resource)
        {
            return ResourceLoader.GetForCurrentView().GetString(resource);
        }
    }
}
