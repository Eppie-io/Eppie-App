using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class MailBoxItem : ObservableObject
    {
        public MailBoxItem()
        {
        }

        public MailBoxItem(EmailAddress email, CompositeFolder folder, string text, bool isRootItem)
        {
            Email = email;
            Folder = folder;
            UnreadMessagesCount = Folder.UnreadCount;
            Path = $"{email?.Address}/{folder?.FullName}";
            Text = text;
            IsRootItem = isRootItem;
        }

        public bool IsRootItem { get; }

        private EmailAddress _email;
        public EmailAddress Email
        {
            get { return _email; }
            set
            {
                SetProperty(ref _email, value);
            }
        }

        private string _path = "";
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        public CompositeFolder Folder { get; private set; }

        private string _text = "";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private int _unreadMessagesCount;
        public int UnreadMessagesCount
        {
            get { return _unreadMessagesCount; }
            set { SetProperty(ref _unreadMessagesCount, value); }
        }

        public ObservableCollection<MailBoxItem> Children { get; } = new ObservableCollection<MailBoxItem>();
    }
}
