using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public interface IPurchaseService
    {
        Task BuySubscriptionAsync();
    }
}
