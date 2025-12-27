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
using Tuvi.App.ViewModels;
using Tuvi.Core.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace Tuvi.App.Shared.Views
{
    internal partial class AddressManagerPageBase : BasePage<AddressManagerPageViewModel, BaseViewModel>
    {
    }

    internal sealed partial class AddressManagerPage : AddressManagerPageBase
    {
        public AddressManagerPage()
        {
            this.InitializeComponent();
        }

        private void OnItemInvoked(object sender, EventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is AddressItem item)
            {
                ViewModel?.NavigateToAddressSettingsPage(item);
            }
        }

        private void OnAddAddressButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is AddressGroupItem item)
            {
                ViewModel?.NavigateToAddAddressPage(item.Type);
            }
        }

        public static ImageSource GetGroupImage(AddressGroupType type)
        {
            switch (type)
            {
                case AddressGroupType.Eppie:
                    return new SvgImageSource(new Uri("ms-appx:///Assets/Svg/Eppie.svg"));
                case AddressGroupType.Gmail:
                    return new SvgImageSource(new Uri("ms-appx:///Assets/Svg/Gmail.svg"));
                case AddressGroupType.Outlook:
                    return new SvgImageSource(new Uri("ms-appx:///Assets/Svg/Outlook.svg"));
                case AddressGroupType.Proton:
                    return new SvgImageSource(new Uri("ms-appx:///Assets/Svg/Proton.svg"));
                case AddressGroupType.Bitcoin:
                    return new SvgImageSource(new Uri("ms-appx:///Assets/Svg/Bitcoin.svg"));
                case AddressGroupType.Ethereum:
                    return new SvgImageSource(new Uri("ms-appx:///Assets/Svg/Ethereum.svg"));
                case AddressGroupType.OtherEmail:
                default:
                    return new SvgImageSource(new Uri("ms-appx:///Assets/Svg/Address.svg"));
            }
        }

        public static Visibility GetFontIconVisibility(AddressGroupType type)
        {
            return GetGroupImage(type) is null ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GetImageVisibility(AddressGroupType type)
        {
            return GetGroupImage(type) is null ? Visibility.Collapsed : Visibility.Visible;
        }

        public static ImageSource GetAvatar(ImageInfo imageInfo)
        {
            // Todo: Implement image conversion logic
            return null;
        }
    }
}
