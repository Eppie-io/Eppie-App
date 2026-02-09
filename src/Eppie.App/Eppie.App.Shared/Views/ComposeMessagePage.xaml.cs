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
using Eppie.App.Services;
using Tuvi.App.ViewModels;
using Windows.ApplicationModel.DataTransfer;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
#endif

namespace Eppie.App.Views
{

    internal partial class ComposeMessagePageBase : BasePage<NewMessagePageViewModel, BaseViewModel>
    {
    }

    internal sealed partial class ComposeMessagePage : ComposeMessagePageBase
    {
        public ComposeMessagePage()
        {
            this.InitializeComponent();
        }

#if WINDOWS_UWP
        private async void OnDrop(object sender, DragEventArgs e)
#else
        private async void OnDrop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
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
        private void OnDragOver(object sender, DragEventArgs e)
#else
        private void OnDragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
#endif
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }
    }
}
