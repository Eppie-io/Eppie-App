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
using System.Threading.Tasks;
using System.Windows.Input;
using Tuvi.App.ViewModels.Services;

namespace Eppie.App.ViewModels.Tests.TestDoubles
{
    internal sealed class TestMessageService : ITuviMailMessageService
    {
        public bool ShowAddAccountMessageCalled { get; private set; }

        public Task ShowAddAccountMessageAsync()
        {
            ShowAddAccountMessageCalled = true;
            return Task.CompletedTask;
        }

        public Task ShowErrorMessageAsync(Exception exception)
        {
            return Task.CompletedTask;
        }

        // Implement other required methods with NotImplementedException
        public Task ShowEnableImapMessageAsync(string forEmail) => throw new NotImplementedException();
        public Task ShowSeedPhraseNotValidMessageAsync() => throw new NotImplementedException();
        public Task ShowPgpPublicKeyAlreadyExistMessageAsync(string fileName) => throw new NotImplementedException();
        public Task ShowPgpUnknownPublicKeyAlgorithmMessageAsync(string fileName) => throw new NotImplementedException();
        public Task ShowPgpPublicKeyImportErrorMessageAsync(string detailedReason, string fileName) => throw new NotImplementedException();
        public Task<bool> ShowWipeAllDataDialogAsync() => throw new NotImplementedException();
        public Task<bool> ShowRemoveAccountDialogAsync() => throw new NotImplementedException();
        public Task<bool> ShowRemoveAIAgentDialogAsync() => throw new NotImplementedException();
        public Task<bool> ShowRemovePgpKeyDialogAsync() => throw new NotImplementedException();
        public Task<bool> ShowRequestReviewMessageAsync() => throw new NotImplementedException();
        public Task ShowNeedToCreateSeedPhraseMessageAsync() => throw new NotImplementedException();
        public Task ShowWhatsNewDialogAsync(string version, bool isStorePaymentProcessor, bool isSupportDevelopmentButtonVisible, string price, ICommand supportDevelopmentCommand, string twitterUrl) => throw new NotImplementedException();
        public Task ShowSupportDevelopmentDialogAsync(bool isStorePaymentProcessor, string price, ICommand supportDevelopmentCommand) => throw new NotImplementedException();
        public Task ShowProtonConnectAddressDialogAsync(object? data = null) => throw new NotImplementedException();
        public Task ShowInvitationDialogAsync(object? invitationData = null) => throw new NotImplementedException();
    }
}
