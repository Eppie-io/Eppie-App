using System.Linq;
using Tuvi.App.IncrementalLoading;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml.Navigation;
#endif

namespace Tuvi.App.Shared.Views
{
    public partial class AllMessagesPageBase : BasePage<AllMessagesPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class AllMessagesPage : AllMessagesPageBase
    {
        public AllMessagesPage()
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
    }
}
