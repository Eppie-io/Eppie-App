namespace Tuvi.App.Shared.Services
{
    public class LocalizationService : Tuvi.App.ViewModels.Services.ILocalizationService
    {
        public string GetString(string resource)
        {
            return Eppie.App.Resources.StringProvider.GetInstance().GetString(resource);
        }
    }
}
