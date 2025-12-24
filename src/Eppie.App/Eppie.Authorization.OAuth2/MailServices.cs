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
using Tuvi.Core.Entities;

namespace Tuvi.OAuth2
{
    public enum MailService
    {
        Unknown,
        Gmail,
        Outlook
    }

    public static class MailServiceExtensions
    {
        public static bool IsMailService(this Account account, MailService mailService)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return account.Type == MailBoxType.Email
                && account.AuthData != null
                && account.AuthData.Type == AuthenticationType.OAuth2
                && account.AuthData is OAuth2Data data
                && data.AuthAssistantId == Enum.GetName(typeof(MailService), mailService);
        }

        public static bool IsGmail(this Account account)
        {
            return account.IsMailService(MailService.Gmail);
        }

        public static bool IsOutlook(this Account account)
        {
            return account.IsMailService(MailService.Outlook);
        }
    }
}
