using Tuvi.App.ViewModels.Validation;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class ErrorControl : UserControl
    {
        public ErrorControl()
        {
            this.InitializeComponent();
        }


        public IValidatableProperty Property
        {
            get { return (IValidatableProperty)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DependencyProperty PropertyProperty =
            DependencyProperty.Register(nameof(Property), typeof(IValidatableProperty), typeof(ErrorControl), new PropertyMetadata(null));

        public UIElement InnerContent
        {
            get { return (UIElement)GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }
        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register(nameof(InnerContent), typeof(UIElement), typeof(ErrorControl), new PropertyMetadata(null, OnInnerContentChanged));
        private static void OnInnerContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ErrorControl errorControl)
            {
                errorControl.InnerContentPresenter.Content = e.NewValue as UIElement;
            }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ErrorControl), new PropertyMetadata(false, OnIsReadOnlyChanged));
        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ErrorControl errorControl && errorControl.InnerContentPresenter.Content is Control control)
            {
                control.IsEnabled = !(bool)e.NewValue;
            }
        }
    }
}
