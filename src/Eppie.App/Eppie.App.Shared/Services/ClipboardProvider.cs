using System;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.ApplicationModel.DataTransfer;

namespace Eppie.App.Shared.Services
{
    public class ClipboardProvider : IClipboardProvider
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
        }
    }
}
