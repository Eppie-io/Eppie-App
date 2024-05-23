using Microsoft.Xaml.Interactivity;
using System;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Behaviors
{
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
