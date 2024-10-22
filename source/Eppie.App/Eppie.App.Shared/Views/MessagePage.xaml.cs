using System;
using Tuvi.App.Services;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    }
}
