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
using System.Windows.Input;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
#endif

namespace Eppie.App.UI.Controls
{
    public sealed partial class SupportDevelopmentControl : UserControl
    {
        public ICommand SupportDevelopmentCommand
        {
            get { return (ICommand)GetValue(SupportDevelopmentCommandProperty); }
            set { SetValue(SupportDevelopmentCommandProperty, value); }
        }
        public static readonly DependencyProperty SupportDevelopmentCommandProperty =
            DependencyProperty.Register(nameof(SupportDevelopmentCommand), typeof(ICommand), typeof(SupportDevelopmentControl), new PropertyMetadata(null));

        public bool IsStorePaymentProcessor
        {
            get { return (bool)GetValue(IsStorePaymentProcessorProperty); }
            set { SetValue(IsStorePaymentProcessorProperty, value); }
        }
        public static readonly DependencyProperty IsStorePaymentProcessorProperty =
            DependencyProperty.Register(nameof(IsStorePaymentProcessor), typeof(bool), typeof(SupportDevelopmentControl), new PropertyMetadata(null));

        public string Price
        {
            get { return (string)GetValue(PriceProperty); }
            set { SetValue(PriceProperty, value); }
        }
        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register(nameof(Price), typeof(string), typeof(SupportDevelopmentControl), new PropertyMetadata(null));

        public bool IsIconVisible
        {
            get { return (bool)GetValue(IsIconVisibleProperty); }
            set { SetValue(IsIconVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsIconVisibleProperty =
            DependencyProperty.Register(nameof(IsIconVisible), typeof(bool), typeof(SupportDevelopmentControl), new PropertyMetadata(null));

        public bool IsCloseButtonVisible
        {
            get { return (bool)GetValue(IsCloseButtonVisibleProperty); }
            set { SetValue(IsCloseButtonVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsCloseButtonVisibleProperty =
            DependencyProperty.Register(nameof(IsCloseButtonVisible), typeof(bool), typeof(SupportDevelopmentControl), new PropertyMetadata(false));

        public SupportDevelopmentControl()
        {
            this.InitializeComponent();
        }

        public event EventHandler CloseRequested;

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
