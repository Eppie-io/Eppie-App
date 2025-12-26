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

using Tuvi.App.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Tuvi.App.Shared.Controls
{
#if WINDOWS_UWP
    public sealed partial class MailBoxesListControl : Windows.UI.Xaml.Controls.UserControl
#else
    public sealed partial class MailBoxesListControl : UserControl
#endif
    {
        public MailBoxesModel MailBoxesModel
        {
            get { return (MailBoxesModel)GetValue(MailBoxesModelProperty); }
            set { SetValue(MailBoxesModelProperty, value); }
        }
        public static readonly DependencyProperty MailBoxesModelProperty =
            DependencyProperty.Register(nameof(MailBoxesModel), typeof(MailBoxesModel), typeof(MailBoxesListControl), new PropertyMetadata(null));

        public MailBoxesListControl()
        {
            this.InitializeComponent();
        }

#if WINDOWS_UWP
        private void MailBoxTreeView_DragOver(object sender, DragEventArgs e)
#else
        private void MailBoxTreeView_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
#endif
        {
            e.AcceptedOperation = DataPackageOperation.None;

            if (sender is TreeView treeView)
            {
                var pointerPosition = e.GetPosition(treeView);

                var hoveredNode = GetTreeViewNodeAtPoint(treeView, pointerPosition);

                var hoveredMailBoxItem = hoveredNode?.Content as MailBoxItem;
                if (hoveredMailBoxItem != null)
                {
                    if (MailBoxesModel.IsDropAllowed(hoveredMailBoxItem))
                    {
                        e.AcceptedOperation = DataPackageOperation.Move;
                        hoveredNode.IsExpanded = hoveredNode.HasChildren;
                    }
                }
            }
        }

        private static TreeViewNode GetTreeViewNodeAtPoint(TreeView treeView, Point position)
        {
            foreach (var item in treeView.RootNodes)
            {
                var node = FindNodeAtPoint(treeView, item, position);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        private static TreeViewNode FindNodeAtPoint(TreeView treeView, TreeViewNode node, Point position)
        {
            var container = treeView.ContainerFromNode(node) as TreeViewItem;

            if (container != null)
            {
                var bounds = container.TransformToVisual(treeView).TransformBounds(new Rect(0, 0, container.ActualWidth, container.ActualHeight));

                if (bounds.Contains(position))
                {
                    return node;
                }
            }

            foreach (var childNode in node.Children)
            {
                var result = FindNodeAtPoint(treeView, childNode, position);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }


#if WINDOWS_UWP
        private void MailBoxTreeView_Drop(object sender, DragEventArgs e)
#else
        private void MailBoxTreeView_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
#endif
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var deferral = e.GetDeferral();

                var targetNode = GetTreeViewNodeAtPoint(sender as TreeView, e.GetPosition(sender as TreeView));
                if (targetNode != null)
                {
                    var targetMailBoxItem = targetNode.Content as MailBoxItem;
                    MailBoxesModel.ItemDrop(targetMailBoxItem);
                }

                deferral.Complete();
            }
        }

    }
}
