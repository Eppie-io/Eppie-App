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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eppie.App.ViewModels.Services;
using Windows.Services.Store;
using Windows.System;

namespace Eppie.App.Shared.Services
{
    public class BaseAppStoreService : IAppStoreService
    {
        protected virtual SubscriptionProduct GetSubscriptionProduct()
        {
            return null;
        }

        public Task BuySubscriptionAsync()
        {
            var context = GetStoreContext();
            return GetSubscriptionProduct().PurchaseAsync(context);
        }

        public Task<bool> IsSubscriptionEnabledAsync()
        {
            var context = GetStoreContext();
            return GetSubscriptionProduct().IsEnabled(context);
        }

        public Task<string> GetSubscriptionPriceAsync()
        {
            var context = GetStoreContext();
            return GetSubscriptionProduct().GetPriceAsync(context);
        }

        public async Task<bool> RequestReviewAsync()
        {
            var context = GetStoreContext();
            if (context is null)
            {
                throw new NotImplementedException("Rate and review is not implemented for this platform.");
            }

            var result = await context.RequestRateAndReviewAppAsync();

            return result.Status == StoreRateAndReviewStatus.Succeeded;
        }

        private StoreContext _StoreContext = null;
        protected StoreContext GetStoreContext()
        {
            if (_StoreContext is null)
            {

#if WINDOWS10_0_19041_0_OR_GREATER || WINDOWS_UWP
                _StoreContext = StoreContext.GetDefault();
#endif

#if WINDOWS10_0_19041_0_OR_GREATER
                var window = Eppie.App.Shared.App.MainWindow;
                nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(_StoreContext, hwnd);
#endif
            }

            return _StoreContext;
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

        public async Task PurchaseAsync(StoreContext context)
        {
            StoreProduct product = await GetProductAsync(context, ProductId);

            if (product != null && !product.IsInUserCollection)
            {
                await product.RequestPurchaseAsync();
            }
        }

        private string _price;
        public virtual async Task<string> GetPriceAsync(StoreContext context)
        {
            if (_price is null)
            {
                StoreProduct product = await GetProductAsync(context, ProductId);

                if (product != null)
                {
                    _price = product.Price.FormattedPrice;
                }
                else
                {
                    _price = "";
                }
            }

            return _price ?? string.Empty;
        }

        private static async Task<StoreProduct> GetProductAsync(StoreContext context, string id)
        {
            if (context is null)
            {
                throw new NotImplementedException("Purchase is not implemented for this platform.");
            }

            const string DurableProductKind = "Durable";
            string[] productKinds = { DurableProductKind };

            List<String> filterList = new List<string>(productKinds);
            string[] storeIds = new string[] { id };

            StoreProductQueryResult queryResult = await context.GetStoreProductsAsync(filterList, storeIds);

            StoreProduct product = queryResult.Products.FirstOrDefault().Value;

            if (product is null)
            {
                throw new NotImplementedException("Purchase is not implemented for this platform.");
            }

            return product;
        }

        internal async Task<bool> IsEnabled(StoreContext context)
        {
            StoreProduct product = await GetProductAsync(context, ProductId);

            return product != null && product.IsInUserCollection;
        }
    }
}
