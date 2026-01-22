// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tuvi.App.ViewModels
{
    public class ManagedCollection<T> : ObservableCollection<T>
    {
        public List<T> OriginalItems { get; } = new List<T>();

        private bool _isChanging;
        public bool IsChanging
        {
            get => _isChanging;
            private set
            {
                if (_isChanging != value)
                {
                    _isChanging = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanging)));
                }
            }
        }

        public IExtendedComparer<T>[] SortingVariants { get; set; } = Array.Empty<IExtendedComparer<T>>();

        public int SelectedSortingIndex
        {
            get { return SortingVariants is null ? -1 : Array.IndexOf(SortingVariants, ItemsComparer); }
            set
            {
                if (value >= 0 && value < SortingVariants.Length)
                {
                    var comparer = SortingVariants[value];
                    if (!object.ReferenceEquals(ItemsComparer, comparer))
                    {
                        ItemsComparer = comparer;
                    }
                }
            }
        }

        private IExtendedComparer<T> _itemsComparer;
        public IExtendedComparer<T> ItemsComparer
        {
            get { return _itemsComparer; }
            set
            {
                if (_itemsComparer == value)
                {
                    return;
                }
                _itemsComparer = value;
                ReFilterItems();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemsComparer)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedSortingIndex)));
            }
        }

        public IFilter<T>[] FilterVariants { get; set; } = Array.Empty<IFilter<T>>();

        private IFilter<T> _itemsFilter;
        public IFilter<T> ItemsFilter
        {
            get { return _itemsFilter; }
            set
            {
                if (_itemsFilter == value)
                {
                    return;
                }
                _itemsFilter = value;
                ReFilterItems();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemsFilter)));
            }
        }

        private ISearchFilter<T> _searchFilter;
        public ISearchFilter<T> SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                if (_searchFilter == value)
                {
                    return;
                }

                if (_searchFilter != null)
                {
                    _searchFilter.PropertyChanged -= OnSearchFilterPropertyChanged;
                }

                _searchFilter = value;

                if (_searchFilter != null)
                {
                    _searchFilter.PropertyChanged += OnSearchFilterPropertyChanged;
                }

                ReFilterItems();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SearchFilter)));
            }
        }
        private void OnSearchFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReFilterItems();
        }

        private void ReFilterItems()
        {
            StartChanging();

            try
            {
                SyncVisualCollection();
            }
            finally
            {
                EndChanging();
            }
        }

        private bool ItemPassedFilter(T item)
        {
            return (ItemsFilter is null || ItemsFilter.ItemPassedFilter(item))
                && (SearchFilter is null || SearchFilter.ItemPassedFilter(item));
        }

        public void RefilterItem(T item)
        {
            if (!OriginalItems.Contains(item))
            {
                int existingIndex = IndexOf(item);
                if (existingIndex >= 0)
                {
                    base.RemoveItem(existingIndex);
                }
                return;
            }

            int itemIndex = IndexOf(item);
            if (itemIndex >= 0)
            {
                // Item is in filtered list. If it does not pass filter now, remove it from the list.
                if (!ItemPassedFilter(item))
                {
                    base.RemoveItem(itemIndex);
                }
                else if (ItemsComparer != null)
                {
                    bool isOrderCorrect = true;
                    if (itemIndex > 0 && ItemsComparer.Compare(Items[itemIndex - 1], item) > 0)
                    {
                        isOrderCorrect = false;
                    }

                    if (isOrderCorrect && itemIndex < Count - 1 && ItemsComparer.Compare(item, Items[itemIndex + 1]) > 0)
                    {
                        isOrderCorrect = false;
                    }

                    if (!isOrderCorrect)
                    {
                        base.RemoveItem(itemIndex);
                        AddItemWithSorting(item);
                    }
                }
            }
            else
            {
                // Item is not in filtered list. If it passes filter now, add it to the list.
                if (ItemPassedFilter(item))
                {
                    AddItemWithSorting(item);
                }
            }
        }

        public void RefilterItems(T[] items)
        {
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                RefilterItem(item);
            }
        }

        public new void Add(T item)
        {
            StartChanging();

            try
            {
                OriginalItems.Add(item);
                if (ItemPassedFilter(item))
                {
                    AddItemWithSorting(item);
                }
            }
            finally
            {
                EndChanging();
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            StartChanging();

            try
            {
                if (items is null)
                {
                    return;
                }
                foreach (var item in items)
                {
                    Add(item);
                }
            }
            finally
            {
                EndChanging();
            }
        }

        protected override void ClearItems()
        {
            StartChanging();

            try
            {
                OriginalItems.Clear();
                base.ClearItems();
            }
            finally
            {
                EndChanging();
            }
        }

        protected override void RemoveItem(int index)
        {
            StartChanging();

            try
            {
                var item = Items[index];
                OriginalItems.Remove(item);
                base.RemoveItem(index);
            }
            finally
            {
                EndChanging();
            }
        }

        /// <summary>
        /// Adds new item if it is not in collection and apply sorting to item if ItemsComparer of collection is not null
        /// </summary>
        /// <param name="item"></param>
        private void AddItemWithSorting(T item)
        {
            StartChanging();

            try
            {
                if (ItemsComparer != null)
                {
                    int index = BinarySearch(Items, item, ItemsComparer);
                    if (index < 0)
                    {
                        base.InsertItem(~index, item);
                    }
                }
                else
                {
                    base.Add(item);
                }
            }
            finally
            {
                EndChanging();
            }
        }

        private static int BinarySearch(IList<T> collection, T item, IComparer<T> comparer)
        {
            return BinarySearch(collection, item, 0, collection.Count - 1, comparer);
        }

        private static int BinarySearch(IList<T> collection, T item, int lowerIndex, int upperIndex, IComparer<T> comparer)
        {
            comparer = comparer ?? Comparer<T>.Default;

            while (lowerIndex <= upperIndex)
            {
                int middleIndex = lowerIndex + (upperIndex - lowerIndex) / 2;
                int comparisonResult = comparer.Compare(item, collection[middleIndex]);
                if (comparisonResult == 0)
                    return middleIndex;
                else if (comparisonResult < 0)
                    upperIndex = middleIndex - 1;
                else
                    lowerIndex = middleIndex + 1;
            }

            return ~lowerIndex;
        }

        public async Task RefreshAsync()
        {
            StartChanging();

            try
            {
                ReFilterItems();
                await RefreshImplAsync().ConfigureAwait(true);
            }
            finally
            {
                EndChanging();
            }
        }

        protected virtual Task RefreshImplAsync()
        {
            return Task.CompletedTask;
        }

        private int _changingRequestsCount;
        public void StartChanging()
        {
            if (_changingRequestsCount == 0)
            {
                IsChanging = true;
            }
            _changingRequestsCount++;
        }

        public void EndChanging()
        {
            if (_changingRequestsCount <= 0)
            {
                throw new InvalidOperationException("EndChanging called without matching StartChanging.");
            }

            _changingRequestsCount--;
            if (_changingRequestsCount == 0)
            {
                IsChanging = false;
            }
        }

        /// <summary>
        /// Reconciles the OriginalItems list with a new trusted list from the source.
        /// Performs minimal Add/Remove/Move/Replace operations to make OriginalItems identical to newItems.
        /// </summary>
        public void ReconcileOriginalItems(IEnumerable<T> newItemsEnumerable, Action<T, T> updateItem = null)
        {
            if (newItemsEnumerable is null) return;

            var newItems = newItemsEnumerable as IList<T> ?? newItemsEnumerable.ToList();

            StartChanging();
            try
            {
                // 1. Map existing items for fast lookup (handling duplicates)
                var originalMap = new Dictionary<T, Queue<T>>(OriginalItems.Count);
                var nullQueue = new Queue<T>(); // Separate queue for null items

                foreach (var item in OriginalItems)
                {
                    if (item == null)
                    {
                        nullQueue.Enqueue(item);
                        continue;
                    }

                    if (!originalMap.TryGetValue(item, out var queue))
                    {
                        queue = new Queue<T>();
                        originalMap[item] = queue;
                    }
                    queue.Enqueue(item);
                }

                // 2. Construct the new state of OriginalItems
                var newOriginalItems = new List<T>(newItems.Count);
                foreach (var newItem in newItems)
                {
                    if (newItem == null)
                    {
                        if (nullQueue.Count > 0)
                        {
                            var existing = nullQueue.Dequeue();
                            updateItem?.Invoke(existing, newItem);
                            newOriginalItems.Add(existing);
                        }
                        else
                        {
                            newOriginalItems.Add(newItem);
                        }
                        continue;
                    }

                    if (originalMap.TryGetValue(newItem, out var queue) && queue.Count > 0)
                    {
                        // Found matching existing item: preserve it
                        var existingItem = queue.Dequeue();

                        // Invoke update action (even if keys match, mutable properties might need sync)
                        updateItem?.Invoke(existingItem, newItem);

                        newOriginalItems.Add(existingItem);
                    }
                    else
                    {
                        // New item
                        newOriginalItems.Add(newItem);
                    }
                }

                // 3. Update OriginalItems
                // Note: We don't need to manually remove items from 'base' (visual collection) here.
                // The SyncVisualCollection method below will detect that items are missing
                // from OriginalItems and remove them from the visual collection.
                OriginalItems.Clear();
                OriginalItems.AddRange(newOriginalItems);

                // 4. Sync Visual Collection
                SyncVisualCollection();
            }
            finally
            {
                EndChanging();
            }
        }

        private void SyncVisualCollection()
        {
            var resultItems = OriginalItems.Where(item => ItemPassedFilter(item));
            if (ItemsComparer != null)
            {
                resultItems = resultItems.OrderBy(item => item, ItemsComparer);
            }
            var targetList = resultItems.ToList();
            var targetSet = new HashSet<T>(targetList);

            StartChanging();
            try
            {
                // 1) Remove items that should no longer be visible
                for (int i = Count - 1; i >= 0; i--)
                {
                    var current = this[i];
                    if (!targetSet.Contains(current))
                    {
                        base.RemoveItem(i);
                    }
                }

                // 2) Align visual collection to target list
                for (int i = 0; i < targetList.Count; i++)
                {
                    var targetItem = targetList[i];

                    if (i >= Count)
                    {
                        base.InsertItem(i, targetItem);
                        continue;
                    }

                    var currentItem = this[i];

                    if (object.ReferenceEquals(currentItem, targetItem))
                    {
                        continue;
                    }

                    if (EqualityComparer<T>.Default.Equals(currentItem, targetItem))
                    {
                        this[i] = targetItem;
                        continue;
                    }

                    // HEURISTIC: Check if the NEXT item is the one we want.
                    // This handles the case where 'currentItem' is an intruder that needs to be moved/removed.
                    // If we remove 'currentItem', the 'targetItem' (at i+1) shifts to 'i'.
                    if (this.Count > i + 1 && EqualityComparer<T>.Default.Equals(this[i + 1], targetItem))
                    {
                        // Remove the intruder. It will be re-inserted later if it belongs elsewhere.
                        base.RemoveItem(i);
                        // Re-evaluate position 'i' (which now holds what was at 'i+1')
                        i--;
                        continue;
                    }

                    // OPTIMIZATION: Instead of searching for the item (O(N)) and moving it,
                    // we simply insert it here.
                    // If the item exists later in the list, it's now a duplicate (shadowed).
                    // As we iterate, that "old" copy will either be matched (if valid) or
                    // pushed to the end and removed in Step 3.
                    base.InsertItem(i, targetItem);
                }

                // 3) Remove excess items
                while (Count > targetList.Count)
                {
                    base.RemoveItem(Count - 1);
                }
            }
            finally
            {
                EndChanging();
            }
        }
    }
}
