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
using CommunityToolkit.Mvvm.ComponentModel;
using Eppie.App.ViewModels.Services;

namespace Eppie.App.ViewModels.Models
{
    public class SupportDevelopmentModel : ObservableObject
    {
        private bool _isStorePaymentProcessor = true;
        public bool IsStorePaymentProcessor
        {
            get => _isStorePaymentProcessor;
            private set
            {
                _isStorePaymentProcessor = value;
                OnPropertyChanged(nameof(IsStorePaymentProcessor));
            }
        }

        private string _price;
        public string Price
        {
            get => _price;
            private set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            private set
            {
                _isAvailable = value;
                OnPropertyChanged(nameof(IsAvailable));
            }
        }

        private IAppStoreService AppStoreService { get; set; }
        private ILauncherService LauncherService { get; set; }
        private IBrandService BrandService { get; set; }
        private IErrorHandler ErrorHandler { get; set; }

        internal SupportDevelopmentModel(IAppStoreService appStoreService, ILauncherService launcherService, IBrandService brandService, IErrorHandler errorHandler)
        {
            AppStoreService = appStoreService;
            LauncherService = launcherService;
            BrandService = brandService;
            ErrorHandler = errorHandler;
        }

        internal async Task SupportDevelopmentAsync()
        {
            try
            {
                await AppStoreService.BuySubscriptionAsync().ConfigureAwait(true);
                Update();
            }
            catch
            {
                await LauncherService.LaunchAsync(new Uri(BrandService.GetDevelopmentSupport())).ConfigureAwait(true);
            }
        }

        internal async void Update()
        {
            try
            {
                await UpdateAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                ErrorHandler?.OnError(e, false);
            }
        }

        internal async Task UpdateAsync()
        {
            if (string.IsNullOrWhiteSpace(BrandService.GetDevelopmentSupport()))
            {
                IsAvailable = false;
                return;
            }

            try
            {
                IsAvailable = !await AppStoreService.IsSubscriptionEnabledAsync().ConfigureAwait(true);

                if (IsAvailable)
                {
                    Price = await AppStoreService.GetSubscriptionPriceAsync().ConfigureAwait(true);
                }
            }
            catch (NotImplementedException)
            {
                IsAvailable = true;
                IsStorePaymentProcessor = false;
                Price = "$3";
            }
        }
    }
}
