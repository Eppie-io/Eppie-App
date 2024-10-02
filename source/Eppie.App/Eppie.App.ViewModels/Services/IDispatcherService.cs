using System;
using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public interface IDispatcherService
    {
        Task RunAsync(Action action);
    }
}
