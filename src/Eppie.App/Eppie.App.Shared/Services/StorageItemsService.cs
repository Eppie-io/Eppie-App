using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Tuvi.App.Shared.Behaviors;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Tuvi.App.Shared.Services
{
    public class StorageItemsService : FileBehavior
    {
        private static IReadOnlyList<IStorageItem> Items { get; set; }

        public StorageItemsService(IReadOnlyList<IStorageItem> items)
        {
            Items = items;
        }

        public override async Task<IEnumerable<AttachedFileInfo>> LoadFilesAsync()
        {
            return await LoadFilesData(Items).ConfigureAwait(true);
        }
    }
}
