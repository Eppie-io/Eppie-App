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
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
#endif

namespace Tuvi.App.Shared.Controls
{
    [ContentProperty(Name = nameof(SettingsContent))]
    public sealed partial class AdvancedSettingsControl : UserControl
    {
        private const int DefaultModeIndex = 0;
        private const int AdvancedModeIndex = 1;

        public UIElement SettingsContent
        {
            get { return (UIElement)GetValue(SettingsContentProperty); }
            set { SetValue(SettingsContentProperty, value); }
        }

        public static readonly DependencyProperty SettingsContentProperty =
            DependencyProperty.Register(nameof(SettingsContent), typeof(UIElement), typeof(AdvancedSettingsControl), new PropertyMetadata(null));


        public bool HideResetButton
        {
            get { return (bool)GetValue(HideResetButtonProperty); }
            set { SetValue(HideResetButtonProperty, value); }
        }

        public static readonly DependencyProperty HideResetButtonProperty =
            DependencyProperty.Register(nameof(HideResetButton), typeof(bool), typeof(AdvancedSettingsControl), new PropertyMetadata(false));


        public ICommand ResetCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(AdvancedSettingsControl), new PropertyMetadata(null));


        public bool IsAdvancedModeActive
        {
            get { return (bool)GetValue(IsAdvancedModeActiveProperty); }
            set { SetValue(IsAdvancedModeActiveProperty, value); }
        }

        public static readonly DependencyProperty IsAdvancedModeActiveProperty =
            DependencyProperty.Register(nameof(IsAdvancedModeActive), typeof(bool), typeof(AdvancedSettingsControl), new PropertyMetadata(false, OnSettingsModeChanged));

        private static void OnSettingsModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is AdvancedSettingsControl control && args.NewValue is bool isAdvancedMode)
            {
                control.SettingsModeSelector.SelectedIndex = isAdvancedMode ? AdvancedModeIndex : DefaultModeIndex;
            }
        }

        public AdvancedSettingsControl()
        {
            this.InitializeComponent();
            SettingsModeSelector.SelectedIndex = DefaultModeIndex;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combobox)
            {
                IsAdvancedModeActive = AdvancedModeIndex == combobox.SelectedIndex;
            }
        }
    }
}
