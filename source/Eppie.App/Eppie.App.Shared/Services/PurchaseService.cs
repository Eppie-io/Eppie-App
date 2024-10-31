using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.Services.Store;

namespace Tuvi.App.Shared.Services
{
    public class PurchaseService : IPurchaseService
    {
        public Task BuySubscriptionAsync()
        {
            var subscription = new SubscriptionProduct("<InAppOfferToken>", "<ProductId>");
            return subscription.PurchaseAsync();
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
