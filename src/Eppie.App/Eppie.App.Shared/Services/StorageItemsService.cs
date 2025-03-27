using System.Collections.Generic;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.Storage;

namespace Eppie.App.Shared.Services
{
    public class StorageItemsService : FileOperationProvider
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
