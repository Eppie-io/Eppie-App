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
using Microsoft.Xaml.Interactivity;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class TrimmedTextTooltipBehavior : Behavior<DependencyObject>
    {
        public ITrimmedTextSource Source
        {
            get { return (ITrimmedTextSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ITrimmedTextSource), typeof(TrimmedTextTooltipBehavior), new PropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TrimmedTextTooltipBehavior behavior)
            {
                if (e.OldValue is ITrimmedTextSource oldSource)
                {
                    oldSource.IsTextTrimmedChanged -= behavior.OnSourceIsTextTrimmedChanged;
                }

                if (e.NewValue is ITrimmedTextSource newSource)
                {
                    newSource.IsTextTrimmedChanged += behavior.OnSourceIsTextTrimmedChanged;
                }
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (Source != null)
            {
                Source.IsTextTrimmedChanged += OnSourceIsTextTrimmedChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (Source != null)
            {
                Source.IsTextTrimmedChanged -= OnSourceIsTextTrimmedChanged;
            }
        }

        private void OnSourceIsTextTrimmedChanged(object sender, IsTextTrimmedChangedEventArgs args)
        {
            UpdateToolTip();
        }

        private void UpdateToolTip()
        {
            string tooltip = Source.Text;

            if (!Source.IsTextTrimmed || string.IsNullOrEmpty(tooltip))
            {
                ToolTipService.SetToolTip(AssociatedObject, null);
            }
            else
            {
                ToolTipService.SetToolTip(AssociatedObject, tooltip);
            }
        }
    }

    public class TrimmedTextBlockTooltipBehavior : Behavior<TextBlock>
    {
        private TrimmedTextTooltipBehavior _behavior;

        protected override void OnAttached()
        {
            base.OnAttached();

            _behavior = new TrimmedTextTooltipBehavior()
            {
                Source = new TrimmedTextBlockSource() { Source = AssociatedObject }
            };

            _behavior.Attach(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            _behavior.Detach();
            _behavior = null;
        }
    }
}
