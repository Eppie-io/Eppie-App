using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eppie.AI;
using Eppie.App.ViewModels.Services;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Eppie.App.Shared.Services
{
    public class AIService : IAIService
    {
        private const string _LocalAIModelFolderName = "local.ai.model";
        private Service _service;

        public Task<string> TranslateTextAsync(string text, string language, CancellationToken cancellationToken, Action<string> onTextUpdate = null)
        {
            if (_service != null)
            {
                return _service.TranslateTextAsync(text, language, cancellationToken, onTextUpdate);
            }

            return Task.FromResult(string.Empty);
        }

        public async Task<bool> IsEnabledAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var modelFolder = await localFolder.TryGetItemAsync(_LocalAIModelFolderName);

            return modelFolder != null;
        }

        public async Task LoadModelIfEnabled()
        {
            if (_service == null && await IsEnabledAsync())
            {
                _service = new Service();

                var localFolder = ApplicationData.Current.LocalFolder;
                var modelFolder = await localFolder.GetFolderAsync(_LocalAIModelFolderName);
                var modelPath = modelFolder.Path;

                await _service.LoadModelAsync(modelPath);
            }
        }

        public async Task DeleteModelAsync()
        {
            _service?.UnloadModel();
            _service = null;

            var localFolder = ApplicationData.Current.LocalFolder;
            var modelFolder = await localFolder.GetFolderAsync(_LocalAIModelFolderName);
            await modelFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public async Task ImportModelAsync()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder selectedFolder = await folderPicker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder modelFolder = await localFolder.CreateFolderAsync(_LocalAIModelFolderName, CreationCollisionOption.ReplaceExisting);

                await CopyFolderContentsAsync(selectedFolder, modelFolder);

                await LoadModelIfEnabled();
            }
        }

        private async Task CopyFolderContentsAsync(StorageFolder sourceFolder, StorageFolder destinationFolder)
        {
            IReadOnlyList<StorageFile> files = await sourceFolder.GetFilesAsync();
            var config = await sourceFolder.TryGetItemAsync("genai_config.json");

            if (files.Count > 0 && config != null)
            {
                foreach (StorageFile file in files)
                {
                    await file.CopyAsync(destinationFolder, file.Name, NameCollisionOption.ReplaceExisting);
                }
            }
            else
            {
                await destinationFolder.DeleteAsync();

                throw new Exception("No AI model files found in the selected folder.");
            }
        }
    }
}
