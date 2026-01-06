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
using System.ComponentModel.DataAnnotations;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ProtonAddressSettingsPageViewModel : BaseAddressSettingsPageViewModel
    {
        private bool _isAdvancedSettingsModeActive;
        public bool IsAdvancedSettingsModeActive
        {
            get => _isAdvancedSettingsModeActive;
            set => SetProperty(ref _isAdvancedSettingsModeActive, value);
        }

        private ProtonAddressSettingsModel _addressSettingsModel;
        [CustomValidation(typeof(ProtonAddressSettingsPageViewModel), nameof(ClearValidationErrors))]
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

        protected override BaseAddressSettingsModel AddressSettingsModelBase => AddressSettingsModel;

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

            if (!IsEmailReadonly && string.IsNullOrEmpty(AddressSettingsModel.Email.Value))
            {
                ValidateProperty(AddressSettingsModel, nameof(AddressSettingsModel));
                return false;
            }

            return true;
        }

        protected override Account ApplySettingsToAccount()
        {
            return AddressSettingsModel.ToAccount();
        }

        public static ValidationResult ClearValidationErrors(ProtonAddressSettingsModel addressSettingsModel, ValidationContext _)
        {
            if (addressSettingsModel != null)
            {
                addressSettingsModel.Email.Errors.Clear();
                addressSettingsModel.SynchronizationInterval.Errors.Clear();
            }

            return ValidationResult.Success;
        }
    }
}
