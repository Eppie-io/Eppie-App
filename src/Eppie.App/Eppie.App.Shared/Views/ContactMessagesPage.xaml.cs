using System.Linq;
using Tuvi.App.IncrementalLoading;
using Tuvi.App.ViewModels;
using Windows.UI.Xaml;


#if WINDOWS_UWP
using Windows.UI.Xaml.Navigation;
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
