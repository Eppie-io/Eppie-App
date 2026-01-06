// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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
    public class AllMessagesPageViewModel : MessagesViewModel
    {
        public class NavigationData : BaseNavigationData
        {
        }

        protected override IEnumerable<MessageInfo> SelectAppropriateMessagesFrom(List<ReceivedMessageInfo> receivedMessages)
        {
            return receivedMessages.Where(m => m.Folder.IsInbox)
                                   .Select(m => new MessageInfo(m.Email, m.Message));
        }

        protected override Task<IReadOnlyList<Message>> LoadMoreMessagesAsync(int count, Message lastMessage, CancellationToken cancellationToken)
        {
            return Core.GetAllEarlierMessagesAsync(count, lastMessage, cancellationToken);
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
            return LocalSettingsService.SelectedMailFilterForAllMessagesPage;
        }

        override public void SaveSelectedItemsFilter(string filter)
        {
            LocalSettingsService.SelectedMailFilterForAllMessagesPage = filter;
        }
    }
}
