using Microsoft.UI.Dispatching;
using Tuvi.App.ViewModels.Services;

namespace Tuvi.App.Shared.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public async Task RunAsync(Action action)
        {
            if (_dispatcherQueue.HasThreadAccess)
            {
                action();
            }
            else
            {
                var completionSource = new TaskCompletionSource<bool>();
                _dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        action();
                        completionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                });
                await completionSource.Task;
            }
        }
    }
}
