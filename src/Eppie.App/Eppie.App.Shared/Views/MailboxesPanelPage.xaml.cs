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

using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
#endif

namespace Eppie.App.Views
{
    internal partial class MailboxesPanelPageBase : BasePage<BaseViewModel, BaseViewModel>
    {
    }

    internal sealed partial class MailboxesPanelPage : MailboxesPanelPageBase
    {
        public MailboxesPanelPage()
        {
            this.InitializeComponent();
        }

        public MailBoxesModel MailBoxesModel
        {
            get { return (MailBoxesModel)GetValue(MailBoxesModelProperty); }
            set { SetValue(MailBoxesModelProperty, value); }
        }

        public static readonly DependencyProperty MailBoxesModelProperty =
            DependencyProperty.Register(nameof(MailBoxesModel), typeof(MailBoxesModel), typeof(MailboxesPanelPage), new PropertyMetadata(null));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e?.Parameter is MailBoxesModel model)
            {
                MailBoxesModel = model;
            }
        }
    }
}
