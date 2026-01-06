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

using Eppie.App.UI.Extensions;
using System.Diagnostics.CodeAnalysis;


#if WINDOWS_UWP
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public partial class RichTextEditor : RichEditBox
    {
        private string _html;
        private bool _htmlNeedsUpdate;

        public string Html
        {
            get
            {
                if (_htmlNeedsUpdate)
                {
                    _html = Document.ToHtml();
                    _htmlNeedsUpdate = false;
                }
                return _html;
            }
            set
            {
                SetValue(HtmlProperty, value);
                _html = value;
                _htmlNeedsUpdate = false;
            }
        }
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register(nameof(Html), typeof(string), typeof(RichTextEditor), new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(RichTextEditor), new PropertyMetadata(string.Empty, OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextEditor editor)
            {
                editor.UpdateDocumentAsText(editor.Text);
            }
        }

#if WINDOWS_UWP || WINDOWS10_0_19041_0_OR_GREATER

        private bool _skipUpdating;

        public RichTextEditor()
        {
            TextChanged += OnContentTextChanged;
        }

        private void OnContentTextChanged(object sender, RoutedEventArgs e)
        {
            if (Document is null)
            {
                return;
            }
            // To skip internal updation of document on setting Text property.
            _skipUpdating = true;
            try
            {
                Document.GetText(TextGetOptions.UseCrlf, out string text);

                if (string.IsNullOrWhiteSpace(text))
                {
                    Text = string.Empty;
                    Html = string.Empty;
                }
                else
                {
                    Text = text;
                    _htmlNeedsUpdate = true;
                }
            }
            finally
            {
                _skipUpdating = false;
            }
        }

        private void UpdateDocumentAsText(string text)
        {
            if (!_skipUpdating && !string.IsNullOrEmpty(text))
            {
                Document?.SetText(TextSetOptions.None, text);
                _htmlNeedsUpdate = true;
            }
        }
#else
        private void UpdateDocumentAsText(string text)
        {
            throw new System.NotImplementedException();
        }
#endif
    }
}
