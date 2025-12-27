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
using System.Diagnostics.CodeAnalysis;


#if WINDOWS_UWP
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class AddressItemControl : UserControl
    {
        public event EventHandler Invoked;

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(AddressItemControl), new PropertyMetadata(null));


        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register(nameof(Address), typeof(string), typeof(AddressItemControl), new PropertyMetadata(null));


        public ImageSource Avatar
        {
            get { return (ImageSource)GetValue(AvatarProperty); }
            set { SetValue(AvatarProperty, value); }
        }

        public static readonly DependencyProperty AvatarProperty =
            DependencyProperty.Register(nameof(Avatar), typeof(ImageSource), typeof(AddressItemControl), new PropertyMetadata(null));


        private bool _canInvoke;
        private bool _isPointerOver;

        public AddressItemControl()
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

                    if (_canInvoke)
                    {
                        VisualStateManager.GoToState(this, "Pressed", true);
                    }
                }
            };
        }

        private void OnRootGridTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_canInvoke)
            {
                Invoked?.Invoke(this, EventArgs.Empty);
            }

            _canInvoke = false;
        }
    }
}
