using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Tuvi.App.ViewModels
{
    public class WelcomePageViewModel : BaseViewModel
    {
        public ICommand CreateAccountCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(SeedGeneratePageViewModel)));

        public ICommand RestoreAccountCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(SeedRestorePageViewModel)));
    }
}
