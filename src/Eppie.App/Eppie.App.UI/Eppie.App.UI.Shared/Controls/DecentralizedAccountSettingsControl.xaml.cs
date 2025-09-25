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

using System.Windows.Input;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    // ToDo: Issue #861 - Remove it
    public sealed partial class DecentralizedAccountSettingsControl : UserControl
    {
        public DecentralizedAddressSettingsModel DecentralizedAddressSettingsModel
        {
            get { return (DecentralizedAddressSettingsModel)GetValue(DecentralizedAddressSettingsModelProperty); }
            set { SetValue(DecentralizedAddressSettingsModelProperty, value); }
        }
        public static readonly DependencyProperty DecentralizedAddressSettingsModelProperty =
            DependencyProperty.Register(nameof(DecentralizedAddressSettingsModel), typeof(DecentralizedAddressSettingsModel), typeof(DecentralizedAccountSettingsControl), new PropertyMetadata(null));

        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(DecentralizedAccountSettingsControl), new PropertyMetadata(false));

        public ICommand ClaimNameCommand
        {
            get { return (ICommand)GetValue(ClaimNameCommandProperty); }
            set { SetValue(ClaimNameCommandProperty, value); }
        }
        public static readonly DependencyProperty ClaimNameCommandProperty =
            DependencyProperty.Register(nameof(ClaimNameCommand), typeof(ICommand), typeof(DecentralizedAccountSettingsControl), new PropertyMetadata(null));

        public bool IsClaimingName
        {
            get { return (bool)GetValue(IsClaimingNameProperty); }
            set { SetValue(IsClaimingNameProperty, value); }
        }
        public static readonly DependencyProperty IsClaimingNameProperty =
            DependencyProperty.Register(nameof(IsClaimingName), typeof(bool), typeof(DecentralizedAccountSettingsControl), new PropertyMetadata(false));

        public DecentralizedAccountSettingsControl()
        {
            this.InitializeComponent();
        }
    }
}
