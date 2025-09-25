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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class AddressControl : UserControl
    {
        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register(nameof(Address), typeof(string), typeof(AddressControl), new PropertyMetadata(null));


        public string Warning
        {
            get { return (string)GetValue(WarningProperty); }
            set { SetValue(WarningProperty, value); }
        }

        public static readonly DependencyProperty WarningProperty =
            DependencyProperty.Register(nameof(Warning), typeof(string), typeof(AddressControl), new PropertyMetadata(null));


        public bool HideWarning
        {
            get { return (bool)GetValue(HideWarningProperty); }
            set { SetValue(HideWarningProperty, value); }
        }

        public static readonly DependencyProperty HideWarningProperty =
            DependencyProperty.Register(nameof(HideWarning), typeof(bool), typeof(AddressControl), new PropertyMetadata(true));


        public UIElement ExtraContent
        {
            get { return (UIElement)GetValue(ExtraContentProperty); }
            set { SetValue(ExtraContentProperty, value); }
        }

        public static readonly DependencyProperty ExtraContentProperty =
            DependencyProperty.Register(nameof(ExtraContent), typeof(UIElement), typeof(AddressControl), new PropertyMetadata(null));


        public AddressControl()
        {
            this.InitializeComponent();
        }
    }
}
