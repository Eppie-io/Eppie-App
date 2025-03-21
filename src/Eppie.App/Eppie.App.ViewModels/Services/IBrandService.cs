namespace Tuvi.App.ViewModels.Services
{
    public interface IBrandService
    {
        string GetName();
        string GetSupport();
        string GetHomepage();
        string GetPublisherDisplayName();
        string GetAppVersion();
        string GetVersion();
        string GetPackageVersion();
        string GetFileVersion();
        string GetInformationalVersion();
    }
}
