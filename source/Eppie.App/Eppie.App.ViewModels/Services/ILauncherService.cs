using System;
using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public interface ILauncherService
    {
        Task<bool> LaunchAsync(Uri uri);
    }
}
