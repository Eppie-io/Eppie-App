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

using Tuvi.App.IncrementalLoading;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml.Navigation;
#endif

namespace Tuvi.App.Shared.Views
{
    internal partial class AllMessagesPageBase : BasePage<AllMessagesPageViewModel, BaseViewModel>
    {
    }

    internal sealed partial class AllMessagesPage : AllMessagesPageBase
    {
        public AllMessagesPage()
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
            if (ViewModel is null ||
                ViewModel.MessageList is IncrementalLoadingCollection<AllMessagesPageViewModel, MessageInfo>)
            {
                return;
            }

            var filterVariants = ViewModel.GetFilterVariants();
            var selectedFilter = ViewModel.GetSavedSelectedFilter(filterVariants);

            ViewModel.MessageList = new IncrementalLoadingCollection<AllMessagesPageViewModel, MessageInfo>(
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
    }
}
