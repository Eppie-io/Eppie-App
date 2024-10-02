using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tuvi.Core.Logging;
using Tuvi.App.ViewModels.Services;
using Windows.UI.Core;

namespace Tuvi.App.Shared.Services
{
    public class ErrorHandler : IErrorHandler
    {
        private IMessageService MessageService { get; set; }

        private readonly CoreDispatcher _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        protected CoreDispatcher Dispatcher { get => _dispatcher; }

        public ErrorHandler()
        {
        }

        public void SetMessageService(IMessageService messageService)
        {
            MessageService = messageService;
        }


        public async void OnError(Exception e, bool silent)
        {
            this.Log().LogError(e, "");
            try
            {
                if (Dispatcher.HasThreadAccess)
                {
                    await OnErrorAsync(e, silent).ConfigureAwait(true);
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            await OnErrorAsync(e, silent).ConfigureAwait(true);
                        }
                        catch
                        {
                        }
                    });
                }
            }
            catch
            {
            }
        }

        private async Task OnErrorAsync(Exception e, bool silent)
        {
            if (e is OperationCanceledException)
            {
                return;
            }

            if (!silent && MessageService != null)
            {
                await MessageService.ShowErrorMessageAsync(e).ConfigureAwait(true);
            }
        }
    }
}
