using Tuvi.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class MailBoxesListControl : UserControl
    {
        public MailBoxesModel MailBoxesModel
        {
            get { return (MailBoxesModel)GetValue(MailBoxesModelProperty); }
            set { SetValue(MailBoxesModelProperty, value); }
        }
        public static readonly DependencyProperty MailBoxesModelProperty =
            DependencyProperty.Register(nameof(MailBoxesModel), typeof(MailBoxesModel), typeof(MailBoxesListControl), new PropertyMetadata(null));

        public MailBoxesListControl()
        {
            this.InitializeComponent();
        }
    }
}
