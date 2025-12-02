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
using Tuvi.App.Shared.Controls;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Windows.System;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.UI.Controls
{
    public sealed partial class MessageControl : AIAgentUserControl
    {
        private bool _webResourceBlockingRegistered = false;

        public bool ExternalContentBlocked
        {
            get => (bool)GetValue(ExternalContentBlockedProperty);
            set => SetValue(ExternalContentBlockedProperty, value);
        }

        public static readonly DependencyProperty ExternalContentBlockedProperty =
            DependencyProperty.Register(nameof(ExternalContentBlocked), typeof(bool), typeof(MessageControl), new PropertyMetadata(false));

        public ExternalContentPolicy ExternalContentPolicy
        {
            get => (ExternalContentPolicy)GetValue(ExternalContentPolicyProperty);
            set => SetValue(ExternalContentPolicyProperty, value);
        }

        public static readonly DependencyProperty ExternalContentPolicyProperty =
            DependencyProperty.Register(nameof(ExternalContentPolicy), typeof(ExternalContentPolicy), typeof(MessageControl), new PropertyMetadata(ExternalContentPolicy.AlwaysAllow));

        private bool _allowExternalOnce = false;

        public RelayCommand AllowExternalCommand { get; }

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

            AllowExternalCommand = new RelayCommand(() =>
            {
                _allowExternalOnce = true;
                OnUpdate();
            });
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await HtmlView.EnsureCoreWebView2Async();
                HtmlView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                HtmlView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                HtmlView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

                if (!_webResourceBlockingRegistered)
                {
                    HtmlView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All); // ToDo: Uno0001
                    HtmlView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested; // ToDo: Uno0001
                    _webResourceBlockingRegistered = true;
                }

                OnUpdate();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (HtmlView.CoreWebView2 != null)
                {
                    if (_webResourceBlockingRegistered)
                    {
                        HtmlView.CoreWebView2.WebResourceRequested -= CoreWebView2_WebResourceRequested; // ToDo: Uno0001
                        HtmlView.CoreWebView2.RemoveWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All); // ToDo: Uno0001
                        _webResourceBlockingRegistered = false;
                    }

                    HtmlView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
                    HtmlView.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
                    HtmlView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
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

        private void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            _allowExternalOnce = false;
        }

        // WebResourceRequested handler: block external network requests based on policy
        private void CoreWebView2_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
        {
            try
            {
                var uri = args.Request?.Uri ?? string.Empty;

                // Check policy
                var policy = ExternalContentPolicy;

                // AlwaysAllow policy - allow everything
                if (policy == ExternalContentPolicy.AlwaysAllow)
                {
                    return;
                }

                // Allow one-time override (for AskEachTime policy)
                if (_allowExternalOnce)
                {
                    return; // allow during this navigation
                }

                // allow schemes
                if (uri.StartsWith("blob:", StringComparison.OrdinalIgnoreCase) ||
                    uri.StartsWith("cid:", StringComparison.OrdinalIgnoreCase) ||
                    uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                    uri.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
                {
                    return; // allow
                }

                // block everything else to prevent external network access
                var env = sender.Environment; // ToDo: Uno0001
                args.Response = env.CreateWebResourceResponse(null, 204, "No Content", "Content-Type: text/plain"); // ToDo: Uno0001

                // Only show blocked content banner if policy is AskEachTime
                if (policy == ExternalContentPolicy.AskEachTime)
                {
                    SetExternalContentBlocked(true);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
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
            SetExternalContentBlocked(false);

            await HtmlView.EnsureCoreWebView2Async();
            HtmlView.CoreWebView2.Settings.IsScriptEnabled = false; // ToDo: Uno0001
            HtmlView.NavigateToString(HtmlBody);
        }

        private void SetExternalContentBlocked(bool value)
        {
            if (ExternalContentBlocked != value)
            {
#if WINDOWS_UWP
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => ExternalContentBlocked = value);
#else
                var dq = Windows.System.DispatcherQueue.GetForCurrentThread();
                if (dq != null)
                {
                    dq.TryEnqueue(() => ExternalContentBlocked = value);
                }
                else
                {
                    ExternalContentBlocked = value;
                }
#endif
            }
        }

        override protected void ShowAIAgentProcessedText()
        {
            SecondColumn.Width = new GridLength(1, GridUnitType.Star);
        }
    }
}
