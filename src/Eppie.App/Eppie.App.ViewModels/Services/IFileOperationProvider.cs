using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public class AttachedFileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }

    public interface IFileOperationProvider
    {
        /// <summary>
        /// Get data from files which user choose.
        /// </summary>
        /// <returns>Files content.</returns>
        Task<IEnumerable<AttachedFileInfo>> LoadFilesAsync();

        /// <summary>
        /// Store <paramref name="data"/> to file user choose.
        /// </summary>
        /// <param name="fileName">Name for file. User can change it.</param>
        Task SaveToFileAsync(byte[] data, string fileName);

        /// <summary>
        /// Store <paramref name="data"/> to temporary file and open it.
        /// </summary>
        /// <param name="fileName">Name for temporary file.</param>
        Task SaveToTempFileAndOpenAsync(byte[] data, string fileName);
    }
}
