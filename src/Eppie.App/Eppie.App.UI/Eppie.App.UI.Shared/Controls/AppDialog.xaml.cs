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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
#endif

namespace Eppie.App.UI.Controls
{
    public interface IAppDialogView
    {
        event EventHandler CloseRequested;
    }

    public interface IAppDialogInitializer<TData>
    {
        void Initialize(TData data);
    }

    public interface IAppDialogLifecycle
    {
        void OnOpened();
        void OnClosing(ContentDialogResult result, out bool cancel);
        void OnClosed(ContentDialogResult result);
    }


    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class AppDialog : ContentDialog
    {
        private IAppDialogView _appDialogView;

        public AppDialog()
        {
            this.InitializeComponent();
        }

        public async Task ShowAsync<TView>()
            where TView : UIElement, IAppDialogView, new()
        {
            TView view = new TView();
            await ShowAsync(view);
        }

        public async Task ShowAsync<TView, TData>(TData data)
            where TView : UIElement, IAppDialogView, IAppDialogInitializer<TData>, new()
        {
            TView view = new TView();
            view.Initialize(data);

            await ShowAsync(view);
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private async Task ShowAsync<TView>(TView view)
            where TView : UIElement, IAppDialogView
        {
            _appDialogView = view;
            view.CloseRequested += OnCloseRequested;

            Opened += OnOpened;
            Closing += OnClosing;
            Closed += OnClosed;

            ContentElement.Child = view;

            long backgroundToken = view.RegisterPropertyChangedCallback(BackgroundProperty, OnBackgroundPropertyChanged);
            long closeButtonVisibilityToken = view.RegisterPropertyChangedCallback(AppDialogExtensions.CloseButtonVisibilityProperty, OnCloseButtonVisibilityPropertyChanged);
            long titleToken = view.RegisterPropertyChangedCallback(AppDialogExtensions.TitleContentProperty, OnTitlePropertyChanged);

            UpdateBackground();
            UpdateCloseButton();
            UpdateTitleContent();

            await ShowAsync();

            view.UnregisterPropertyChangedCallback(AppDialogExtensions.TitleContentProperty, titleToken);
            view.UnregisterPropertyChangedCallback(AppDialogExtensions.CloseButtonVisibilityProperty, closeButtonVisibilityToken);
            view.UnregisterPropertyChangedCallback(BackgroundProperty, backgroundToken);

            view.CloseRequested -= OnCloseRequested;

            Opened -= OnOpened;
            Closing -= OnClosing;
            Closed -= OnClosed;
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            Hide();
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (_appDialogView is IAppDialogLifecycle lifecycle)
            {
                lifecycle.OnClosing(args.Result, out bool cancel);
                args.Cancel = cancel;
            }
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (_appDialogView is IAppDialogLifecycle lifecycle)
            {
                lifecycle.OnClosed(args.Result);
            }
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (_appDialogView is IAppDialogLifecycle lifecycle)
            {
                lifecycle.OnOpened();
            }
        }

        private void OnBackgroundPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateBackground();
        }

        private void OnCloseButtonVisibilityPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateCloseButton();
        }

        private void OnTitlePropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateTitleContent();
        }

        private void UpdateTitleContent()
        {
            if (_appDialogView is DependencyObject obj)
            {
                TitlePresenter.Content = AppDialogExtensions.GetTitleContent(obj);
            }
            else
            {
                TitlePresenter.Content = null;
            }

            if (TitlePresenter.Content is null)
            {
                TitlePresenter.Visibility = Visibility.Collapsed;
                Grid.SetRow(ContentElement, 0);
                Grid.SetRowSpan(ContentElement, 2);
            }
            else
            {
                TitlePresenter.Visibility = Visibility.Visible;
                Grid.SetRow(ContentElement, 1);
                Grid.SetRowSpan(ContentElement, 1);
            }
        }

        private void UpdateCloseButton()
        {
            if (_appDialogView is DependencyObject obj)
            {
                CloseButton.Visibility = AppDialogExtensions.GetCloseButtonVisibility(obj);
            }
            else
            {
                CloseButton.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateBackground()
        {
            if (_appDialogView is Control control && control.Background != null)
            {
                Background = control.Background;
            }
        }
    }

    public static class AppDialogExtensions
    {
        private const string TitleContentPropertyName = "TitleContent";
        private const string CloseButtonVisibilityPropertyName = "CloseButtonVisibility";

        public static UIElement GetTitleContent(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(TitleContentProperty);
        }

        public static void SetTitleContent(DependencyObject obj, UIElement value)
        {
            obj.SetValue(TitleContentProperty, value);
        }

        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.RegisterAttached(TitleContentPropertyName, typeof(UIElement), typeof(AppDialogExtensions), new PropertyMetadata(null));


        public static Visibility GetCloseButtonVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(CloseButtonVisibilityProperty);
        }

        public static void SetCloseButtonVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(CloseButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.RegisterAttached(CloseButtonVisibilityPropertyName, typeof(Visibility), typeof(AppDialogExtensions), new PropertyMetadata(Visibility.Visible));
    }
}
