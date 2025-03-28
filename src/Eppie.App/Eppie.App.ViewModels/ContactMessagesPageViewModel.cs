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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ContactMessagesPageViewModel : MessagesViewModel
    {
        public class NavigationData : BaseNavigationData
        {
            public ContactItem ContactItem { get; set; }
        }

        private ContactItem _contactItem;
        public ContactItem ContactItem
        {
            get { return _contactItem; }
            set
            {
                SetProperty(ref _contactItem, value);
            }
        }

        public override void OnNavigatedTo(object data)
        {
            if (data is NavigationData navigationData)
            {
                ContactItem = navigationData.ContactItem;
            }

            base.OnNavigatedTo(data);
        }

        protected override IEnumerable<MessageInfo> SelectAppropriateMessagesFrom(List<ReceivedMessageInfo> receivedMessages)
        {
            if (ContactItem?.Email != null)
            {
                return receivedMessages.Where(m => m.Message != null
                                                && m.Message.IsFromCorrespondenceWithContact(m.Email, ContactItem.Email)
                                                && !m.Folder.IsJunk
                                                && !m.Folder.IsTrash)
                                       .Select(m => new MessageInfo(m.Email, m.Message));
            }
            else
            {
                return receivedMessages.Select(m => new MessageInfo(m.Email, m.Message));
            }
        }

        protected override Task<IReadOnlyList<Message>> LoadMoreMessagesAsync(int count, Message lastMessage, CancellationToken cancellationToken)
        {
            return Core.GetContactEarlierMessagesAsync(ContactItem.Email, count, lastMessage, cancellationToken);
        }

        protected override async Task RefreshMessagesAsync()
        {
            try
            {
                await Core.CheckForNewInboxMessagesAsync(CancellationTokenSource.Token).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        override protected string GetSavedSelectedFilter()
        {
            return LocalSettingsService.SelectedMailFilterForContactMessagesPage;
        }

        override public void SaveSelectedItemsFilter(string filter)
        {
            LocalSettingsService.SelectedMailFilterForContactMessagesPage = filter;
        }
    }
}
