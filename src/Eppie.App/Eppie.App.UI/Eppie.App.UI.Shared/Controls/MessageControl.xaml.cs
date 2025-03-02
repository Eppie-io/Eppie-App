using System;
using Tuvi.App.Shared.Controls;
using System.Threading.Tasks;


#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.UI.Controls
{
    public sealed partial class MessageControl : BaseUserControl
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
            DependencyProperty.Register(nameof(HtmlBody), typeof(string), typeof(MessageControl), new PropertyMetadata(null, OnHtmlBodyChanged));

        public string AIAgentProcessedBody
        {
            get { return (string)GetValue(AIAgentProcessedBodyProperty); }
            set
            {
                SetValue(AIAgentProcessedBodyProperty, value);
                SetValue(HasAIAgentProcessedBodyProperty, !string.IsNullOrEmpty(AIAgentProcessedBody));
                if (HasAIAgentProcessedBody)
                {
                    ShowTranslatedText();
                }
            }
        }

        public static readonly DependencyProperty AIAgentProcessedBodyProperty =
            DependencyProperty.Register(nameof(AIAgentProcessedBody), typeof(string), typeof(MessageControl), new PropertyMetadata(null));

        public bool HasAIAgentProcessedBody
        {
            get { return (bool)GetValue(HasAIAgentProcessedBodyProperty); }
            set { SetValue(HasAIAgentProcessedBodyProperty, value); }
        }

        public static readonly DependencyProperty HasAIAgentProcessedBodyProperty =
            DependencyProperty.Register(nameof(HasAIAgentProcessedBody), typeof(string), typeof(MessageControl), new PropertyMetadata(null));


        private static void OnHtmlBodyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is MessageControl control && control.IsLoaded)
            {
                control.OnUpdate();
            }
        }

        public MessageControl()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OnUpdate();
        }

        private async void OnUpdate()
        {
            try
            {
                await UpdateHtmlView();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task UpdateHtmlView()
        {
            await HtmlView.EnsureCoreWebView2Async();

            HtmlView.CoreWebView2.Settings.IsScriptEnabled = false; // ToDo: Uno0001
            HtmlView.NavigateToString(HtmlBody);
        }

        private void ShowTranslatedText()
        {
            SecondColumn.Width = new GridLength(1, GridUnitType.Star);
        }
    }
}
