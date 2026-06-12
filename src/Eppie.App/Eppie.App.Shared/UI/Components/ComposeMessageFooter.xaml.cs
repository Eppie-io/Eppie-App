// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Input;
using Eppie.App.UI.Resources;
using Tuvi.Core.Entities;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Components
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class ComposeMessageFooter : UserControl
    {
        public object SenderItemsSource
        {
            get { return (object)GetValue(SenderItemsSourceProperty); }
            set { SetValue(SenderItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SenderItemsSourceProperty =
            DependencyProperty.Register(nameof(SenderItemsSource), typeof(object), typeof(ComposeMessageFooter), new PropertyMetadata(null));


        public int SenderSelectedIndex
        {
            get { return (int)GetValue(SenderSelectedIndexProperty); }
            set { SetValue(SenderSelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SenderSelectedIndexProperty =
            DependencyProperty.Register(nameof(SenderSelectedIndex), typeof(int), typeof(ComposeMessageFooter), new PropertyMetadata(-1));


        public ICommand SendMessageCommand
        {
            get { return (ICommand)GetValue(SendMessageCommandProperty); }
            set { SetValue(SendMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty SendMessageCommandProperty =
            DependencyProperty.Register(nameof(SendMessageCommand), typeof(ICommand), typeof(ComposeMessageFooter), new PropertyMetadata(null));


        public ICommand DeleteMessageCommand
        {
            get { return (ICommand)GetValue(DeleteMessageCommandProperty); }
            set { SetValue(DeleteMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteMessageCommandProperty =
            DependencyProperty.Register(nameof(DeleteMessageCommand), typeof(ICommand), typeof(ComposeMessageFooter), new PropertyMetadata(null));


        public ICommand AttachCommand
        {
            get { return (ICommand)GetValue(AttachCommandProperty); }
            set { SetValue(AttachCommandProperty, value); }
        }

        public static readonly DependencyProperty AttachCommandProperty =
            DependencyProperty.Register(nameof(AttachCommand), typeof(ICommand), typeof(ComposeMessageFooter), new PropertyMetadata(null));


        public ComposeMessageFooter()
        {
            this.InitializeComponent();
        }
    }
}
