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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
#endif

namespace Eppie.App.UI.Controls
{
    public interface IPopupPage
    {
        event EventHandler ClosePopupRequested;
        void OnCloseClicked();
    }

    public sealed partial class PopupHostControl : UserControl
    {
        public Type PageType
        {
            get { return (Type)GetValue(ClientViewProperty); }
            set { SetValue(ClientViewProperty, value); }
        }

        private object _navigationParameter;
        public object NavigationParameter
        {
            get { return _navigationParameter; }
            set
            {
                _navigationParameter = value;
            }
        }

        public static readonly DependencyProperty ClientViewProperty =
            DependencyProperty.Register(nameof(PageType), typeof(Type), typeof(PopupHostControl), new PropertyMetadata(null, OnPageTypeChanged));

        private static void OnPageTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is PopupHostControl control && args.NewValue is Type pageType)
            {
                control.Navigate();
            }
        }

        public bool IsCloseButtonVisible
        {
            get { return (bool)GetValue(IsCloseButtonVisibleProperty); }
            set { SetValue(IsCloseButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsCloseButtonVisibleProperty =
            DependencyProperty.Register(nameof(IsCloseButtonVisible), typeof(bool), typeof(PopupHostControl), new PropertyMetadata(true));

        public event EventHandler CloseRequested;

        private IPopupPage PopupPage => PopupHostFrame.Content as IPopupPage;

        public PopupHostControl()
        {
            this.InitializeComponent();
        }

        public void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            ClosePopup(true);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Navigate();
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                e.Handled = true;
                ClosePopup(true);
            }

            base.OnKeyDown(e);
        }

        private void Navigate()
        {
            if (PageType != null)
            {
                PopupHostFrame.Navigate(PageType, NavigationParameter, new SuppressNavigationTransitionInfo());

                if (PopupPage != null)
                {
                    PopupPage.ClosePopupRequested += (s, a) => { ClosePopup(false); };
                }
            }
        }

        private void ClosePopup(bool manual)
        {
            if (manual)
            {
                PopupPage?.OnCloseClicked();
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
