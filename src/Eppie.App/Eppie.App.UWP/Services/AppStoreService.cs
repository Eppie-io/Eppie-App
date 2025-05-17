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
using Tuvi.App.ViewModels.Services;
using Windows.Services.Store;

namespace Tuvi.App.Shared.Services
{
    public class AppStoreService : IAppStoreService
    {
        public Task BuySubscriptionAsync()
        {
            var subscription = new SubscriptionProduct("<InAppOfferToken>", "<ProductId>");
            return subscription.PurchaseAsync();
        }

        public async Task<bool> RequestReviewAsync()
        {
            var context = StoreContext.GetDefault();
            var result = await context.RequestRateAndReviewAppAsync();

            return result.Status == StoreRateAndReviewStatus.Succeeded;
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
