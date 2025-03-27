using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Logging;

namespace Tuvi.App.Shared.Services
{
    public class ErrorHandler : IErrorHandler
    {
        private IMessageService MessageService { get; set; }

        private readonly DispatcherService _dispatcher = new DispatcherService();
        protected DispatcherService Dispatcher { get => _dispatcher; }

        public ErrorHandler()
        {
        }

        public void SetMessageService(IMessageService messageService)
        {
            MessageService = messageService;
        }


        public async void OnError(Exception e, bool silent)
        {
            LoggingExtension.Log(this).LogError(e, "");
            try
            {
                await Dispatcher.RunAsync(async () =>
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
