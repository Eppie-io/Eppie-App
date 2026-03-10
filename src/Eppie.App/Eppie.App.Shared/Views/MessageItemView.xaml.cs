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
using System.Windows.Input;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Devices.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
#endif

namespace Eppie.App.Views
{
    public class MessageItemViewEventArgs : EventArgs
    {
        public MessageInfo MessageInfo { get; }
        public MessageItemViewEventArgs(MessageInfo messageInfo)
        {
            MessageInfo = messageInfo;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class MessageItemView : UserControl
    {
        public MessageInfo MessageInfo
        {
            get { return (MessageInfo)GetValue(MessageInfoProperty); }
            set { SetValue(MessageInfoProperty, value); }
        }

        public static readonly DependencyProperty MessageInfoProperty =
            DependencyProperty.Register(nameof(MessageInfo), typeof(MessageInfo), typeof(MessageItemView), new PropertyMetadata(null));

        public event EventHandler<MessageItemViewEventArgs> Deleted;
        public event EventHandler<MessageItemViewEventArgs> Flagged;
        public event EventHandler<MessageItemViewEventArgs> Unflagged;
        public event EventHandler<MessageItemViewEventArgs> ReadMarked;
        public event EventHandler<MessageItemViewEventArgs> UnreadMarked;

        public MessageItemView()
        {
            InitializeComponent();
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse || e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                VisualStateManager.GoToState(sender as Control, "HoverButtonsShown", true);
            }
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "HoverButtonsHidden", true);
        }

        private void OnRightSwipeInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            Deleted?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
        }

        private void OnLeftSwipeInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            if (MessageInfo.IsMarkedAsRead)
            {
                UnreadMarked?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
            }
            else
            {
                ReadMarked?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
            }
        }

        private void OnDeleted(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            Deleted?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
        }

        private void OnFlagged(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            Flagged?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
        }

        private void OnUnflagged(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            Unflagged?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
        }

        private void OnMarkAsRead(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            ReadMarked?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
        }

        private void OnMarkAsUnread(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            UnreadMarked?.Invoke(this, new MessageItemViewEventArgs(MessageInfo));
        }
    }
}
