using System;
using System.Collections.Generic;
using System.Windows.Input;
using Tuvi.App.ViewModels;
using Windows.ApplicationModel.DataTransfer;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class MessageListControl : BaseUserControl
    {
        public ManagedCollection<MessageInfo> Messages
        {
            get { return (ManagedCollection<MessageInfo>)GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }
        public static readonly DependencyProperty MessagesProperty =
            DependencyProperty.Register(nameof(Messages), typeof(ManagedCollection<MessageInfo>), typeof(MessageListControl), new PropertyMetadata(null));

        public ICommand MessageClickCommand
        {
            get { return (ICommand)GetValue(MessageClickCommandProperty); }
            set { SetValue(MessageClickCommandProperty, value); }
        }
        public static readonly DependencyProperty MessageClickCommandProperty =
            DependencyProperty.Register(nameof(MessageClickCommand), typeof(ICommand), typeof(MessageListControl), new PropertyMetadata(null));

        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(MessageListControl), new PropertyMetadata(ListViewSelectionMode.None));

        public IList<object> SelectedItems { get; }

        public ICommand SelectedItemsChangedCommand
        {
            get { return (ICommand)GetValue(SelectedItemsChangedCommandProperty); }
            set { SetValue(SelectedItemsChangedCommandProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemsChangedCommandProperty =
            DependencyProperty.Register(nameof(SelectedItemsChangedCommand), typeof(ICommand), typeof(MessageListControl), new PropertyMetadata(null));

        public Control CommandBarHolder
        {
            get { return (Control)GetValue(CommandBarHolderProperty); }
            set { SetValue(CommandBarHolderProperty, value); }
        }
        public static readonly DependencyProperty CommandBarHolderProperty =
            DependencyProperty.Register(nameof(CommandBarHolder), typeof(Control), typeof(MessageListControl), new PropertyMetadata(null));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(MessageListControl), new PropertyMetadata(null));

        public FrameworkElement HeaderHolder
        {
            get { return (FrameworkElement)GetValue(HeaderHolderProperty); }
            set { SetValue(HeaderHolderProperty, value); }
        }
        public static readonly DependencyProperty HeaderHolderProperty =
            DependencyProperty.Register(nameof(HeaderHolder), typeof(FrameworkElement), typeof(MessageListControl), new PropertyMetadata(null));

        public bool IsDeleteInProcess
        {
            get { return (bool)GetValue(IsDeleteInProcessProperty); }
            set { SetValue(IsDeleteInProcessProperty, value); }
        }
        public static readonly DependencyProperty IsDeleteInProcessProperty =
            DependencyProperty.Register(nameof(IsDeleteInProcess), typeof(bool), typeof(MessageListControl), new PropertyMetadata(false));

        public string MessagesDeletedText
        {
            get { return (string)GetValue(MessagesDeletedTextProperty); }
            set { SetValue(MessagesDeletedTextProperty, value); }
        }
        public static readonly DependencyProperty MessagesDeletedTextProperty =
            DependencyProperty.Register(nameof(MessagesDeletedText), typeof(string), typeof(MessageListControl), new PropertyMetadata(string.Empty));

        public ICommand CancelMessagesDeleteCommand
        {
            get { return (ICommand)GetValue(CancelMessagesDeleteCommandProperty); }
            set { SetValue(CancelMessagesDeleteCommandProperty, value); }
        }
        public static readonly DependencyProperty CancelMessagesDeleteCommandProperty =
            DependencyProperty.Register(nameof(CancelMessagesDeleteCommand), typeof(ICommand), typeof(MessageListControl), new PropertyMetadata(null));

        public bool IsWaitingForMoreMessages
        {
            get { return (bool)GetValue(IsWaitingForMoreMessagesProperty); }
            set { SetValue(IsWaitingForMoreMessagesProperty, value); }
        }
        public static readonly DependencyProperty IsWaitingForMoreMessagesProperty =
            DependencyProperty.Register(nameof(IsWaitingForMoreMessages), typeof(bool), typeof(MessageListControl), new PropertyMetadata(false));


        public ICommand StartDragMessagesCommand
        {
            get { return (ICommand)GetValue(StartDragMessagesCommandProperty); }
            set { SetValue(StartDragMessagesCommandProperty, value); }
        }
        public static readonly DependencyProperty StartDragMessagesCommandProperty =
            DependencyProperty.Register(nameof(StartDragMessagesCommand), typeof(ICommand), typeof(MessageListControl), new PropertyMetadata(null));


        public MessageListControl()
        {
            this.InitializeComponent();

            SelectedItems = MessageListView.SelectedItems;
        }

        private void MessageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                foreach (var item in e.RemovedItems)
                {
                    if (SelectedItems.Contains(item))
                    {
                        SelectedItems.Remove(item);
                    }
                }
                foreach (var item in e.AddedItems)
                {
                    if (!SelectedItems.Contains(item))
                    {
                        SelectedItems.Add(item);
                    }
                }

                SelectedItemsChangedCommand?.Execute(null);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void MessageListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items != null)
            {
                e.Data.SetText(nameof(MessageInfo));
                e.Data.RequestedOperation = DataPackageOperation.Move;

                StartDragMessagesCommand?.Execute(e.Items);
            }
        }
        public void SelectAllMessages()
        {
            MessageListView.SelectAll();
        }
    }
}
