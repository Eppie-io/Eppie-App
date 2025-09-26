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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Markup;
#endif

// ToDo: Change namespace
namespace Tuvi.App.Shared.Controls
{
    [ContentProperty(Name = nameof(OptionContent))]
    public sealed partial class OptionControl : UserControl
    {
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(nameof(Caption), typeof(string), typeof(OptionControl), new PropertyMetadata(null));


        public Inline InlineNote
        {
            get { return (Inline)GetValue(InlineNoteProperty); }
            set { SetValue(InlineNoteProperty, value); }
        }

        public static readonly DependencyProperty InlineNoteProperty =
            DependencyProperty.Register(nameof(InlineNote), typeof(Inline), typeof(OptionControl), new PropertyMetadata(null, OnInlineNotePropertyChanged));

        private static void OnInlineNotePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OptionControl control && e.NewValue is Inline inline)
            {
                control.NoteTextBlock.Inlines.Clear();
                control.NoteTextBlock.Inlines.Add(inline);
            }
        }


        public string TextNote
        {
            get { return (string)GetValue(TextNoteProperty); }
            set { SetValue(TextNoteProperty, value); }
        }

        public static readonly DependencyProperty TextNoteProperty =
            DependencyProperty.Register(nameof(TextNote), typeof(string), typeof(OptionControl), new PropertyMetadata(null, OnTextNotePropertyChanged));

        private static void OnTextNotePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OptionControl control && e.NewValue is string text && control.InlineNote is null)
            {
                control.NoteTextBlock.Text = text;
            }
        }


        public double GapHeight
        {
            get { return (double)GetValue(GapHeightProperty); }
            set { SetValue(GapHeightProperty, value); }
        }

        public static readonly DependencyProperty GapHeightProperty =
            DependencyProperty.Register(nameof(GapHeight), typeof(double), typeof(OptionControl), new PropertyMetadata(36.0));


        public UIElement OptionContent
        {
            get { return (UIElement)GetValue(OptionContentProperty); }
            set { SetValue(OptionContentProperty, value); }
        }

        public static readonly DependencyProperty OptionContentProperty =
            DependencyProperty.Register(nameof(OptionContent), typeof(UIElement), typeof(OptionControl), new PropertyMetadata(null));


        public bool HideNote
        {
            get { return (bool)GetValue(HideNoteProperty); }
            set { SetValue(HideNoteProperty, value); }
        }

        public static readonly DependencyProperty HideNoteProperty =
            DependencyProperty.Register(nameof(HideNote), typeof(bool), typeof(OptionControl), new PropertyMetadata(false));


        public OptionControl()
        {
            this.InitializeComponent();
        }
    }
}
