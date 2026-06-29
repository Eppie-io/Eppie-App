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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#endif

namespace Eppie.App.UI.Controls
{
    public enum AddressPresenterMode
    {
        Normal,
        Compact,
        Structured
        // Todo: Add an "Extended" mode that displays information in multi-line format
    }

    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class AddressPresenter : UserControl
    {
        public static readonly string AddressElementName = nameof(AddressTextBlock);
        public static readonly string DisplayNameElementName = nameof(DisplayNameTextBlock);

        public AddressPresenterMode Mode
        {
            get { return (AddressPresenterMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(AddressPresenterMode), typeof(AddressPresenter), new PropertyMetadata(AddressPresenterMode.Normal));


        public bool IsAvatarVisible
        {
            get { return (bool)GetValue(IsAvatarVisibleProperty); }
            set { SetValue(IsAvatarVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsAvatarVisibleProperty =
            DependencyProperty.Register(nameof(IsAvatarVisible), typeof(bool), typeof(AddressPresenter), new PropertyMetadata(true));


        public bool IsTextSelectionEnabled
        {
            get { return (bool)GetValue(IsTextSelectionEnabledProperty); }
            set { SetValue(IsTextSelectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsTextSelectionEnabledProperty =
            DependencyProperty.Register(nameof(IsTextSelectionEnabled), typeof(bool), typeof(AddressPresenter), new PropertyMetadata(false));


        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(AddressPresenter), new PropertyMetadata(null));


        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register(nameof(Address), typeof(string), typeof(AddressPresenter), new PropertyMetadata(null));


        public bool IsAddressValid
        {
            get { return (bool)GetValue(IsAddressValidProperty); }
            set { SetValue(IsAddressValidProperty, value); }
        }

        public static readonly DependencyProperty IsAddressValidProperty =
            DependencyProperty.Register(nameof(IsAddressValid), typeof(bool), typeof(AddressPresenter), new PropertyMetadata(true));


        public bool IsSecure
        {
            get { return (bool)GetValue(IsSecureProperty); }
            set { SetValue(IsSecureProperty, value); }
        }

        public static readonly DependencyProperty IsSecureProperty =
            DependencyProperty.Register(nameof(IsSecure), typeof(bool), typeof(AddressPresenter), new PropertyMetadata(false));


        public ImageSource Avatar
        {
            get { return (ImageSource)GetValue(AvatarProperty); }
            set { SetValue(AvatarProperty, value); }
        }

        public static readonly DependencyProperty AvatarProperty =
            DependencyProperty.Register(nameof(Avatar), typeof(ImageSource), typeof(AddressPresenter), new PropertyMetadata(null));


        public bool IsAvatarHighlighted
        {
            get { return (bool)GetValue(IsAvatarHighlightedProperty); }
            set { SetValue(IsAvatarHighlightedProperty, value); }
        }

        public static readonly DependencyProperty IsAvatarHighlightedProperty =
            DependencyProperty.Register(nameof(IsAvatarHighlighted), typeof(bool), typeof(AddressPresenter), new PropertyMetadata(false));


        public double NameColumnMaxWidth
        {
            get { return (double)GetValue(NameColumnMaxWidthProperty); }
            set { SetValue(NameColumnMaxWidthProperty, value); }
        }

        public static readonly DependencyProperty NameColumnMaxWidthProperty =
            DependencyProperty.Register(nameof(NameColumnMaxWidth), typeof(double), typeof(AddressPresenter), new PropertyMetadata(144.0));


        public Style DisplayNameTextStyle
        {
            get { return (Style)GetValue(DisplayNameTextStyleProperty); }
            set { SetValue(DisplayNameTextStyleProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameTextStyleProperty =
            DependencyProperty.Register(nameof(DisplayNameTextStyle), typeof(Style), typeof(AddressPresenter), new PropertyMetadata(null));


        public Style SecureTextStyle
        {
            get { return (Style)GetValue(SecureTextStyleProperty); }
            set { SetValue(SecureTextStyleProperty, value); }
        }

        public static readonly DependencyProperty SecureTextStyleProperty =
            DependencyProperty.Register(nameof(SecureTextStyle), typeof(Style), typeof(AddressPresenter), new PropertyMetadata(null));


        public Style AddressTextStyle
        {
            get { return (Style)GetValue(AddressTextStyleProperty); }
            set { SetValue(AddressTextStyleProperty, value); }
        }

        public static readonly DependencyProperty AddressTextStyleProperty =
            DependencyProperty.Register(nameof(AddressTextStyle), typeof(Style), typeof(AddressPresenter), new PropertyMetadata(null));


        public AddressPresenter()
        {
            this.InitializeComponent();
        }

        public Style GetTextBlockStyle(bool isAddressValid, Style customValidStyle, string defaultValidStyleKey, string defaultInvalidStyleKey)
        {
            if (!isAddressValid)
            {
                return Resources[defaultInvalidStyleKey] as Style;
            }

            return customValidStyle ?? Resources[defaultValidStyleKey] as Style;
        }
    }

    public static partial class DataBinding
    {
        public static double GetNameColumnMaxWidth(AddressPresenterMode mode, double maxWidth)
        {
            return mode == AddressPresenterMode.Structured ? maxWidth : double.PositiveInfinity;
        }
    }
}
