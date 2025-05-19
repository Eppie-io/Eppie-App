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

using Tuvi.App.ViewModels.Services;
using Windows.Services.Store;
using Windows.System;

namespace Tuvi.App.Shared.Services
{
    public class AppStoreService : IAppStoreService
    {
        public Task BuySubscriptionAsync()
        {
            const string InAppOfferToken = "<InAppOfferToken>";
            const string AppOfferProductId = "<AppOfferProductId>";

            var subscription = new SubscriptionProduct(InAppOfferToken, AppOfferProductId);

            return subscription.PurchaseAsync();
        }

        public async Task<bool> RequestReviewAsync()
        {
            try
            {
                var context = StoreContext.GetDefault();

#if WINDOWS10_0_19041_0_OR_GREATER
                //TODO: Need to pass the window handle to init StoreContext
                //nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                //WinRT.Interop.InitializeWithWindow.Initialize(context, hwnd);
#endif
                var result = await context.RequestRateAndReviewAppAsync();

                return result.Status == StoreRateAndReviewStatus.Succeeded;
            }
            catch
            {
                // Replace with actual product ID
                const string AppProductId = "<AppProductId>";

                var uri = new Uri($"ms-windows-store://review/?ProductId={AppProductId}");

                return await Launcher.LaunchUriAsync(uri);
            }
        }
    }

    public class SubscriptionProduct
    {
        private string InAppOfferToken { get; }
        private string ProductId { get; }

        public SubscriptionProduct(string token, string storeId)
        {
            InAppOfferToken = token;
            ProductId = storeId;
        }

        private StoreContext _storeContext = null;
        protected StoreContext GetStoreContext()
        {
            if (_storeContext == null)
            {
                _storeContext = StoreContext.GetDefault();
            }

            return _storeContext;
        }

        public async Task PurchaseAsync()
        {
            StoreContext context = StoreContext.GetDefault();

            const string productKind = "Durable";
            string[] productKinds = { productKind };

            List<String> filterList = new List<string>(productKinds);
            string[] storeIds = new string[] { ProductId };

            StoreProductQueryResult queryResult = await context.GetStoreProductsAsync(filterList, storeIds);

            StoreProduct subscription = queryResult.Products.FirstOrDefault().Value;

            if (subscription != null && !subscription.IsInUserCollection)
            {
                await subscription.RequestPurchaseAsync();
            }
        }
    }
}
