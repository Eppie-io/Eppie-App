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
    public partial class FolderMessagesPageBase : BasePage<FolderMessagesPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class FolderMessagesPage : FolderMessagesPageBase
    {
        public FolderMessagesPage()
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
            if (ViewModel is null)
            {
                return;
            }
            var filterVariants = ViewModel.GetFilterVariants();
            ViewModel.MessageList = new IncrementalLoadingCollection<FolderMessagesPageViewModel, MessageInfo>(
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
