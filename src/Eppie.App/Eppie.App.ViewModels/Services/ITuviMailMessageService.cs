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

using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public interface ITuviMailMessageService : IMessageService
    {
        Task ShowEnableImapMessageAsync(string forEmail);
        Task ShowAddAccountMessageAsync();

        Task ShowSeedPhraseNotValidMessageAsync();

        Task ShowPgpPublicKeyAlreadyExistMessageAsync(string fileName);
        Task ShowPgpUnknownPublicKeyAlgorithmMessageAsync(string fileName);
        Task ShowPgpPublicKeyImportErrorMessageAsync(string detailedReason, string fileName);

        Task<bool> ShowWipeAllDataDialogAsync();
        Task<bool> ShowRemoveAccountDialogAsync();
        Task<bool> ShowRemoveAIAgentDialogAsync();
        Task<bool> ShowRequestReviewMessageAsync();

        Task ShowNeedToCreateSeedPhraseMessageAsync();
    }
}
