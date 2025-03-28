// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System;
using Tuvi.App.Shared.Services;
using Tuvi.Core;
using Tuvi.Core.Entities;

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

            if (e.Folder.IsInbox)
            {
                var loader = Eppie.App.UI.Resources.StringProvider.GetInstance();
                NotificationService.ShowToastNotification(loader.GetString("NewEmailsReceivedMessage"));
            }
        }

        private void OnMessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            UpdateUnreadEmailsBadge();
        }

        private async void UpdateUnreadEmailsBadge()
        {
            try
            {
                int unreadCount = await Core.GetUnreadCountForAllAccountsInboxAsync().ConfigureAwait(false);
                NotificationService.SetBadgeNumber(unreadCount);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
