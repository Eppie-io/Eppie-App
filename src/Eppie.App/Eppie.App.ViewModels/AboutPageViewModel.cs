namespace Tuvi.App.ViewModels
{
    public class AboutPageViewModel : BaseViewModel
    {
        private string _version = "";
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
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
