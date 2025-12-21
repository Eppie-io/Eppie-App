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
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class BitcoinAddressSettingsPageViewModel : DecentralizedAddressSettingsPageViewModel
    {
        public IRelayCommand ActivateAddressCommand { get; }

        private bool _isActivationEnabled = true;
        public bool IsActivationEnabled
        {
            get => _isActivationEnabled;
            private set
            {
                SetProperty(ref _isActivationEnabled, value);
                ActivateAddressCommand.NotifyCanExecuteChanged();
            }
        }

        private string _activationStatus = string.Empty;
        public string ActivationStatus
        {
            get => _activationStatus;
            private set => SetProperty(ref _activationStatus, value);
        }

        private bool _isActivationError;
        public bool IsActivationError
        {
            get => _isActivationError;
            private set => SetProperty(ref _isActivationError, value);
        }

        private bool _isActivationSubmitted;
        public bool IsActivationSubmitted
        {
            get => _isActivationSubmitted;
            private set
            {
                SetProperty(ref _isActivationSubmitted, value);
                ValidateActivationProperty();
            }
        }

        private bool _isActivationStatusVisible;
        public bool IsActivationStatusVisible
        {
            get => _isActivationStatusVisible;
            private set => SetProperty(ref _isActivationStatusVisible, value);
        }

        private CancellationTokenSource _activationCts;

        private const int ActivationPollIntervalMs = 15_000;

        [CustomValidation(typeof(BitcoinAddressSettingsPageViewModel), nameof(ValidateActivation))]
        public object ActivationValidation => null;

        public BitcoinAddressSettingsPageViewModel() : base()
        {
            ActivateAddressCommand = new AsyncRelayCommand(ActivateAddressAsync, () => IsActivationEnabled && AddressSettingsModel != null);
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account existing)
                {
                    if (existing.Email.Network != NetworkType.Bitcoin)
                    {
                        throw new ArgumentException("The provided account is not a Bitcoin account.");
                    }

                    IsCreatingAccountMode = false;
                    IsActivationStatusVisible = true;
                    AddressSettingsModel = DecentralizedAddressSettingsModel.Create(existing);
                    AddressSettingsModel.SecretKeyWIF = await Core.GetSecurityManager().GetSecretKeyWIFAsync(existing, CancellationToken.None).ConfigureAwait(true);

                    SetActivationStatus("ActivationSent");
                    _ = MonitoringPublicKeyAsync();
                }
                else
                {
                    IsCreatingAccountMode = true;
                    IsActivationSubmitted = false;
                    var account = await CreateDecentralizedAccountAsync(NetworkType.Bitcoin, CancellationToken.None).ConfigureAwait(true);
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
                ActivateAddressCommand.NotifyCanExecuteChanged();
                ApplySettingsCommand.NotifyCanExecuteChanged();
            }
        }

        private void ValidateActivationProperty()
        {
            ValidateProperty(ActivationValidation, nameof(ActivationValidation));
            ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        public static ValidationResult ValidateActivation(object _, ValidationContext context)
        {
            var vm = context?.ObjectInstance as BitcoinAddressSettingsPageViewModel;
            if (vm is null)
            {
                return ValidationResult.Success;
            }

            if (!vm.IsCreatingAccountMode)
            {
                return ValidationResult.Success;
            }

            if (!vm.IsActivationSubmitted)
            {
                var error = vm.GetLocalizedString("ActivationRequired");
                return new ValidationResult(error);
            }

            return ValidationResult.Success;
        }

        protected override void OnCancel()
        {
            try
            {
                _activationCts?.Cancel();
            }
            finally
            {
                _activationCts?.Dispose();
                _activationCts = null;
            }

            SetActivationError("ActivationCanceled");
            EndActivation();

            base.OnCancel();
        }

        private void StartActivation()
        {
            IsActivationEnabled = false;
            IsWaitingResponse = true;
            IsActivationError = false;

            SetActivationStatus("ActivationStarting");
        }

        private void EndActivation()
        {
            IsWaitingResponse = false;
            IsActivationEnabled = !IsActivationSubmitted;

            ActivateAddressCommand.NotifyCanExecuteChanged();
        }

        private void SetActivationStatus(string resourceKey)
        {
            ActivationStatus = GetLocalizedString(resourceKey);
        }

        private void SetActivationError(string resourceKey)
        {
            IsActivationError = true;
            SetActivationStatus(resourceKey);
        }

        private async Task ActivateAddressAsync()
        {
            try
            {
                _activationCts?.Cancel();
            }
            finally
            {
                _activationCts?.Dispose();
                _activationCts = null;
            }

            _activationCts = new CancellationTokenSource();
            var token = _activationCts.Token;

            try
            {
                StartActivation();

                if (AddressSettingsModel is null)
                {
                    return;
                }

                var account = AddressSettingsModel.ToAccount();

                await Core.GetSecurityManager().ActivateAddressAsync(account, token).ConfigureAwait(true);

                IsActivationSubmitted = true;
                SetActivationStatus("ActivationSent");
            }
            catch (OperationCanceledException)
            {
                SetActivationError("ActivationCanceled");
            }
            catch (Exception)
            {
                SetActivationError("ActivationFailed");
            }
            finally
            {
                EndActivation();

                _activationCts?.Dispose();
                _activationCts = null;
            }
        }

        private async Task MonitoringPublicKeyAsync()
        {
            try
            {
                _activationCts?.Cancel();
            }
            finally
            {
                _activationCts?.Dispose();
                _activationCts = null;
            }

            _activationCts = new CancellationTokenSource();
            var token = _activationCts.Token;

            var account = AddressSettingsModel.ToAccount();
            var email = account?.Email;
            string pubKey = null;

            if (email is null)
            {
                return;
            }

            try
            {
                while (true)
                {
                    pubKey = await Core.GetSecurityManager().GetEmailPublicKeyStringAsync(email, token).ConfigureAwait(true);

                    if (!string.IsNullOrEmpty(pubKey))
                    {
                        IsActivationStatusVisible = false;
                        SetActivationStatus("ActivationCompleted");

                        return;
                    }

                    await Task.Delay(ActivationPollIntervalMs, token).ConfigureAwait(true);
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                _activationCts?.Dispose();
                _activationCts = null;
            }
        }
    }
}
