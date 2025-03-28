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

using Microsoft.Xaml.Interactivity;
using System;
using System.Windows.Input;
using Windows.System;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class DialogBehavior : Behavior<Page>
    {
        public ICommand ApplyCommand
        {
            get { return (ICommand)GetValue(ApplyCommandProperty); }
            set { SetValue(ApplyCommandProperty, value); }
        }
        public static readonly DependencyProperty ApplyCommandProperty =
            DependencyProperty.Register(nameof(ApplyCommand), typeof(ICommand), typeof(DialogBehavior), new PropertyMetadata(null));

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(DialogBehavior), new PropertyMetadata(null));

        public ICommand HandleErrorCommand
        {
            get { return (ICommand)GetValue(HandleErrorCommandProperty); }
            set { SetValue(HandleErrorCommandProperty, value); }
        }
        public static readonly DependencyProperty HandleErrorCommandProperty =
            DependencyProperty.Register(nameof(HandleErrorCommand), typeof(ICommand), typeof(DialogBehavior), new PropertyMetadata(null));


        protected override void OnAttached()
        {
            if (AssociatedObject is Page page)
            {
                page.KeyDown += OnKeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is Page page)
            {
                page.KeyDown -= OnKeyDown;
            }
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == VirtualKey.Enter)
                {
                    ApplyCommand?.Execute(null);
                }
                else if (e.Key == VirtualKey.Escape)
                {
                    CancelCommand?.Execute(null);
                }
            }
            catch (Exception ex)
            {
                HandleErrorCommand?.Execute(ex);
            }
        }
    }
}
