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
    public sealed partial class NewAccountSettingsControl : UserControl
    {
        public AccountSettingsModel AccountSettingsModel
        {
            get { return (AccountSettingsModel)GetValue(AccountSettingsModelProperty); }
            set { SetValue(AccountSettingsModelProperty, value); }
        }
        public static readonly DependencyProperty AccountSettingsModelProperty =
            DependencyProperty.Register(nameof(AccountSettingsModel), typeof(AccountSettingsModel), typeof(NewAccountSettingsControl), new PropertyMetadata(null, OnModelChanged));

        private static void OnModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is NewAccountSettingsControl control)
            {
                control.BasicAccountSettingsModel = control.AccountSettingsModel as BasicAccountSettingsModel;
            }
        }

        public BasicAccountSettingsModel BasicAccountSettingsModel
        {
            get { return (BasicAccountSettingsModel)GetValue(BasicAccountSettingsModelProperty); }
            private set { SetValue(BasicAccountSettingsModelProperty, value); }
        }
        public static readonly DependencyProperty BasicAccountSettingsModelProperty =
            DependencyProperty.Register(nameof(BasicAccountSettingsModel), typeof(BasicAccountSettingsModel), typeof(NewAccountSettingsControl), new PropertyMetadata(null));

        public bool IsEmailReadOnly
        {
            get { return (bool)GetValue(IsEmailReadOnlyProperty); }
            set { SetValue(IsEmailReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsEmailReadOnlyProperty =
            DependencyProperty.Register(nameof(IsEmailReadOnly), typeof(bool), typeof(NewAccountSettingsControl), new PropertyMetadata(false));

        public Array IncomingProtocolTypes
        {
            get { return (Array)GetValue(IncomingProtocolTypesProperty); }
            set { SetValue(IncomingProtocolTypesProperty, value); }
        }
        public static readonly DependencyProperty IncomingProtocolTypesProperty =
            DependencyProperty.Register(nameof(IncomingProtocolTypes), typeof(Array), typeof(NewAccountSettingsControl), new PropertyMetadata(null));

        public IClipboardProvider ClipboardProvider
        {
            get { return (IClipboardProvider)GetValue(ClipboardProviderProperty); }
            set { SetValue(ClipboardProviderProperty, value); }
        }
        public static readonly DependencyProperty ClipboardProviderProperty =
            DependencyProperty.Register(nameof(ClipboardProvider), typeof(IClipboardProvider), typeof(NewAccountSettingsControl), new PropertyMetadata(null));

        public IFileOperationProvider FileProvider
        {
            get { return (IFileOperationProvider)GetValue(FileProviderProperty); }
            set { SetValue(FileProviderProperty, value); }
        }
        public static readonly DependencyProperty FileProviderProperty =
            DependencyProperty.Register(nameof(FileProvider), typeof(IFileOperationProvider), typeof(NewAccountSettingsControl), new PropertyMetadata(null));

        public int SelectedSettingsModeIndex
        {
            get { return (int)GetValue(SelectedSettingsModeIndexProperty); }
            set { SetValue(SelectedSettingsModeIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedSettingsModeIndexProperty =
            DependencyProperty.Register(nameof(SelectedSettingsModeIndex), typeof(int), typeof(NewAccountSettingsControl), new PropertyMetadata(0));

        public bool ShouldAutoExpandOutgoingServer
        {
            get { return (bool)GetValue(ShouldAutoExpandOutgoingServerProperty); }
            set { SetValue(ShouldAutoExpandOutgoingServerProperty, value); }
        }
        public static readonly DependencyProperty ShouldAutoExpandOutgoingServerProperty =
            DependencyProperty.Register(nameof(ShouldAutoExpandOutgoingServer), typeof(bool), typeof(NewAccountSettingsControl), new PropertyMetadata(false));

        public bool ShouldAutoExpandIncomingServer
        {
            get { return (bool)GetValue(ShouldAutoExpandIncomingServerProperty); }
            set { SetValue(ShouldAutoExpandIncomingServerProperty, value); }
        }
        public static readonly DependencyProperty ShouldAutoExpandIncomingServerProperty =
            DependencyProperty.Register(nameof(ShouldAutoExpandIncomingServer), typeof(bool), typeof(NewAccountSettingsControl), new PropertyMetadata(false));

        public ICommand CreateHybridAddressCommand
        {
            get { return (ICommand)GetValue(CreateHybridAddressProperty); }
            set { SetValue(CreateHybridAddressProperty, value); }
        }
        public static readonly DependencyProperty CreateHybridAddressProperty =
            DependencyProperty.Register(nameof(CreateHybridAddressCommand), typeof(ICommand), typeof(NewAccountSettingsControl), new PropertyMetadata(null));

        public bool ShowHybridAddressButton
        {
            get { return (bool)GetValue(ShowHybridAddressButtonProperty); }
            set { SetValue(ShowHybridAddressButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowHybridAddressButtonProperty =
            DependencyProperty.Register(nameof(ShowHybridAddressButton), typeof(bool), typeof(NewAccountSettingsControl), new PropertyMetadata(false));


        public NewAccountSettingsControl()
        {
            this.InitializeComponent();
        }
    }
}
