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

using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;
using Tuvi.OAuth2;
using TuviPgpLib.Entities;

namespace Eppie.App.ViewModels.Tests.TestDoubles
{
    internal sealed class TestNavigationService : INavigationService
    {

        public enum Page
        {
            Default,
            MainPage,
            WelcomePage,
            SeedGenerator,
            SeedRestorer,
            PasswordManager,
            MessageComposer,
            MessageViewer,
            AllMessages,
            FolderMessages,
            ContactMessages,
            About,
            AddressManager,
            AppSettings,
            EppieAddressSettings,
            BitcoinAddressSettings,
            EthereumAddressSettings,
            ProtonAddressSettings,
            EmailAddressSettings,
            LocalAIAgentSettings,
            ListPgpKeys,
            PgpKeyInformation
        }

        public Page? LastMainPage { get; private set; }
        public object? LastMainData { get; private set; }

        public Page? LastContentPage { get; private set; }
        public object? LastContentData { get; private set; }

        public bool CanGoBack()
        {
            return false;
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void ExitApplication()
        {
            throw new NotImplementedException();
        }

        public void NavigateToMainPage()
        {
            Navigate(Page.MainPage);
        }

        public void NavigateToWelcomePage()
        {
            Navigate(Page.WelcomePage);
        }

        public void NavigateToSeedGenerator()
        {
            Navigate(Page.SeedGenerator);
        }

        public void NavigateToSeedRestorer()
        {
            Navigate(Page.SeedRestorer);
        }

        public void NavigateToSeedRestorer(SeedRestoreActions action)
        {
            Navigate(Page.SeedRestorer, action);

        }

        public void NavigateToPasswordManager(PasswordActions action)
        {
            Navigate(Page.PasswordManager, action);
        }

        public void NavigateToPasswordManager(PasswordStartContext context)
        {
            Navigate(Page.PasswordManager, context);
        }

        public void NavigateToMessageComposer(NewMessageData messageData)
        {
            NavigateContent(Page.MessageComposer, messageData);
        }

        public void NavigateToMessageViewer(MessageInfo messageInfo)
        {
            NavigateContent(Page.MessageViewer, messageInfo);
        }

        public void NavigateToAllMessages(IErrorHandler errorHandler)
        {
            NavigateContent(Page.AllMessages, new AllMessagesPageViewModel.NavigationData() { ErrorHandler = errorHandler });
        }

        public void NavigateToFolderMessages(MailBoxItem mailBoxItem, IErrorHandler errorHandler)
        {
            NavigateContent(Page.FolderMessages, new FolderMessagesPageViewModel.NavigationData() { MailBoxItem = mailBoxItem, ErrorHandler = errorHandler });
        }

        public void NavigateToContactMessages(ContactItem contactItem, IErrorHandler errorHandler)
        {
            NavigateContent(Page.ContactMessages, new ContactMessagesPageViewModel.NavigationData() { ContactItem = contactItem, ErrorHandler = errorHandler });
        }

        public void NavigateToAbout()
        {
            NavigateContent(Page.About);
        }

        public void NavigateToAddressManager()
        {
            NavigateContent(Page.AddressManager);
        }

        public void NavigateToAppSettings()
        {
            NavigateContent(Page.AppSettings);
        }

        public void NavigateToEppieAddressSettings(Account? account = null)
        {
            NavigateContent(Page.EppieAddressSettings, account);
        }

        public void NavigateToBitcoinAddressSettings(Account? account = null)
        {
            NavigateContent(Page.BitcoinAddressSettings, account);
        }

        public void NavigateToEthereumAddressSettings(Account? account = null)
        {
            NavigateContent(Page.EthereumAddressSettings, account);
        }

        public void NavigateToProtonAddressSettings(Account? account = null)
        {
            NavigateContent(Page.ProtonAddressSettings, account);
        }

        public void NavigateToEmailAddressSettings(Account? account = null, bool isReloginNeeded = false)
        {
            if (isReloginNeeded)
            {
                NavigateContent(Page.EmailAddressSettings, new EmailAddressSettingsPageViewModel.NeedReloginData { Account = account });
            }
            else
            {
                NavigateContent(Page.EmailAddressSettings, account);
            }
        }

        public void NavigateToEmailAddressSettings(MailService mailService)
        {
            NavigateContent(Page.EmailAddressSettings, mailService);
        }

        public void NavigateToLocalAIAgentSettings(LocalAIAgent? agent = null)
        {
            NavigateContent(Page.LocalAIAgentSettings, agent);
        }

        public void NavigateToListPgpKeys()
        {
            NavigateContent(Page.ListPgpKeys);
        }

        public void NavigateToPgpKeyInformation(PgpKeyInfo info)
        {
            NavigateContent(Page.PgpKeyInformation, info);
        }

        public void Navigate(Page page, object? data = null)
        {
            LastMainPage = page;
            LastMainData = data;
        }

        public void NavigateContent(Page page, object? data = null)
        {
            LastContentPage = page;
            LastContentData = data;
        }
    }
}
