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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class EditMessageControl : AIAgentUserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditMessageControl), new PropertyMetadata(null));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register(nameof(Html), typeof(string), typeof(EditMessageControl), new PropertyMetadata(null));


        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(EditMessageControl), new PropertyMetadata(null));


        public EditMessageControl()
        {
            this.InitializeComponent();
        }

        override protected void ShowAIAgentProcessedText()
        {
            SecondColumn.Width = new GridLength(1, GridUnitType.Star);
        }
    }
}
