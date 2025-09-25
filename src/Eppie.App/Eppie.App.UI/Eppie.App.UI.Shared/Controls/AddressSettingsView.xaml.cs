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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class AddressSettingsView : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(AddressSettingsView), new PropertyMetadata(null));


        public UIElement SettingsContent
        {
            get { return (UIElement)GetValue(SettingsContentProperty); }
            set { SetValue(SettingsContentProperty, value); }
        }

        public static readonly DependencyProperty SettingsContentProperty =
            DependencyProperty.Register(nameof(SettingsContent), typeof(UIElement), typeof(AddressSettingsView), new PropertyMetadata(null));


        public ICommand ApplyCommand
        {
            get { return (ICommand)GetValue(ApplyCommandProperty); }
            set { SetValue(ApplyCommandProperty, value); }
        }

        public static readonly DependencyProperty ApplyCommandProperty =
            DependencyProperty.Register(nameof(ApplyCommand), typeof(ICommand), typeof(AddressSettingsView), new PropertyMetadata(null));


        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(AddressSettingsView), new PropertyMetadata(null));


        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(AddressSettingsView), new PropertyMetadata(null));


        public bool IsApplying
        {
            get { return (bool)GetValue(IsApplyingProperty); }
            set { SetValue(IsApplyingProperty, value); }
        }

        public static readonly DependencyProperty IsApplyingProperty =
            DependencyProperty.Register(nameof(IsApplying), typeof(bool), typeof(AddressSettingsView), new PropertyMetadata(false));



        public bool IsApplyButtonEnabled
        {
            get { return (bool)GetValue(IsApplyButtonEnabledProperty); }
            set { SetValue(IsApplyButtonEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsApplyButtonEnabledProperty =
            DependencyProperty.Register(nameof(IsApplyButtonEnabled), typeof(bool), typeof(AddressSettingsView), new PropertyMetadata(true));



        public bool IsRemoveButtonVisible
        {
            get { return (bool)GetValue(IsRemoveButtonVisibleProperty); }
            set { SetValue(IsRemoveButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsRemoveButtonVisibleProperty =
            DependencyProperty.Register(nameof(IsRemoveButtonVisible), typeof(bool), typeof(AddressSettingsView), new PropertyMetadata(false));



        public AddressSettingsView()
        {
            this.InitializeComponent();
        }
    }
}
