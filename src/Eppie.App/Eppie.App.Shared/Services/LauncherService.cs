using System;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.System;

namespace Tuvi.App.Shared.Services
{
    public class LauncherService : ILauncherService
    {
        public Task<bool> LaunchAsync(Uri uri)
        {
            return Launcher.LaunchUriAsync(uri).AsTask();
        }
    }
}
