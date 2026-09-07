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

using Tuvi.App.ViewModels.Common;
using Tuvi.Core.Entities;
using Tuvi.OAuth2;
using TuviPgpLib.Entities;

namespace Tuvi.App.ViewModels.Services
{
    public interface INavigationService
    {
        void NavigateToMainPage();
        void NavigateToWelcomePage();

        void NavigateToSeedGenerator();
        void NavigateToSeedRestorer();
        void NavigateToSeedRestorer(SeedRestoreActions action);

        void NavigateToPasswordManager(PasswordActions action);
        void NavigateToPasswordManager(PasswordStartContext context);

        void NavigateToMessageComposer(NewMessageData messageData);
        void NavigateToMessageViewer(MessageInfo messageInfo);

        void NavigateToEppieAddressSettings(Account account = null);
        void NavigateToBitcoinAddressSettings(Account account = null);
        void NavigateToEthereumAddressSettings(Account account = null);
        void NavigateToProtonAddressSettings(Account account = null);
        void NavigateToEmailAddressSettings(Account account = null, bool isReloginNeeded = false);
        void NavigateToEmailAddressSettings(MailService mailService);

        void NavigateToLocalAIAgentSettings(LocalAIAgent agent = null);

        void NavigateToListPgpKeys();
        void NavigateToPgpKeyInformation(PgpKeyInfo info);

        bool CanGoBack();
        void GoBack();
        void ExitApplication();
    }
}
