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

#if WINDOWS_UWP
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    [ContentProperty(Name = nameof(ItemContent))]
    public sealed partial class ItemContainer : UserControl
    {
        public event EventHandler Invoked;

        public UIElement ItemContent
        {
            get { return (UIElement)GetValue(ItemContentProperty); }
            set { SetValue(ItemContentProperty, value); }
        }

        public static readonly DependencyProperty ItemContentProperty =
            DependencyProperty.Register(nameof(ItemContent), typeof(UIElement), typeof(ItemContainer), new PropertyMetadata(null));


        private bool _canInvoke;
        private bool _isPointerOver;

        public ItemContainer()
        {
            this.InitializeComponent();

            InitializeVisualStateManager();
        }

        private void InitializeVisualStateManager()
        {
            RootGrid.PointerExited += (s, e) =>
            {
                _canInvoke = false;
                _isPointerOver = false;
                VisualStateManager.GoToState(this, "Normal", true);
            };

            RootGrid.PointerEntered += (s, e) =>
            {
                _canInvoke = false;
                _isPointerOver = true;
                VisualStateManager.GoToState(this, "PointerOver", true);
            };

            RootGrid.PointerReleased += (s, e) => VisualStateManager.GoToState(this, _isPointerOver ? "PointerOver" : "Normal", true);

            RootGrid.PointerPressed += (s, e) =>
            {
                if (s is UIElement element)
                {
                    PointerPointProperties properties = e.GetCurrentPoint(element)?.Properties;

                    _canInvoke = e.Pointer.PointerDeviceType != PointerDeviceType.Mouse || properties?.IsLeftButtonPressed == true;

                    if (_canInvoke && Invoked != null)
                    {
                        VisualStateManager.GoToState(this, "Pressed", true);
                    }
                }
            };
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_canInvoke)
            {
                Invoked?.Invoke(this, EventArgs.Empty);
            }

            _canInvoke = false;
        }
    }
}
