#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Shared.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
