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
using Eppie.App.Views;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;
using Tuvi.OAuth2;
using TuviPgpLib.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Services
{
    public class NavigationService : INavigationService
    {
        protected Frame MainFrame { get; }
        protected Frame ContentFrame { get; private set; }

        public NavigationService(Frame mainFrame)
        {
            MainFrame = mainFrame;
        }

        public void SetContentFrame(Frame contentFrame)
        {
            ContentFrame = contentFrame;
        }


        public void NavigateToMainPage()
        {
            Navigate(typeof(MainPage));
        }

        public void NavigateToWelcomePage()
        {
            Navigate(typeof(WelcomePage));
        }

        public void NavigateToSeedGenerator()
        {
            Navigate(typeof(SeedGeneratePage));
        }

        public void NavigateToSeedRestorer()
        {
            Navigate(typeof(SeedRestorePage));
        }

        public void NavigateToSeedRestorer(SeedRestoreActions action)
        {
            Navigate(typeof(SeedRestorePage), action);
        }

        public void NavigateToPasswordManager(PasswordActions action)
        {
            Navigate(typeof(PasswordPage), action);
        }

        public void NavigateToPasswordManager(PasswordStartContext context)
        {
            Navigate(typeof(PasswordPage), context);
        }


        public void NavigateToMessageComposer(NewMessageData messageData)
        {
            NavigateContent(typeof(ComposeMessagePage), messageData);
        }

        public void NavigateToMessageViewer(MessageInfo messageInfo)
        {
            NavigateContent(typeof(MessagePage), messageInfo);
        }

        public void NavigateToEppieAddressSettings(Account account = null)
        {
            NavigateContent(typeof(EppieAddressSettingsPage), account);
        }

        public void NavigateToBitcoinAddressSettings(Account account = null)
        {
            NavigateContent(typeof(BitcoinAddressSettingsPage), account);
        }

        public void NavigateToEthereumAddressSettings(Account account = null)
        {
            NavigateContent(typeof(EthereumAddressSettingsPage), account);
        }

        public void NavigateToProtonAddressSettings(Account account = null)
        {
            NavigateContent(typeof(ProtonAddressSettingsPage), account);
        }

        public void NavigateToEmailAddressSettings(Account account = null, bool isReloginNeeded = false)
        {
            if (isReloginNeeded)
            {
                NavigateContent(typeof(EmailAddressSettingsPage), new EmailAddressSettingsPageViewModel.NeedReloginData { Account = account });
            }
            else
            {
                NavigateContent(typeof(EmailAddressSettingsPage), account);
            }
        }

        public void NavigateToEmailAddressSettings(MailService mailService)
        {
            NavigateContent(typeof(EmailAddressSettingsPage), mailService);
        }

        public void NavigateToLocalAIAgentSettings(LocalAIAgent agent = null)
        {
            NavigateContent(typeof(LocalAIAgentSettingsPage), agent);
        }

        public void NavigateToListPgpKeys()
        {
            NavigateContent(typeof(PgpKeysPage));
        }

        public void NavigateToPgpKeyInformation(PgpKeyInfo info)
        {
            NavigateContent(typeof(PgpKeyPage), info);
        }

        public bool CanGoBack()
        {
            var frame = GetGoBackFrame();
            return frame != null && frame.CanGoBack;
        }

        public void GoBack()
        {
            var frame = GetGoBackFrame();
            if (frame != null && frame.CanGoBack)
            {
                frame.GoBack();
            }
        }

        public void ExitApplication()
        {
            Application.Current.Exit(); // ToDo: Uno0001
        }

        private void Navigate(Type pageType, object parameter = null)
        {
            MainFrame?.Navigate(pageType, parameter);
        }

        private void NavigateContent(Type pageType, object parameter = null)
        {
            var frame = ContentFrame ?? MainFrame;
            frame?.Navigate(pageType, parameter);
        }

        private Frame GetGoBackFrame()
        {
            if (ContentFrame != null && ContentFrame.IsLoaded && ContentFrame.CanGoBack)
            {
                return ContentFrame;
            }

            return MainFrame;
        }
    }
}
