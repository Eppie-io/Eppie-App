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
    public interface ITooltipSource
    {
        bool IsActive { get; }
        string Text { get; }

        event EventHandler TooltipChanged;
    }

    public interface ITooltipSource<T> : ITooltipSource
#if HAS_UNO
        where T : class, DependencyObject
#else
        where T : DependencyObject
#endif
    {
        T Source { get; set; }
    }

    public class TooltipBehavior : Behavior<DependencyObject>
    {
        public ITooltipSource Source
        {
            get { return (ITooltipSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ITooltipSource), typeof(TooltipBehavior), new PropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TooltipBehavior behavior)
            {
                if (e.OldValue is ITooltipSource oldSource)
                {
                    oldSource.TooltipChanged -= behavior.OnTooltipChanged;
                }

                if (e.NewValue is ITooltipSource newSource)
                {
                    newSource.TooltipChanged += behavior.OnTooltipChanged;
                }
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (Source != null)
            {
                Source.TooltipChanged += OnTooltipChanged;
                UpdateToolTip();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (Source != null)
            {
                Source.TooltipChanged -= OnTooltipChanged;
            }
        }

        private void OnTooltipChanged(object sender, EventArgs args)
        {
            UpdateToolTip();
        }

        private void UpdateToolTip()
        {
            string tooltip = Source.Text;

            if (!Source.IsActive || string.IsNullOrEmpty(tooltip))
            {
                ToolTipService.SetToolTip(AssociatedObject, null);
            }
            else
            {
                ToolTipService.SetToolTip(AssociatedObject, tooltip);
            }
        }
    }


    public class TooltipBehavior<T, TSource> : Behavior<T>
#if HAS_UNO
        where T : class, DependencyObject
#else
        where T : DependencyObject
#endif
        where TSource : ITooltipSource<T>, new()

    {
        private TooltipBehavior _behavior;

        protected override void OnAttached()
        {
            base.OnAttached();

            _behavior = new TooltipBehavior()
            {
                Source = new TSource() { Source = AssociatedObject }
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
