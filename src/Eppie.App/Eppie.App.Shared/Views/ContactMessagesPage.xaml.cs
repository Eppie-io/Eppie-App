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
    public partial class ContactMessagesPageBase : BasePage<ContactMessagesPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class ContactMessagesPage : ContactMessagesPageBase
    {
        public ContactMessagesPage()
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
            if (ViewModel == null)
            {
                return;
            }
            var filterVariants = ViewModel.GetFilterVariants();
            var selectedFilter = ViewModel.GetSavedSelectedFilter(filterVariants);

            ViewModel.MessageList = new IncrementalLoadingCollection<ContactMessagesPageViewModel, MessageInfo>(
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
