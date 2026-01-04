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

using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class TreeViewItemInvokeBehavior : Behavior<TreeView>
    {
        public ICommand InvokeCommand
        {
            get { return (ICommand)GetValue(InvokeCommandProperty); }
            set { SetValue(InvokeCommandProperty, value); }
        }

        public static readonly DependencyProperty InvokeCommandProperty =
            DependencyProperty.Register(nameof(InvokeCommand), typeof(ICommand), typeof(TreeViewItemInvokeBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            if (AssociatedObject != null)
            {
                //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
                AssociatedObject.ItemInvoked -= OnItemInvoked;
                // End
                AssociatedObject.ItemInvoked += OnItemInvoked;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
                if (AssociatedObject.Parent == null)
                // End
                {
                    AssociatedObject.ItemInvoked -= OnItemInvoked;
                }
            }
        }

        private void OnItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            InvokeCommand?.Execute(args?.InvokedItem);
        }
    }
}
