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
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
#endif

namespace Eppie.App.UI.Controls
{
    public enum ExpanderGlyphPosition
    {
        None,
        Left,
        Right,
    }

    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    [ContentProperty(Name = nameof(ExpanderContent))]
    public sealed partial class SimpleExpander : UserControl
    {
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(SimpleExpander), new PropertyMetadata(null));

        public bool? IsExpanded
        {
            get { return (bool?)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool?), typeof(SimpleExpander), new PropertyMetadata(false));


        public UIElement ExpanderContent
        {
            get { return (UIElement)GetValue(ExpanderContentProperty); }
            set { SetValue(ExpanderContentProperty, value); }
        }

        public static readonly DependencyProperty ExpanderContentProperty =
            DependencyProperty.Register(nameof(ExpanderContent), typeof(UIElement), typeof(SimpleExpander), new PropertyMetadata(null));


        public string Glyph
        {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }

        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(SimpleExpander), new PropertyMetadata(null));


        public ExpanderGlyphPosition GlyphPosition
        {
            get { return (ExpanderGlyphPosition)GetValue(GlyphPositionProperty); }
            set { SetValue(GlyphPositionProperty, value); }
        }

        public static readonly DependencyProperty GlyphPositionProperty =
            DependencyProperty.Register(nameof(GlyphPosition), typeof(ExpanderGlyphPosition), typeof(SimpleExpander), new PropertyMetadata(ExpanderGlyphPosition.Left));


        public double GlyphSize
        {
            get { return (double)GetValue(GlyphSizeProperty); }
            set { SetValue(GlyphSizeProperty, value); }
        }

        public static readonly DependencyProperty GlyphSizeProperty =
            DependencyProperty.Register(nameof(GlyphSize), typeof(double), typeof(SimpleExpander), new PropertyMetadata(8d));


        public SimpleExpander()
        {
            this.InitializeComponent();
        }

        public static Visibility GetGlyphVisibility(ExpanderGlyphPosition position, ExpanderGlyphPosition fontIconPosition)
        {
            return position != ExpanderGlyphPosition.None && position == fontIconPosition ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public partial class StringContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is string)
            {
                return StringTemplate;
            }

            // For other types, default rendering is used.
            return null;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
