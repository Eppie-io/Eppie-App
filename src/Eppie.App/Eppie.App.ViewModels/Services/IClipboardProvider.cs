using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public interface IClipboardProvider
    {
        /// <summary>
        /// Get text content of clipboard.
        /// </summary>
        /// <returns>Empty string if clipboard doesn't contain any text.</returns>
        Task<string> GetClipboardContentAsync();

        /// <summary>
        /// Push text to clipboard.
        /// </summary>
        void SetClipboardContent(string text);
    }
}
