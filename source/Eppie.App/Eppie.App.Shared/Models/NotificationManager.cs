using System;
using Tuvi.App.Shared.Services;
using Tuvi.Core;
using Tuvi.Core.Entities;
using Windows.ApplicationModel.Resources;

namespace Tuvi.App.Shared.Models
{
    public class NotificationManager
    {
        private ITuviMail Core { get; }
        private Action<Exception> OnError { get; }

        public NotificationManager(ITuviMail core, Action<Exception> onError)
        {
            Core = core;
            OnError = onError;

            Core.UnreadMessagesReceived += OnUnreadMessagesReceived;
            Core.MessagesIsReadChanged += OnMessagesIsReadChanged;
            Core.MessageDeleted += OnMessageDeleted;
            Core.WipeAllDataNeeded += OnWipeAllDataNeeded;
            Core.AccountDeleted += OnAccountDeleted;
        }

        private void OnAccountDeleted(object sender, AccountEventArgs e)
        {
            UpdateUnreadEmailsBadge();
        }

        private void OnWipeAllDataNeeded(object sender, EventArgs e)
        {
            NotificationService.SetBadgeNumber(0);
        }

        private void OnMessagesIsReadChanged(object sender, MessagesAttributeChangedEventArgs e)
        {
            UpdateUnreadEmailsBadge();
        }

        private void OnUnreadMessagesReceived(object sender, UnreadMessagesReceivedEventArgs e)
        {
            UpdateUnreadEmailsBadge();

            var loader = Eppie.App.Resources.StringProvider.GetInstance();
            NotificationService.ShowToastNotification(loader.GetString("NewEmailsReceivedMessage"));
        }

        private void OnMessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            UpdateUnreadEmailsBadge();
        }

        private async void UpdateUnreadEmailsBadge()
        {
            try
            {
                int unreadCount = await Core.GetUnreadCountForAllAccountsAsync().ConfigureAwait(false);
                NotificationService.SetBadgeNumber(unreadCount);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
