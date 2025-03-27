using System;
using Tuvi.App.Shared.Controls;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Windows.System;




#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.UI.Controls
{
    public sealed partial class MessageControl : AIAgentUserControl
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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await HtmlView.EnsureCoreWebView2Async();
                HtmlView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                HtmlView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

                OnUpdate();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (HtmlView.CoreWebView2 != null)
            {
                HtmlView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
                HtmlView.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
            }
        }

        private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true;

            if (args.IsUserInitiated)
            {
                LaunchDefaultBrowser(args.Uri);
            }
        }

        private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (!args.Uri.StartsWith("data:"))
            {
                args.Cancel = true;
            }

            if (args.IsUserInitiated)
            {
                LaunchDefaultBrowser(args.Uri);
            }
        }

        private async void LaunchDefaultBrowser(string url)
        {
            try
            {
                if (url != null)
                {
                    await Launcher.LaunchUriAsync(new Uri(url));
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
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

        override protected void ShowAIAgentProcessedText()
        {
            SecondColumn.Width = new GridLength(1, GridUnitType.Star);
        }
    }
}
