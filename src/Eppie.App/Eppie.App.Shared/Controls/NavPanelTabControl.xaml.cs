using Tuvi.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Controls
{
    public class ControlModelTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ContactsModelTemplate { get; set; }
        public DataTemplate MailBoxesModelTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case ContactsModel _: return ContactsModelTemplate;
                case MailBoxesModel _: return MailBoxesModelTemplate;
                default: return base.SelectTemplateCore(item);
            }
        }
    }

    public sealed partial class NavPanelTabControl : UserControl
    {
        public NavPanelTabModel TabModel
        {
            get { return (NavPanelTabModel)GetValue(TabModelProperty); }
            set { SetValue(TabModelProperty, value); }
        }
        public static readonly DependencyProperty TabModelProperty =
            DependencyProperty.Register(nameof(TabModel), typeof(NavPanelTabModel), typeof(NavPanelTabControl), new PropertyMetadata(null));


        public NavPanelTabControl()
        {
            this.InitializeComponent();
        }
    }
}
