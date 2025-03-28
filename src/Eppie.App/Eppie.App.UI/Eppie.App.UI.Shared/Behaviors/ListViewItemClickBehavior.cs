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
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class ListViewItemClickBehavior : Behavior<ListViewBase>
    {
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(ListViewItemClickBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
            AssociatedObject.ItemClick -= OnItemClick;
            // End
            AssociatedObject.ItemClick += OnItemClick;
        }

        protected override void OnDetaching()
        {
            //TODO: TVM-283 Remove when Microsoft fixes bug: https://github.com/microsoft/microsoft-ui-xaml/issues/4999
            if (AssociatedObject.Parent == null)
            // End
            {
                AssociatedObject.ItemClick -= OnItemClick;
            }
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (ClickCommand != null && ClickCommand.CanExecute(e?.ClickedItem))
            {
                ClickCommand.Execute(e?.ClickedItem);
            }
        }
    }
}
