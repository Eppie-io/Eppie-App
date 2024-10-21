#if WINDOWS_UWP
using Tuvi.App.Shared.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Controls
{
    public class RichTextEditor : RichEditBox
    {
        private bool _skipUpdating;

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register(nameof(Html), typeof(string), typeof(RichTextEditor), new PropertyMetadata(""));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(RichTextEditor), new PropertyMetadata("", OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextEditor editor)
            {
                editor.UpdateDocumentAsText((string)e.NewValue);
            }
        }


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
                Document.GetText(Windows.UI.Text.TextGetOptions.UseCrlf, out string text);

                if (!string.IsNullOrWhiteSpace(text))
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
                Document?.SetText(Windows.UI.Text.TextSetOptions.None, text);
            }
        }
    }
}
#endif
