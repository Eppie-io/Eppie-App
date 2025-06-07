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
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    public sealed partial class WhatsNewControl : UserControl
    {
        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register(nameof(Version), typeof(string), typeof(WhatsNewControl), new PropertyMetadata(null));

        public ICommand SupportDevelopmentCommand
        {
            get { return (ICommand)GetValue(SupportDevelopmentCommandProperty); }
            set { SetValue(SupportDevelopmentCommandProperty, value); }
        }
        public static readonly DependencyProperty SupportDevelopmentCommandProperty =
            DependencyProperty.Register(nameof(SupportDevelopmentCommand), typeof(ICommand), typeof(WhatsNewControl), new PropertyMetadata(null));

        public bool IsStorePaymentProcessor
        {
            get { return (bool)GetValue(IsStorePaymentProcessorProperty); }
            set { SetValue(IsStorePaymentProcessorProperty, value); }
        }
        public static readonly DependencyProperty IsStorePaymentProcessorProperty =
            DependencyProperty.Register(nameof(IsStorePaymentProcessor), typeof(bool), typeof(WhatsNewControl), new PropertyMetadata(null));

        public string Price
        {
            get { return (string)GetValue(PriceProperty); }
            set { SetValue(PriceProperty, value); }
        }
        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register(nameof(Price), typeof(string), typeof(WhatsNewControl), new PropertyMetadata(null));


        public string TwitterUrl
        {
            get { return (string)GetValue(TwitterUrlProperty); }
            set { SetValue(TwitterUrlProperty, value); }
        }
        public static readonly DependencyProperty TwitterUrlProperty =
            DependencyProperty.Register(nameof(TwitterUrl), typeof(string), typeof(WhatsNewControl), new PropertyMetadata(null));


        public bool IsSupportDevelopmentButtonVisible
        {
            get { return (bool)GetValue(IsSupportDevelopmentButtonVisibleProperty); }
            set { SetValue(IsSupportDevelopmentButtonVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsSupportDevelopmentButtonVisibleProperty =
            DependencyProperty.Register(nameof(IsSupportDevelopmentButtonVisible), typeof(bool), typeof(WhatsNewControl), new PropertyMetadata(null));


        public event EventHandler CloseRequested;

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public WhatsNewControl()
        {
            this.InitializeComponent();
        }
    }
}
