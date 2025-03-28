﻿using System;
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
            set
            {
                if (_isChanging != value)
                {
                    _isChanging = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanging)));
                }
            }
        }

        public IExtendedComparer<T>[] SortingVariants = Array.Empty<IExtendedComparer<T>>();

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
            }
        }

        public IFilter<T>[] FilterVariants = Array.Empty<IFilter<T>>();

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
                var resultItems = OriginalItems.Where(item => ItemPassedFilter(item));
                if (ItemsComparer != null)
                {
                    resultItems = resultItems.Distinct(ItemsComparer).OrderBy(item => item, ItemsComparer);
                }

                base.ClearItems();
                foreach (var item in resultItems)
                {
                    base.Add(item);
                }
            }
            finally
            {
                EndChanging();
            }
        }

        private bool ItemPassedFilter(T item)
        {
            return (ItemsFilter == null || ItemsFilter.ItemPassedFilter(item))
                && (SearchFilter == null || SearchFilter.ItemPassedFilter(item));
        }

        public void VerifyItemPassFilter(T item)
        {
            int itemIndex = IndexOf(item);
            if (itemIndex >= 0)
            {
                // Item is in filtered list. If it does not pass filter now, remove it from the list.
                if (!ItemPassedFilter(item))
                {
                    base.RemoveItem(itemIndex);
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

        public void VerifyItemsPassFilter(T[] items)
        {
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                VerifyItemPassFilter(item);
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
            Interlocked.Increment(ref _changingRequestsCount);
            IsChanging = true;
        }
        public void EndChanging()
        {
            if (Interlocked.Decrement(ref _changingRequestsCount) == 0)
            {
                IsChanging = false;
            }
        }
    }
}
