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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.Core;
using Tuvi.Core.Entities;
using Tuvi.Core.Web.BackupService.Client;

namespace Tuvi.App.ViewModels.Extensions
{
    internal static class CoreExtensions
    {
        internal static async Task BackupIfNeededAsync(this ITuviMail core)
        {
            var accounts = await core.GetAccountsAsync().ConfigureAwait(true);
            var isSeedInitialized = await core.GetSecurityManager().IsSeedPhraseInitializedAsync().ConfigureAwait(true);

            // ToDo: Check if we need to have a backup even if there are no accounts.
            // `accounts.Count` should be greater than 0. If we can have a backup even if there are no accounts,
            // then we can remove the check for accounts.Count >= 0. But if we want to have a backup
            // only if there are accounts, then we should change the condition to accounts.Count > 0.
            if (accounts.Count >= 0 && isSeedInitialized)
            {
                // Test node URI
                const string uploadUrl = "https://testnet.eppie.io/api/UploadBackupFunction?code=1";
                // Local node URI
                //const string uploadUrl = "http://localhost:7071/api/UploadBackupFunction";

                var fingerprint = core.GetBackupManager().GetBackupKeyFingerprint();

                using (var backup = new MemoryStream())
                using (var deatachedSignatureData = new MemoryStream())
                using (var publicKey = new MemoryStream())
                {
                    await core.GetBackupManager().CreateBackupAsync(backup).ConfigureAwait(true);
                    await core.GetBackupManager().CreateDetachedSignatureDataAsync(backup, deatachedSignatureData, publicKey).ConfigureAwait(true);

                    await BackupServiceClient.UploadAsync(new Uri(uploadUrl), fingerprint, publicKey, deatachedSignatureData, backup).ConfigureAwait(true);
                }
            }
        }

        internal static async Task ProcessAccountDataAsync(this ITuviMail core, Account account, CancellationToken cancellationToken = default)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            bool existAccount = await core.ExistsAccountWithEmailAddressAsync(account.Email, cancellationToken).ConfigureAwait(true);

            if (!existAccount)
            {
                await core.AddAccountAsync(account, cancellationToken).ConfigureAwait(true);
            }
            else
            {
                await core.UpdateAccountAsync(account, cancellationToken).ConfigureAwait(true);
            }

            await core.BackupIfNeededAsync().ConfigureAwait(true);
        }
    }
}
