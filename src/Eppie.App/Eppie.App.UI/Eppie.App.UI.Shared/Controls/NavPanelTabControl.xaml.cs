// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
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
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    public class ControlModelTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ContactsModelTemplate { get; set; }
        public DataTemplate MailBoxesModelTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case ContactsModel _: return ContactsModelTemplate;
                case MailBoxesModel _: return MailBoxesModelTemplate;
                default: return base.SelectTemplateCore(item);
            }
        }
    }

    public sealed partial class NavPanelTabControl : UserControl
    {
        public NavPanelTabModel TabModel
        {
            get { return (NavPanelTabModel)GetValue(TabModelProperty); }
            set { SetValue(TabModelProperty, value); }
        }
        public static readonly DependencyProperty TabModelProperty =
            DependencyProperty.Register(nameof(TabModel), typeof(NavPanelTabModel), typeof(NavPanelTabControl), new PropertyMetadata(null));


        public NavPanelTabControl()
        {
            this.InitializeComponent();
        }
    }
}
