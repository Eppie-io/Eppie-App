using System;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Views
{
    public partial class MessagePageBase : BasePage<MessagePageViewModel, BaseViewModel>
    {
    }

    public sealed partial class MessagePage : MessagePageBase
    {
        public MessagePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            InitAIAgentButton(AIAgentButton);
        }
    }
}
