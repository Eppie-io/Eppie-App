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

using System.Collections.Generic;
using System.Diagnostics;
using Tuvi.Core.Entities;

namespace Tuvi.OAuth2
{
    public static class DefaultAccountConfig
    {
        private static Dictionary<MailService, DefaultAccountData> Config { get; set; } = new Dictionary<MailService, DefaultAccountData>()
        {
            {
                MailService.Gmail,
                new DefaultAccountData()
                {
                    IncomingServerAddress = "imap.gmail.com",
                    OutgoingServerAddress = "smtp.gmail.com",
                    //OutgoingServerPort = 587, // TLS/STARTTLS
                }
            },
            {
                MailService.Outlook,
                new DefaultAccountData()
                {
                    IncomingServerAddress = "outlook.office365.com",
                    OutgoingServerAddress = "smtp.office365.com",
                    OutgoingServerPort = 587, // STARTTLS
                }
            },
        };

        public static Account CreateDefaultOAuth2Account(MailService mailService)
        {
            return CreateOAuth2Account(mailService, null, null);
        }

        public static Account CreateOAuth2Account(MailService mailService, string email, string refreshToken)
        {
            var accountData = Account.Default;

            if (!string.IsNullOrEmpty(email))
            {
                accountData.Email = new EmailAddress(email);
            }

            accountData.AuthData = new OAuth2Data()
            {
                AuthAssistantId = mailService.ToString(),
                RefreshToken = refreshToken
            };

            if (Config.TryGetValue(mailService, out var data))
            {
                accountData.IncomingMailProtocol = data.IncomingMailProtocol;
                accountData.IncomingServerAddress = data.IncomingServerAddress;
                accountData.IncomingServerPort = data.IncomingServerPort;

                accountData.OutgoingMailProtocol = data.OutgoingMailProtocol;
                accountData.OutgoingServerAddress = data.OutgoingServerAddress;
                accountData.OutgoingServerPort = data.OutgoingServerPort;
            }
            else
            {
                Debug.Assert(false, "Configuration is not found");
            }

            return accountData;
        }
    }

    internal class DefaultAccountData
    {
        public MailProtocol IncomingMailProtocol { get; set; } = MailProtocol.IMAP;
        public string IncomingServerAddress { get; set; }
        public int IncomingServerPort { get; set; } = 993;

        public MailProtocol OutgoingMailProtocol { get; set; } = MailProtocol.SMTP;
        public string OutgoingServerAddress { get; set; }
        public int OutgoingServerPort { get; set; } = 465; // SSL
    }
}
