// this is missing
//using Tuvi.App.Helpers;
using System;
using Eppie.App.Shared.Services;
using Tuvi.App.ViewModels;
using Windows.ApplicationModel.DataTransfer;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
#endif

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

        private void DecentralizedChecked(object sender, RoutedEventArgs e)
        {
            ViewModel.IsEncrypted = true;
            ViewModel.IsSigned = true;
        }

        private void onFromChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.OnFromChanged();
        }

#if WINDOWS_UWP
        private async void NewMessagePage_Grid_Drop(object sender, DragEventArgs e)
#else
        private async void NewMessagePage_Grid_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
#endif
        {
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var deferral = e.GetDeferral();

                    var items = await e.DataView.GetStorageItemsAsync();
                    ViewModel.AttachFilesCommand.Execute(new StorageItemsService(items));

                    deferral.Complete();
                }
            }
            catch (Exception ex)
            {
                ViewModel.OnError(ex);
            }
        }

#if WINDOWS_UWP
        private void NewMessagePage_Grid_DragOver(object sender, DragEventArgs e)
#else
        private void NewMessagePage_Grid_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
#endif
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

#if __ANDROID__ || __IOS__
        private bool _isEditingHtml = false;
        private async void OnEmailBodyTapped(object sender, TappedRoutedEventArgs e)
        {

            // ToDo:

            //if (_isEditingHtml) return;

            //try
            //{
            //    _isEditingHtml = true;
            //    ViewModel.HtmlBody = await HtmlHelper.EditHtmlAsync(ViewModel.HtmlBody);
            //}
            //catch (Exception ex)
            //{
            //    ViewModel.OnError(ex);
            //}
            //finally
            //{
            //    _isEditingHtml = false;
            //}
        }
#endif
    }
}
