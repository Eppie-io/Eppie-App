namespace Tuvi.App.Shared.Services
{
    public class LocalizationService : Tuvi.App.ViewModels.Services.ILocalizationService
    {
        public string GetString(string resource)
        {
            return Eppie.App.UI.Resources.StringProvider.GetInstance().GetString(resource);
        }
    }
}
