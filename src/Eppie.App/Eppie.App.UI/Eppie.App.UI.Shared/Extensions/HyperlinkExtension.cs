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
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Extensions
{
    public static class HyperlinkExtension
    {
        public static string GetLink(DependencyObject obj)
        {
            return (string)obj?.GetValue(LinkProperty);
        }
        public static void SetLink(DependencyObject obj, string value)
        {
            obj?.SetValue(LinkProperty, value);
        }
        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.RegisterAttached("Link", typeof(string), typeof(HyperlinkExtension), new PropertyMetadata("", OnLinkPropertyChanged));
        private static void OnLinkPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyperlinkButton hyperlinkButton)
            {
                hyperlinkButton.NavigateUri = new Uri(e.NewValue as string);
            }
        }
    }
}
