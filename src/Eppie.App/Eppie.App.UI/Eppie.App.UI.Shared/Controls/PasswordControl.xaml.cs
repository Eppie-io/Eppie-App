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

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class PasswordControl : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(PasswordControl), new PropertyMetadata(""));

        public bool IsTextVisible
        {
            get { return (bool)GetValue(IsTextVisibleProperty); }
            set { SetValue(IsTextVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsTextVisibleProperty =
            DependencyProperty.Register(nameof(IsTextVisible), typeof(bool), typeof(PasswordControl), new PropertyMetadata(false));

        public PasswordControlModel PasswordModel
        {
            get { return (PasswordControlModel)GetValue(PasswordModelProperty); }
            set { SetValue(PasswordModelProperty, value); }
        }
        public static readonly DependencyProperty PasswordModelProperty =
            DependencyProperty.Register(nameof(PasswordModel), typeof(PasswordControlModel), typeof(PasswordControl), new PropertyMetadata(null, OnPasswordModelPropertyChanged));
        private static void OnPasswordModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordControl control && e.NewValue is PasswordControlModel passwordModel)
            {
                control.IsPasswordVisible = (passwordModel.PasswordAction & PasswordActions.EnterPassword) == PasswordActions.EnterPassword;
                control.IsConfirmPasswordVisible = (passwordModel.PasswordAction & PasswordActions.Confirm) == PasswordActions.Confirm;
                control.IsCurrentPasswordVisible = (passwordModel.PasswordAction & PasswordActions.CheckCurrent) == PasswordActions.CheckCurrent;
            }
        }

        public bool IsCurrentPasswordVisible
        {
            get { return (bool)GetValue(IsCurrentPasswordVisibleProperty); }
            set { SetValue(IsCurrentPasswordVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsCurrentPasswordVisibleProperty =
            DependencyProperty.Register(nameof(IsCurrentPasswordVisible), typeof(bool), typeof(PasswordControl), new PropertyMetadata(false));

        public bool IsPasswordVisible
        {
            get { return (bool)GetValue(IsPasswordVisibleProperty); }
            set { SetValue(IsPasswordVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsPasswordVisibleProperty =
            DependencyProperty.Register(nameof(IsPasswordVisible), typeof(bool), typeof(PasswordControl), new PropertyMetadata(true));

        public bool IsConfirmPasswordVisible
        {
            get { return (bool)GetValue(IsConfirmPasswordVisibleProperty); }
            set { SetValue(IsConfirmPasswordVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsConfirmPasswordVisibleProperty =
            DependencyProperty.Register(nameof(IsConfirmPasswordVisible), typeof(bool), typeof(PasswordControl), new PropertyMetadata(false));

        public PasswordControl()
        {
            this.InitializeComponent();
        }
    }


}
