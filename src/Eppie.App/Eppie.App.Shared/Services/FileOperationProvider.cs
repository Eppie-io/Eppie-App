// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Eppie.App.UI.Tools;
using Tuvi.App.ViewModels.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else 
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.Shared.Services
{
    public class FileOperationProvider : IFileOperationProvider
    {
        public virtual Task<IEnumerable<AttachedFileInfo>> LoadFilesAsync()
        {
            return SelectAndLoadFilesDataAsync();
        }

        public async Task SaveToFileAsync(byte[] data, string fileName)
        {
            await SaveDataToFileAsync(fileName, data).ConfigureAwait(true);
        }

        public async Task SaveToTempFileAndOpenAsync(byte[] data, string fileName)
        {
            await SaveDataToTempFileAndOpenAsync(fileName, data).ConfigureAwait(true);
        }

        private static async Task SaveDataToFileAsync(string fileName, byte[] data)
        {
            FileSavePicker fileSavePicker = FileSavePickerBuilder.CreateBuilder(App.MainWindow)
                                                                 .Configure((picker) =>
                                                                 {
                                                                     picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

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
                                                                 })
                                                                 .Build();

            var file = await fileSavePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await fileStream.WriteAsync(data.AsBuffer());
                }
            }
        }

        private static async Task SaveDataToTempFileAndOpenAsync(string fileName, byte[] data)
        {
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var file = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await fileStream.WriteAsync(data.AsBuffer());
                }

                await Launcher.LaunchFileAsync(file); // ToDo: Uno0001
            }
        }

        private static async Task<IEnumerable<AttachedFileInfo>> SelectAndLoadFilesDataAsync()
        {
            var files = await SelectFiles().ConfigureAwait(true);

            var attachedFiles = await LoadFilesData(files).ConfigureAwait(true);

            return attachedFiles;
        }

        protected static async Task<IReadOnlyList<AttachedFileInfo>> LoadFilesData(IReadOnlyList<IStorageItem> files)
        {
            var attachedFiles = new List<AttachedFileInfo>();
            if (files.Count > 0)
            {
                foreach (StorageFile file in files)
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

        private static async Task<IReadOnlyList<StorageFile>> SelectFiles()
        {
            FileOpenPicker fileOpenPicker = FileOpenPickerBuilder.CreateBuilder(App.MainWindow)
                                             .Configure((picker) =>
                                             {
                                                 picker.ViewMode = PickerViewMode.Thumbnail;
                                                 picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                                                 picker.FileTypeFilter.Add("*");
                                             })
                                             .Build();

            return await fileOpenPicker.PickMultipleFilesAsync();
        }

        protected static async Task<AttachedFileInfo> ReadFileDataAsync(StorageFile file)
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
