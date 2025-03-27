using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tuvi.App.ViewModels.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static void SetItems<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (collection != null)
            {
                collection.Clear();
                foreach (var item in items)
                {
                    collection.Add(item);
                }
            }
        }
    }
}
