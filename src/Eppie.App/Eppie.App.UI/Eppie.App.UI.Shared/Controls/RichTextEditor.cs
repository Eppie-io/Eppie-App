using System;
using Tuvi.App.Shared.Extensions;

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
    public partial class RichTextEditor : RichEditBox
    {
        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
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
                    Html = Document.ToHtml();
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
