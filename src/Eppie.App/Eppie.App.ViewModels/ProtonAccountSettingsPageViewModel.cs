﻿// ---------------------------------------------------------------------------- //
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
    public class ProtonAccountSettingsPageViewModel : BaseAccountSettingsPageViewModel
    {
        public class NeedReloginData
        {
            public Account Account { get; set; }
        }

        private ProtonAccountSettingsModel _accountSettingsModel;
        [CustomValidation(typeof(ProtonAccountSettingsPageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(ProtonAccountSettingsPageViewModel), nameof(ValidatePasswordIsNotEmpty))]
        [CustomValidation(typeof(BaseAccountSettingsPageViewModel), nameof(ValidateEmailIsNotEmpty))]
        [CustomValidation(typeof(BaseAccountSettingsPageViewModel), nameof(ValidateSynchronizationIntervalIsCorrect))]
        public ProtonAccountSettingsModel AccountSettingsModel
        {
            get { return _accountSettingsModel; }
            set
            {
                if (_accountSettingsModel != null)
                {
                    _accountSettingsModel.PropertyChanged -= OnAccountSettingsModelPropertyChanged;
                }

                SetProperty(ref _accountSettingsModel, value, true);
                OnPropertyChanged(nameof(IsEmailReadonly));

                if (_accountSettingsModel != null)
                {
                    _accountSettingsModel.PropertyChanged += OnAccountSettingsModelPropertyChanged;
                }
            }
        }

        private void OnAccountSettingsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ValidateProperty(AccountSettingsModel, nameof(AccountSettingsModel));
        }

        public ProtonAccountSettingsPageViewModel() : base()
        {
        }

        public override void OnNavigatedTo(object data)
        {
            try
            {
                if (data is Account accountData)
                {
                    InitModel(ProtonAccountSettingsModel.Create(accountData), false);
                }
                else if (data is NeedReloginData needReloginData)
                {
                    var account = needReloginData.Account;
                    InitModel(ProtonAccountSettingsModel.Create(account), false);

                    AccountSettingsModel.Password.NeedsValidation = true;
                    AccountSettingsModel.Password.Errors.Clear();
                    AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
                }
                else
                {
                    InitModel(ProtonAccountSettingsModel.Create(Account.Default), true);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void InitModel(ProtonAccountSettingsModel accountSettingsModel, bool isCreatingMode)
        {
            IsCreatingAccountMode = isCreatingMode;
            AccountSettingsModel = accountSettingsModel;
        }

        protected override bool IsValid()
        {
            AccountSettingsModel.Email.NeedsValidation = true;
            AccountSettingsModel.Password.NeedsValidation = true;
            AccountSettingsModel.TwoFactorCode.NeedsValidation = true;
            AccountSettingsModel.MailboxPassword.NeedsValidation = true;

            if ((!IsEmailReadonly && string.IsNullOrEmpty(AccountSettingsModel.Email.Value))
               || string.IsNullOrEmpty(AccountSettingsModel.Password.Value))
            {
                ValidateProperty(AccountSettingsModel, nameof(AccountSettingsModel));
                return false;
            }

            return true;
        }

        protected override Account AccountSettingsModelToAccount()
        {
            return AccountSettingsModel.ToAccount();
        }

        public static ValidationResult ClearValidationErrors(ProtonAccountSettingsModel accountModel, ValidationContext _)
        {
            if (accountModel != null)
            {
                accountModel.Email.Errors.Clear();
                accountModel.SynchronizationInterval.Errors.Clear();
                accountModel.Password.Errors.Clear();
                accountModel.TwoFactorCode.Errors.Clear();
                accountModel.MailboxPassword.Errors.Clear();
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidatePasswordIsNotEmpty(ProtonAccountSettingsModel accountModel, ValidationContext context)
        {
            if (context?.ObjectInstance is ProtonAccountSettingsPageViewModel viewModel)
            {
                if (accountModel != null &&
                    accountModel.Password.NeedsValidation &&
                    string.IsNullOrEmpty(accountModel.Password.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    accountModel.Password.Errors.Add(error);
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
                AccountSettingsModel.Password.Errors.Clear();
                AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (AuthenticationException)
            {
                AccountSettingsModel.Password.Errors.Clear();
                AccountSettingsModel.Password.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (ProtonSessionRequestException)
            {
                AccountSettingsModel.TwoFactorCode.Errors.Clear();
                AccountSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("AuthenticationError"));
            }
            catch (NeedAdditionalAuthInfo)
            {
                AccountSettingsModel.TwoFactorCode.Errors.Clear();
                AccountSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("AuthenticationError"));
            }

            throw new NeedAdditionalAuthInfo();
        }

        private async Task<Account> LoginAsync()
        {
            var (userId, refreshToken, saltedKeyPass) = await Proton.ClientAuth.LoginFullAsync
                (
                    AccountSettingsModel.Email.Value,
                    AccountSettingsModel.Password.Value,
                    async (ct) =>
                    {
                        await DispatcherService.RunAsync(() =>
                        {
                            if (string.IsNullOrEmpty(AccountSettingsModel.TwoFactorCode.Value))
                            {
                                AccountSettingsModel.TwoFactorCode.Errors.Clear();
                                AccountSettingsModel.TwoFactorCode.Errors.Add(GetLocalizedString("NeedTwofactorCode"));

                                throw new NeedAdditionalAuthInfo();
                            }
                        }).ConfigureAwait(true);
                        return AccountSettingsModel.TwoFactorCode.Value;
                    },
                    async (ct) =>
                    {
                        await DispatcherService.RunAsync(() =>
                        {
                            if (string.IsNullOrEmpty(AccountSettingsModel.MailboxPassword.Value))
                            {
                                AccountSettingsModel.MailboxPassword.Errors.Clear();
                                AccountSettingsModel.MailboxPassword.Errors.Add(GetLocalizedString("NeedMailboxPassword"));

                                throw new NeedAdditionalAuthInfo();
                            }
                        }).ConfigureAwait(true);
                        return AccountSettingsModel.MailboxPassword.Value;
                    },
                    default
                ).ConfigureAwait(true);

            var accountData = AccountSettingsModel.ToAccount();
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
                AccountSettingsModel.MailboxPassword.Errors.Clear();
                AccountSettingsModel.MailboxPassword.Errors.Add(GetLocalizedString("NeedMailboxPassword"));
            }
        }
    }
}
