using System;
using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public interface IMessageService
    {
        Task ShowErrorMessageAsync(Exception ex);
    }
}
