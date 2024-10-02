using Windows.ApplicationModel.Resources;

namespace Tuvi.App.Shared.Services
{
    public class LocalizationService : Tuvi.App.ViewModels.Services.ILocalizationService
    {
        public string GetString(string resource)
        {
            return ResourceLoader.GetForCurrentView().GetString(resource);
        }
    }
}
