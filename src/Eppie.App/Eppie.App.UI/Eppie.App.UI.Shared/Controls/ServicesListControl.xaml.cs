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

using System.Collections.ObjectModel;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class ServicesListControl : UserControl
    {
        public ObservableCollection<ServiceInfo> Items
        {
            get { return (ObservableCollection<ServiceInfo>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<ServiceInfo>), typeof(ServicesListControl), new PropertyMetadata(null));

        public ServiceInfo SelectedItem
        {
            get { return (ServiceInfo)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(ServiceInfo), typeof(ServicesListControl), new PropertyMetadata(null));

        public ServicesListControl()
        {
            this.InitializeComponent();
        }
    }
}
