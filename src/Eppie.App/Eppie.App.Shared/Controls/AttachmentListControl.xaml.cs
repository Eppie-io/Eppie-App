using System.Collections.Generic;
using Tuvi.Core.Entities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class AttachmentListControl : UserControl
    {
        public IList<Attachment> Items
        {
            get { return (IList<Attachment>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IList<Attachment>), typeof(AttachmentListControl), new PropertyMetadata(null));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(AttachmentListControl), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(AttachmentListControl), new PropertyMetadata(""));

        public bool IsTitleVisible
        {
            get { return (bool)GetValue(IsTitleVisibleProperty); }
            set { SetValue(IsTitleVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsTitleVisibleProperty =
            DependencyProperty.Register(nameof(IsTitleVisible), typeof(bool), typeof(AttachmentListControl), new PropertyMetadata(false));

        public GridLength ListAreaHeight
        {
            get { return (GridLength)GetValue(ListAreaHeightProperty); }
            set { SetValue(ListAreaHeightProperty, value); }
        }
        public static readonly DependencyProperty ListAreaHeightProperty =
            DependencyProperty.Register(nameof(ListAreaHeight), typeof(GridLength), typeof(AttachmentListControl), new PropertyMetadata(new GridLength(88)));

        public AttachmentListControl()
        {
            this.InitializeComponent();
        }
    }
}
