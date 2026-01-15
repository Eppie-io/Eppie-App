// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Eppie.App.Helpers;
using Eppie.App.UI.Tools;
using Tuvi.App.IncrementalLoading;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
#endif

namespace Eppie.App.Views
{
    internal partial class ContactsPanelPageBase : BasePage<ContactsPanelPageViewModel, BaseViewModel>
    {
    }

    internal sealed partial class ContactsPanelPage : ContactsPanelPageBase
    {
        private readonly CancellationTokenSource _contactsCancellationTokenSource;

        public ContactsPanelPage()
        {
            this.InitializeComponent();
            _contactsCancellationTokenSource = new CancellationTokenSource();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModel != null && ViewModel.Contacts == null)
            {
                ViewModel.SetAvatarProvider(PickAvatarBytesAsync);

                var contactsCollection = new IncrementalLoadingCollection<ContactsPanelPageViewModel, ContactItem>(
                    ViewModel,
                    _contactsCancellationTokenSource);

                ViewModel.SetContactsCollection(contactsCollection);
            }

            base.OnNavigatedTo(e);
        }

        private async Task<byte[]> PickAvatarBytesAsync()
        {
            try
            {
                FileOpenPicker fileOpenPicker = FileOpenPickerBuilder.CreateBuilder(Eppie.App.App.MainWindow)
                                                                     .Configure((picker) =>
                                                                     {
                                                                         picker.ViewMode = PickerViewMode.Thumbnail;
                                                                         picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                                                                         picker.FileTypeFilter.Add(".jpg");
                                                                         picker.FileTypeFilter.Add(".jpeg");
                                                                         picker.FileTypeFilter.Add(".png");
                                                                         picker.FileTypeFilter.Add(".bmp");
                                                                     })
                                                                     .Build();

                StorageFile file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    return await BitmapTools.GetThumbnailPixelDataAsync(file, (uint)ContactItem.DefaultAvatarSize, (uint)ContactItem.DefaultAvatarSize).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                ViewModel.OnError(e);
            }
            return null;
        }
    }
}
