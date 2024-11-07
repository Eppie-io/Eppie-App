namespace Tuvi.App.ViewModels.Services
{
    public interface INavigationService
    {
        void Navigate(string pageKey, object parameter = null);
        bool CanGoBack();
        void GoBack();
        bool CanGoBackTo(string pageKey);
        void GoBackTo(string pageKey);
        void GoBackOrNavigate(string pageKey, object parameter = null);
        void GoBackToOrNavigate(string pageKey, object parameter = null);
        void ExitApplication();
        void ClearHistory();
    }
}
