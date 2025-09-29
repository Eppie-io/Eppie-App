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
using System.Threading;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class EthereumAddressSettingsPageViewModel : DecentralizedAddressSettingsPageViewModel
    {
        public IRelayCommand CopySecretKeyCommand { get; }

        public EthereumAddressSettingsPageViewModel()
        {
            CopySecretKeyCommand = new RelayCommand<IClipboardProvider>(CopySecretKey, CanCopySecretKey);
        }

        private bool CanCopySecretKey(IClipboardProvider provider)
        {
            return AddressSettingsModel != null && !string.IsNullOrEmpty(AddressSettingsModel.SecretKeyWIF);
        }

        private void CopySecretKey(IClipboardProvider clipboard)
        {
            if (clipboard != null && AddressSettingsModel?.SecretKeyWIF != null)
            {
                clipboard.SetClipboardContent(AddressSettingsModel.SecretKeyWIF);
            }
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account existing)
                {
                    if (existing.Email.Network != NetworkType.Ethereum)
                    {
                        throw new ArgumentException("The provided account is not a Ethereum account.");
                    }

                    IsCreatingAccountMode = false;
                    AddressSettingsModel = DecentralizedAddressSettingsModel.Create(existing);
                    AddressSettingsModel.SecretKeyWIF = await Core.GetSecurityManager().GetSecretKeyWIFAsync(existing, CancellationToken.None).ConfigureAwait(true);
                }
                else
                {
                    IsCreatingAccountMode = true;
                    var account = await CreateDecentralizedAccountAsync(NetworkType.Ethereum, CancellationToken.None).ConfigureAwait(true);
                    AddressSettingsModel = DecentralizedAddressSettingsModel.Create(account);
                    AddressSettingsModel.SecretKeyWIF = await Core.GetSecurityManager().GetSecretKeyWIFAsync(account, CancellationToken.None).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                CopySecretKeyCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
