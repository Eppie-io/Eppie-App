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
        private string InAppOfferToken { get; set; }
        private string ProductId { get; set; }

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

        public async Task<bool> PurchaseAsync()
        {
            StoreContext context = StoreContext.GetDefault();

            var id = ProductId;

            string[] productKinds = { "Durable" };
            List<String> filterList = new List<string>(productKinds);
            string[] storeIds = new string[] { id };

            StoreProductQueryResult queryResult = await context.GetStoreProductsAsync(filterList, storeIds);

            StoreProduct subscription = queryResult.Products.FirstOrDefault().Value;

            if (subscription != null && !subscription.IsInUserCollection)
            {
                StorePurchaseResult result = await subscription.RequestPurchaseAsync();

                string extendedError = string.Empty;
                if (result.ExtendedError != null)
                {
                    extendedError = result.ExtendedError.Message;
                }

                switch (result.Status)
                {
                    case StorePurchaseStatus.Succeeded:
                        return true;

                    case StorePurchaseStatus.NotPurchased:
                        return false;


                    default:
                        return false;
                }
            }

            return false;
        }
    }
}
