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

using System;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class ProtonSettingsControl : UserControl
    {
        public ProtonAddressSettingsModel ProtonAddressSettingsModel
        {
            get { return (ProtonAddressSettingsModel)GetValue(ProtonAddressSettingsModelProperty); }
            set { SetValue(ProtonAddressSettingsModelProperty, value); }
        }
        public static readonly DependencyProperty ProtonAddressSettingsModelProperty =
            DependencyProperty.Register(nameof(ProtonAddressSettingsModel), typeof(ProtonAddressSettingsModel), typeof(ProtonSettingsControl), new PropertyMetadata(null));

        public bool IsEmailReadOnly
        {
            get { return (bool)GetValue(IsEmailReadOnlyProperty); }
            set { SetValue(IsEmailReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsEmailReadOnlyProperty =
            DependencyProperty.Register(nameof(IsEmailReadOnly), typeof(bool), typeof(ProtonSettingsControl), new PropertyMetadata(false));


        public bool IsAdvancedSettingsModeActive
        {
            get { return (bool)GetValue(IsAdvancedSettingsModeActiveProperty); }
            set { SetValue(IsAdvancedSettingsModeActiveProperty, value); }
        }
        public static readonly DependencyProperty IsAdvancedSettingsModeActiveProperty =
            DependencyProperty.Register(nameof(IsAdvancedSettingsModeActive), typeof(bool), typeof(ProtonSettingsControl), new PropertyMetadata(false));


        public ICommand CreateHybridAddressCommand
        {
            get { return (ICommand)GetValue(CreateHybridAddressProperty); }
            set { SetValue(CreateHybridAddressProperty, value); }
        }
        public static readonly DependencyProperty CreateHybridAddressProperty =
            DependencyProperty.Register(nameof(CreateHybridAddressCommand), typeof(ICommand), typeof(ProtonSettingsControl), new PropertyMetadata(null));


        public bool ShowHybridAddressButton
        {
            get { return (bool)GetValue(ShowHybridAddressButtonProperty); }
            set { SetValue(ShowHybridAddressButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowHybridAddressButtonProperty =
            DependencyProperty.Register(nameof(ShowHybridAddressButton), typeof(bool), typeof(ProtonSettingsControl), new PropertyMetadata(false));


        public ProtonSettingsControl()
        {
            this.InitializeComponent();
        }
    }
}
