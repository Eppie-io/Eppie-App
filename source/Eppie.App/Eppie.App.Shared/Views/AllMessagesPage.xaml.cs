using System.Linq;
using Tuvi.App.IncrementalLoading;
using Tuvi.App.ViewModels;
using Windows.UI.Xaml.Navigation;

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
            ViewModel.MessageList = new IncrementalLoadingCollection<AllMessagesPageViewModel, MessageInfo>(
                ViewModel,
                ViewModel.CancellationTokenSource,
                itemsComparer: new DescOrderByDateMessageComparer(),
                filterVariants: filterVariants,
                itemsFilter: filterVariants.OfType<AllMessagesFilter>().FirstOrDefault(),
                searchFilter: new SearchMessageFilter(),
                onError: ViewModel.OnError);
        }
    }
}
