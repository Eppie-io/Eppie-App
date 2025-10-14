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
using Tuvi.Core.Entities;
using Tuvi.Proton.Client.Exceptions;

namespace Tuvi.App.ViewModels
{
    public class ProtonAddressSettingsPageViewModel : BaseAddressSettingsPageViewModel
    {
        public class NeedReloginData
        {
            public Account Account { get; set; }
        }

        private bool _isAdvancedSettingsModeActive;
        public bool IsAdvancedSettingsModeActive
        {
            get => _isAdvancedSettingsModeActive;
            set => SetProperty(ref _isAdvancedSettingsModeActive, value);
        }

        private ProtonAddressSettingsModel _addressSettingsModel;
        [CustomValidation(typeof(ProtonAddressSettingsPageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(ProtonAddressSettingsPageViewModel), nameof(ValidatePasswordIsNotEmpty))]
        [CustomValidation(typeof(BaseAddressSettingsPageViewModel), nameof(ValidateEmailIsNotEmpty))]
        [CustomValidation(typeof(BaseAddressSettingsPageViewModel), nameof(ValidateSynchronizationIntervalIsCorrect))]
        public ProtonAddressSettingsModel AddressSettingsModel
        {
            get { return _addressSettingsModel; }
            set
            {
                if (_addressSettingsModel != null)
                {
                    _addressSettingsModel.PropertyChanged -= OnAddressSettingsModelPropertyChanged;
                }

                SetProperty(ref _addressSettingsModel, value, true);
                OnPropertyChanged(nameof(IsEmailReadonly));

                if (_addressSettingsModel != null)
                {
                    _addressSettingsModel.PropertyChanged += OnAddressSettingsModelPropertyChanged;
                }
            }
        }

        private void OnAddressSettingsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ValidateProperty(AddressSettingsModel, nameof(AddressSettingsModel));
        }

        public ProtonAddressSettingsPageViewModel() : base()
        {
        }

        public override void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account accountData)
                {
                    InitModel(ProtonAddressSettingsModel.Create(accountData), false);
                }
                else if (data is NeedReloginData needReloginData)
                {
                    var account = needReloginData.Account;
                    InitModel(ProtonAddressSettingsModel.Create(account), false);

                    AddressSettingsModel.Password.NeedsValidation = true;
                    AddressSettingsModel.Password.Errors.Clear();
                    AddressSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
                }
                else
                {
                    InitModel(ProtonAddressSettingsModel.Create(Account.Default), true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void InitModel(ProtonAddressSettingsModel addressSettingsModel, bool isCreatingMode)
        {
            IsCreatingAccountMode = isCreatingMode;
            IsAdvancedSettingsModeActive = isCreatingMode;
            AddressSettingsModel = addressSettingsModel;
        }

        protected override bool IsValid()
        {
            AddressSettingsModel.Email.NeedsValidation = true;
            AddressSettingsModel.Password.NeedsValidation = true;
            AddressSettingsModel.TwoFactorCode.NeedsValidation = true;
            AddressSettingsModel.MailboxPassword.NeedsValidation = true;

            if ((!IsEmailReadonly && string.IsNullOrEmpty(AddressSettingsModel.Email.Value))
               || string.IsNullOrEmpty(AddressSettingsModel.Password.Value))
            {
                ValidateProperty(AddressSettingsModel, nameof(AddressSettingsModel));
                return false;
            }

            return true;
        }

        protected override Account AddressSettingsModelToAccount()
        {
            return AddressSettingsModel.ToAccount();
        }

        public static ValidationResult ClearValidationErrors(ProtonAddressSettingsModel addressSettingsModel, ValidationContext _)
        {
            if (addressSettingsModel != null)
            {
                addressSettingsModel.Email.Errors.Clear();
                addressSettingsModel.SynchronizationInterval.Errors.Clear();
                addressSettingsModel.Password.Errors.Clear();
                addressSettingsModel.TwoFactorCode.Errors.Clear();
                addressSettingsModel.MailboxPassword.Errors.Clear();
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidatePasswordIsNotEmpty(ProtonAddressSettingsModel addressSettingsModel, ValidationContext context)
        {
            if (context?.ObjectInstance is ProtonAddressSettingsPageViewModel viewModel)
            {
                if (addressSettingsModel != null &&
                    addressSettingsModel.Password.NeedsValidation &&
                    string.IsNullOrEmpty(addressSettingsModel.Password.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    addressSettingsModel.Password.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        protected override async Task<bool> CheckEmailAccountAsync(Account accountData, CancellationToken token = default)
        {
            try
            {
                accountData = await LoginAsync().ConfigureAwait(true);
                return true;
            }
            catch (AuthorizationException)
            {
                AddressSettingsModel.Password.Errors.Clear();
                AddressSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (AuthenticationException)
            {
                AddressSettingsModel.Password.Errors.Clear();
                AddressSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (ProtonSessionRequestException)
            {
                AddressSettingsModel.TwoFactorCode.Errors.Clear();
                AddressSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (NeedAdditionalAuthInfo)
            {
                AddressSettingsModel.TwoFactorCode.Errors.Clear();
                AddressSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("AuthenticationError"));
            }

            throw new NeedAdditionalAuthInfo();
        }

        private async Task<Account> LoginAsync()
        {
            var (userId, refreshToken, saltedKeyPass) = await Proton.ClientAuth.LoginFullAsync
                (
                    AddressSettingsModel.Email.Value,
                    AddressSettingsModel.Password.Value,
                    async (ct) =>
                    {
                        await DispatcherService.RunAsync(() =>
                        {
                            if (string.IsNullOrEmpty(AddressSettingsModel.TwoFactorCode.Value))
                            {
                                AddressSettingsModel.TwoFactorCode.Errors.Clear();
                                AddressSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("NeedTwofactorCode"));

                                throw new NeedAdditionalAuthInfo();
                            }
                        }).ConfigureAwait(true);
                        return AddressSettingsModel.TwoFactorCode.Value;
                    },
                    async (ct) =>
                    {
                        await DispatcherService.RunAsync(() =>
                        {
                            if (string.IsNullOrEmpty(AddressSettingsModel.MailboxPassword.Value))
                            {
                                AddressSettingsModel.MailboxPassword.Errors.Clear();
                                AddressSettingsModel.MailboxPassword.Errors.Add(GetLocalizedString("NeedMailboxPassword"));

                                throw new NeedAdditionalAuthInfo();
                            }
                        }).ConfigureAwait(true);
                        return AddressSettingsModel.MailboxPassword.Value;
                    },
                    default
                ).ConfigureAwait(true);

            var accountData = AddressSettingsModel.ToAccount();
            accountData.AuthData = new ProtonAuthData()
            {
                UserId = userId,
                RefreshToken = refreshToken,
                SaltedPassword = saltedKeyPass
            };
            return accountData;
        }

        protected override async Task ApplySettingsAndGoBackAsync()
        {
            try
            {
                await base.ApplySettingsAndGoBackAsync().ConfigureAwait(true);
            }
            catch (NeedAdditionalAuthInfo)
            {
                AddressSettingsModel.MailboxPassword.Errors.Clear();
                AddressSettingsModel.MailboxPassword.Errors.Add(GetLocalizedString("NeedMailboxPassword"));
            }
        }
    }
}
