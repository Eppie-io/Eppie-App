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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
#endif

namespace Eppie.App.UI.Controls
{
    /// <summary>
    /// Provides attached properties for setting tooltips on UI elements.This class makes it easy to add tooltips
    /// to any UWP and WinUI elements using the x:Uid property and string resources with a name in the format:
    /// "{UID_KEY}.[using:Eppie.App.UI.Controls]ToolTipProvider.ToolTip"
    /// </summary>

    public static class ToolTipProvider
    {
        public static DependencyProperty ToolTipProperty { get; } = DependencyProperty.RegisterAttached("ToolTip", typeof(object), typeof(ToolTipProvider), new PropertyMetadata(null, OnUpdateToolTip));

        public static object GetToolTip(DependencyObject element)
        {
            return element.GetValue(ToolTipProperty);
        }

        public static void SetToolTip(DependencyObject element, object value)
        {
            element.SetValue(ToolTipProperty, value);
        }

        public static DependencyProperty PlacementProperty { get; } = DependencyProperty.RegisterAttached("Placement", typeof(PlacementMode), typeof(ToolTipProvider), new PropertyMetadata(PlacementMode.Top, OnUpdateToolTip));

        public static PlacementMode GetPlacement(DependencyObject element)
        {
            return (PlacementMode)element.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject element, PlacementMode value)
        {
            element.SetValue(PlacementProperty, value);
        }

        public static DependencyProperty PlacementTargetProperty { get; } = DependencyProperty.RegisterAttached("PlacementTarget", typeof(UIElement), typeof(ToolTipProvider), new PropertyMetadata(null, OnUpdateToolTip));

        public static UIElement GetPlacementTarget(DependencyObject element)
        {
            return (UIElement)element.GetValue(PlacementTargetProperty);
        }

        public static void SetPlacementTarget(DependencyObject element, UIElement value)
        {
            element.SetValue(PlacementTargetProperty, value);
        }

        private static void OnUpdateToolTip(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                object content = GetToolTip(element);

                if (content != null)
                {
                    ToolTipService.SetToolTip(element, content);
                    ToolTipService.SetPlacement(element, GetPlacement(element));
                    ToolTipService.SetPlacementTarget(element, GetPlacementTarget(element));
                }
                else
                {
                    ToolTipService.SetToolTip(element, null);
                }
            }
        }
    }
}
