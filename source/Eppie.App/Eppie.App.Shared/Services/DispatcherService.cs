using System;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.UI.Core;

namespace Tuvi.App.Shared.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly CoreDispatcher _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        public CoreDispatcher Dispatcher => _dispatcher;

        public DispatcherService()
        {
        }

        public async Task RunAsync(Action action)
        {
            if (Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
            }
        }
    }
}
