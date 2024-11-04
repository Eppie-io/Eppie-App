#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    public sealed partial class MessageControl : UserControl
    {
        public bool HasHtmlBody
        {
            get { return (bool)GetValue(HasHtmlBodyProperty); }
            set { SetValue(HasHtmlBodyProperty, value); }
        }

        public static readonly DependencyProperty HasHtmlBodyProperty =
            DependencyProperty.Register(nameof(HasHtmlBody), typeof(bool), typeof(MessageControl), new PropertyMetadata(false));

        public string TextBody
        {
            get { return (string)GetValue(TextBodyProperty); }
            set { SetValue(TextBodyProperty, value); }
        }

        public static readonly DependencyProperty TextBodyProperty =
            DependencyProperty.Register(nameof(TextBody), typeof(string), typeof(MessageControl), new PropertyMetadata(null));

        public string HtmlBody
        {
            get { return (string)GetValue(HtmlBodyProperty); }
            set { SetValue(HtmlBodyProperty, value); }
        }

        public static readonly DependencyProperty HtmlBodyProperty =
            DependencyProperty.Register(nameof(HtmlBody), typeof(string), typeof(MessageControl), new PropertyMetadata(null));

        public MessageControl()
        {
            this.InitializeComponent();
        }
    }
}
