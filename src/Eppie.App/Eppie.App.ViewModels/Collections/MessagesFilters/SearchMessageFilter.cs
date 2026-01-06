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

using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class SearchMessageFilter : ObservableObject, ISearchFilter<MessageInfo>
    {
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private static bool EmailContains(EmailAddress email, string text)
        {
            return StringHelper.StringContainsIgnoreCase(email.Address, text)
                || StringHelper.StringContainsIgnoreCase(email.Name, text);
        }

        public bool ItemPassedFilter(MessageInfo item)
        {
            if (item == null || item.WasDeleted)
            {
                return false;
            }

            var message = item.MessageData;

            return string.IsNullOrEmpty(SearchText)
                || StringHelper.StringContainsIgnoreCase(message.Subject, SearchText)
                || StringHelper.StringContainsIgnoreCase(message.PreviewText, SearchText)
                || StringHelper.StringContainsIgnoreCase(message.TextBody, SearchText)
                || message.From.Exists(x => EmailContains(x, SearchText))
                || message.To.Exists(x => EmailContains(x, SearchText))
                || message.ReplyTo.Exists(x => EmailContains(x, SearchText))
                || message.Cc.Exists(x => EmailContains(x, SearchText))
                || message.Bcc.Exists(x => EmailContains(x, SearchText))
            ;
        }
    }
}
