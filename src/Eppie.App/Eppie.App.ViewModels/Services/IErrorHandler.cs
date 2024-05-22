using System;

namespace Tuvi.App.ViewModels.Services
{
    public interface IErrorHandler
    {
        void SetMessageService(IMessageService messageService);

        void OnError(Exception ex, bool silent = false);
    }
}
