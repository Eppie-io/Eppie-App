using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tuvi.App.Shared.Models;
using Tuvi.App.Shared.Services;
using Tuvi.App.ViewModels;
using Tuvi.Core.Entities;
using Eppie.App.Shared.Services;



#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Input;
#endif

namespace Tuvi.App.Shared.Views
{
    public partial class BasePage<TViewModel, TViewModelBase> : Page, INotifyPropertyChanged
                 where TViewModel : TViewModelBase
                 where TViewModelBase : BaseViewModel
    {
        public string AppName
        {
            get
            {
                var brand = new BrandLoader();
                return brand.GetName();
            }
        }

        public TViewModel ViewModel { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public BasePage()
        {
            // TODO: investigate a leak here. DependancyPropertyChanged handler retains whole page (TVM-551)
            Uno.UI.Toolkit.VisibleBoundsPadding.SetPaddingMask(this, Uno.UI.Toolkit.VisibleBoundsPadding.PaddingMask.All);

            DataContextChanged += BasePage_DataContextChanged;
        }

        private void DataContextChangedImpl()
        {
            var app = Application.Current as Eppie.App.Shared.App;

            ViewModel = DataContext as TViewModel;
            ViewModel.SetCore(() => app.Core);
            ViewModel.SetNavigationService(app.NavigationService);
            ViewModel.SetLocalSettingsService(app.LocalSettingsService);
            ViewModel.SetAIService(app.AIService);
            ViewModel.SetLocalizationService(new LocalizationService());
            ViewModel.SetMessageService(new MessageService(() => app.XamlRoot));
            ViewModel.SetErrorHandler(new ErrorHandler());
            ViewModel.SetDispatcherService(new DispatcherService());
            ViewModel.SetBrandService(new BrandLoader());
            ViewModel.SetLauncherService(new LauncherService());
            ViewModel.SetPurchaseService(new PurchaseService());
            ViewModel.SetDragAndDropService(new DragAndDropService());
            AfterDataContextChanged();
        }

        // UWP
        public void BasePage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            DataContextChangedImpl();
        }

        // Android, iOS
        public void BasePage_DataContextChanged(DependencyObject sender, DataContextChangedEventArgs args)
        {
            DataContextChangedImpl();
        }

        public virtual void AfterDataContextChanged()
        {
        }

        protected virtual void GoBack(object sender, RoutedEventArgs e)
        {
            if (Frame != null && Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        protected virtual void GoForward(object sender, RoutedEventArgs e)
        {
            if (Frame != null && Frame.CanGoForward)
            {
                Frame.GoForward();
            }
        }

        protected virtual bool CanGoBack()
        {
            return Frame.CanGoBack;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel?.OnNavigatedTo(e?.Parameter);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel?.OnNavigatedFrom();
        }

        protected virtual void OnExceptionOccurred(object sender, ExceptionEventArgs e)
        {
            ViewModel.OnError(e.Exception);
        }

        #region INotifyPropertyChanged implementation
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected void ListViewSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse || e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                VisualStateManager.GoToState(sender as Control, "HoverButtonsShown", true);
            }
        }

        protected void ListViewSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "HoverButtonsHidden", true);
        }
    }
}
