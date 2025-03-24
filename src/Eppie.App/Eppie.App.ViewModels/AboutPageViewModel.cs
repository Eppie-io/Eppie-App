namespace Tuvi.App.ViewModels
{
    public class AboutPageViewModel : BaseViewModel
    {
        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion ?? (_appVersion = BrandService.GetAppVersion()); }
        }

        public string PublisherDisplayName
        {
            get { return BrandService.GetPublisherDisplayName(); }
        }

        public string ApplicationName
        {
            get { return BrandService.GetName(); }
        }

        public string SupportEmail
        {
            get { return BrandService.GetSupport(); }
        }

        public string SupportEmailLink
        {
            get { return $"mailto:{SupportEmail}"; }
        }
    }
}
