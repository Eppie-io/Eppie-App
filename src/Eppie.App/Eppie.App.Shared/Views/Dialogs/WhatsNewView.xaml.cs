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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Eppie.App.UI.Controls;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Views.Dialogs
{
    internal struct WhatsNewViewData
    {
        public bool IsStorePaymentProcessor { get; set; }
        public bool IsSupportDevelopmentButtonVisible { get; set; }
        public string Version { get; set; }
        public string Price { get; set; }
        public string TwitterUrl { get; set; }
        public ICommand SupportDevelopmentCommand { get; set; }
    }

    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    internal sealed partial class WhatsNewView : UserControl, AppDialog.IClosable, AppDialog.IInitializer<WhatsNewViewData>
    {
        public event EventHandler CloseRequested;

        public WhatsNewView()
        {
            this.InitializeComponent();
        }

        public void Initialize(WhatsNewViewData data)
        {
            WhatsNewContent.IsStorePaymentProcessor = data.IsStorePaymentProcessor;
            WhatsNewContent.IsSupportDevelopmentButtonVisible = data.IsSupportDevelopmentButtonVisible;
            WhatsNewContent.Version = data.Version;
            WhatsNewContent.Price = data.Price;
            WhatsNewContent.TwitterUrl = data.TwitterUrl;
            WhatsNewContent.SupportDevelopmentCommand = data.SupportDevelopmentCommand;
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
