using System;
using Tuvi.Core.Entities;
using Tuvi.App.Helpers;
using Tuvi.App.ViewModels;
using Windows.UI.Xaml.Input;

namespace Tuvi.App.Shared.Views
{
    public partial class NewMessagePageBase : BasePage<NewMessagePageViewModel, BaseViewModel>
    { 
    }

    public sealed partial class NewMessagePage : NewMessagePageBase
    {
        public NewMessagePage()
        {
            this.InitializeComponent();
        }

        private void DecentralizedChecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsEncrypted = true;
            ViewModel.IsSigned = true;
        }

        private void onFromChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.IsSigned
                = ViewModel.IsEncrypted
                = ViewModel.IsDecentralized
                = StringHelper.IsDecentralizedEmail(ViewModel.From);
        }

#if __ANDROID__ || __IOS__
        private bool _isEditingHtml = false;
        private async void OnEmailBodyTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_isEditingHtml) return;

            try
            {
                _isEditingHtml = true;
                ViewModel.HtmlBody = await HtmlHelper.EditHtmlAsync(ViewModel.HtmlBody);
            }
            catch (Exception ex)
            {
                ViewModel.OnError(ex);
            }
            finally
            {
                _isEditingHtml = false;
            }
        }
#endif
    }
}
