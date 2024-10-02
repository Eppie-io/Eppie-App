using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace Tuvi.App.Services
{
    internal class FilePickerService
    {
        public static async Task SaveDataToFileAsync(string fileName, byte[] data)
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                picker.FileTypeChoices.Add("", new List<string>() { "." });
            }
            else
            {
                picker.FileTypeChoices.Add(extension, new List<string>() { extension });
            }

            picker.SuggestedFileName = fileName;

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await fileStream.WriteAsync(data.AsBuffer());
                }
            }
        }

        public static async Task SaveDataToTempFileAndOpenAsync(string fileName, byte[] data)
        {
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var file = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await fileStream.WriteAsync(data.AsBuffer());                    
                }

                await Launcher.LaunchFileAsync(file);
            }
        }
        
        public static async Task<IEnumerable<AttachedFileInfo>> SelectAndLoadFilesDataAsync()
        {
            var attachedFiles = new List<AttachedFileInfo>();

            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");

            var files = await picker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    var attachedFile = await ReadFileDataAsync(file).ConfigureAwait(true);
                    if (attachedFile != null)
                    {
                        attachedFiles.Add(attachedFile);
                    }
                }
            }

            return attachedFiles;
        }

        private static async Task<AttachedFileInfo> ReadFileDataAsync(StorageFile file)
        {
            using (var fileStream = await file.OpenReadAsync())
            {
                if (fileStream != null && fileStream.CanRead)
                {
                    var buffer = new Windows.Storage.Streams.Buffer((uint)fileStream.Size);
                    await fileStream.ReadAsync(buffer, (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                    return new AttachedFileInfo { Path = file.Path, Name = file.Name, Data = buffer.ToArray() };
                }

                return null;
            }
        }
    }
}
