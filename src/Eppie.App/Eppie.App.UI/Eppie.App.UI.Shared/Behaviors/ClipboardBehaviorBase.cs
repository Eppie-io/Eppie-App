using Microsoft.Xaml.Interactivity;
using System;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.ApplicationModel.DataTransfer;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    // ToDo: remove it [Eppie-io/Eppie-App#561]
    public class ClipboardBehaviorBase : Behavior<Button>, IClipboardProvider
    {
        public async Task<string> GetClipboardContentAsync()
        {
            DataPackageView content = Clipboard.GetContent();
            if (content.Contains(StandardDataFormats.Text))
            {
                return await content.GetTextAsync();
            }

            return string.Empty;
        }

        public void SetClipboardContent(string text)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }
}
