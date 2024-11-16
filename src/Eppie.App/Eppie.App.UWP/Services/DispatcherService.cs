using System;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.UI.Core;

namespace Tuvi.App.Shared.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly CoreDispatcher _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

        public async Task RunAsync(Action action)
        {
            if (_dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
            }
        }
    }
}
