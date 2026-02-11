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

using Tuvi.App.IncrementalLoading;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Views
{
    internal partial class FolderMessagesPageBase : BasePage<FolderMessagesPageViewModel, BaseViewModel>
    {
    }

    internal sealed partial class FolderMessagesPage : FolderMessagesPageBase
    {
        public FolderMessagesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            InitAIAgentButton(AIAgentButton, MessageListControl);
            InitializeMessageListAsIncrementalLoadingCollection();
        }
        private void InitializeMessageListAsIncrementalLoadingCollection()
        {
            if (ViewModel is null)
            {
                return;
            }
            var filterVariants = ViewModel.GetFilterVariants();
            var selectedFilter = ViewModel.GetSavedSelectedFilter(filterVariants);

            ViewModel.MessageList = new IncrementalLoadingCollection<FolderMessagesPageViewModel, MessageInfo>(
                ViewModel,
                ViewModel.CancellationTokenSource,
                itemsComparer: new DescOrderByDateMessageComparer(),
                filterVariants: filterVariants,
                itemsFilter: selectedFilter,
                searchFilter: new SearchMessageFilter(),
                onError: ViewModel.OnError);
        }

        public void OnSelectAllButton(object sender, RoutedEventArgs e)
        {
            MessageListControl.SelectAllMessages();
        }

        private void OnDeleteSwipeItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            if (args.SwipeControl.DataContext is IMessageInfo messageInfo)
            {
                ViewModel?.DeleteMessageCommand?.Execute(messageInfo);
            }
        }

        private void OnToggleReadSwipeItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            if (args.SwipeControl.DataContext is IMessageInfo messageInfo)
            {
                if (messageInfo.IsMarkedAsRead)
                {
                    ViewModel?.MarkMessageAsUnreadCommand?.Execute(messageInfo);
                }
                else
                {
                    ViewModel?.MarkMessageAsReadCommand?.Execute(messageInfo);
                }
            }
        }
    }
}
