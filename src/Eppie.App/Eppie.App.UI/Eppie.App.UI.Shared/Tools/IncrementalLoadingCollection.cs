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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.App.ViewModels;
using Windows.Foundation;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else 
using Microsoft.UI.Xaml.Data;
#endif

namespace Tuvi.App.IncrementalLoading
{
    /// <summary>
    /// This class represents an <see cref="ObservableCollection{IType}"/> whose items can be loaded incrementally.
    /// </summary>
    /// <typeparam name="TSource">
    /// The data source that must be loaded incrementally.
    /// </typeparam>
    /// <typeparam name="TItemType">
    /// The type of collection items.
    /// </typeparam>
    /// <seealso cref="IIncrementalSource{TSource}"/>
    /// <seealso cref="ISupportIncrementalLoading"/>
    public class IncrementalLoadingCollection<TSource, TItemType> : ManagedCollection<TItemType>,
         ISupportIncrementalLoading
         where TSource : class, IIncrementalSource<TItemType>
    {
        /// <summary>
        /// Gets or sets an <see cref="Action"/> that is called when a retrieval operation begins.
        /// </summary>
        public Action OnStartLoading { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="Action"/> that is called when a retrieval operation ends.
        /// </summary>
        public Action OnEndLoading { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="Action"/> that is called if an error occours during data retrieval. The actual <see cref="Exception"/> is passed as an argument.
        /// </summary>
        public Action<Exception> OnError { get; set; }

        /// <summary>
        /// Gets a value indicating the source of incremental loading.
        /// </summary>
        protected WeakReference<TSource> Source { get; }

        private bool _isLoading;
        private bool _hasMoreItems;
        private CancellationToken _cancellationToken;
        private bool _refreshOnLoad;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets a value indicating whether new items are being loaded.
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            private set
            {
                if (value != _isLoading)
                {

                    _isLoading = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsLoading)));

                    if (_isLoading)
                    {
                        StartChanging();
                        OnStartLoading?.Invoke();
                    }
                    else
                    {
                        EndChanging();
                        OnEndLoading?.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains more items to retrieve.
        /// </summary>
        public bool HasMoreItems
        {
            get
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                return _hasMoreItems;
            }

            private set
            {
                if (value != _hasMoreItems)
                {
                    _hasMoreItems = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasMoreItems)));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalLoadingCollection{TSource, IType}"/> class optionally specifying how many items to load for each data page.
        /// </summary>
        /// <param name="itemsComparer">
        /// Comparer is needed to sort collection. If null, no sorting will be applied.
        /// </param>
        /// <param name="filterVariants">
        /// Array of filters that can be applied to items.
        /// </param>
        /// <param name="itemsFilter">
        /// Filter is needed to select only items that passed filter. If null, all items will be shown.
        /// </param>
        /// <param name="onStartLoading">
        /// An <see cref="Action"/> that is called when a retrieval operation begins.
        /// </param>
        /// <param name="onEndLoading">
        /// An <see cref="Action"/> that is called when a retrieval operation ends.
        /// </param>
        /// <param name="onError">
        /// An <see cref="Action"/> that is called if an error occours during data retrieval.
        /// </param>
        /// <seealso cref="IIncrementalSource{TSource}"/>
        public IncrementalLoadingCollection(CancellationTokenSource cts,
                                            IExtendedComparer<TItemType> itemsComparer = null,
                                            IFilter<TItemType>[] filterVariants = null,
                                            IFilter<TItemType> itemsFilter = null,
                                            Action onStartLoading = null,
                                            Action onEndLoading = null,
                                            Action<Exception> onError = null)
            : this(Activator.CreateInstance<TSource>(), cts, itemsComparer, filterVariants, itemsFilter, onStartLoading, onEndLoading, onError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalLoadingCollection{TSource, IType}"/> class using the specified <see cref="IIncrementalSource{TSource}"/> implementation and, optionally, how many items to load for each data page.
        /// </summary>
        /// <param name="source">
        /// An implementation of the <see cref="IIncrementalSource{TSource}"/> interface that contains the logic to actually load data incrementally.
        /// </param>
        /// <param name="itemsComparer">
        /// Comparer is needed to sort collection. If null, no sorting will be applied.
        /// </param>
        /// <param name="filterVariants">
        /// Array of filters that can be applied to items.
        /// </param>
        /// <param name="itemsFilter">
        /// Filter is needed to select only items that passed filter. If null, all items will be shown.
        /// </param>
        /// <param name="onStartLoading">
        /// An <see cref="Action"/> that is called when a retrieval operation begins.
        /// </param>
        /// <param name="onEndLoading">
        /// An <see cref="Action"/> that is called when a retrieval operation ends.
        /// </param>
        /// <param name="onError">
        /// An <see cref="Action"/> that is called if an error occours during data retrieval.
        /// </param>
        /// <seealso cref="IIncrementalSource{TSource}"/>
        public IncrementalLoadingCollection(TSource source,
                                            CancellationTokenSource cts,
                                            IExtendedComparer<TItemType> itemsComparer = null,
                                            IFilter<TItemType>[] filterVariants = null,
                                            IFilter<TItemType> itemsFilter = null,
                                            Action onStartLoading = null,
                                            Action onEndLoading = null,
                                            Action<Exception> onError = null,
                                            ISearchFilter<TItemType> searchFilter = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Source = new WeakReference<TSource>(source);
            _cancellationTokenSource = cts;

            ItemsComparer = itemsComparer;

            FilterVariants = filterVariants;
            ItemsFilter = itemsFilter;
            SearchFilter = searchFilter;

            OnStartLoading = onStartLoading;
            OnEndLoading = onEndLoading;
            OnError = onError;

            _hasMoreItems = true;
        }

        /// <summary>
        /// Initializes incremental loading from the view.
        /// </summary>
        /// <param name="count">
        /// The number of items to load.
        /// </param>
        /// <returns>
        /// An object of the <see cref="LoadMoreItemsAsync(uint)"/> that specifies how many items have been actually retrieved.
        /// </returns>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var token = _cancellationTokenSource.Token;
            return LoadMoreItemsAsync(count, token).AsAsyncOperation();
        }

        /// <summary>
        /// Clears the collection and triggers/forces a reload of the first page
        /// </summary>
        /// <returns>This method does not return a result</returns>
        protected override Task RefreshImplAsync()
        {
            if (IsLoading)
            {
                _refreshOnLoad = true;
            }
            else
            {
                var previousCount = Count;
                Clear();
                HasMoreItems = true;
                GetSource()?.Reset();
                if (previousCount == 0)
                {
                    // When the list was empty before clearing, the automatic reload isn't fired, so force a reload.
                    return LoadMoreItemsAsync(0).AsTask();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Actually performs the incremental loading.
        /// </summary>
        /// <param name="cancellationToken">
        /// Used to propagate notification that operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns a collection of <typeparamref name="TItemType"/>.
        /// </returns>
        protected virtual async Task<IEnumerable<TItemType>> LoadDataAsync(int count, CancellationToken cancellationToken)
        {
            var source = GetSource();
            if (source is null)
            {
                return new List<TItemType>();
            }
            return await source.LoadMoreItemsAsync(count, cancellationToken).ConfigureAwait(true);
        }

        private TSource GetSource()
        {
            if (Source.TryGetTarget(out TSource target))
            {
                return target;
            }
            return null;
        }

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count, CancellationToken cancellationToken)
        {
            uint resultCount = 0;
            _cancellationToken = cancellationToken;

            try
            {
                if (!_cancellationToken.IsCancellationRequested)
                {
                    IEnumerable<TItemType> data = null;
                    try
                    {
                        IsLoading = true;
                        data = await LoadDataAsync((int)count, _cancellationToken).ConfigureAwait(true);
                    }
                    catch (OperationCanceledException)
                    {
                        // The operation has been canceled using the Cancellation Token.
                    }
                    catch (ObjectDisposedException)
                    {
                        // It seems like we've recreated core
                    }
                    catch (Exception ex) when (OnError != null)
                    {
                        OnError.Invoke(ex);
                    }

                    if (data != null && data.Any() && data.First() != null && !_cancellationToken.IsCancellationRequested)
                    {
                        resultCount = (uint)data.Count();
                        AddRange(data);
                        HasMoreItems = true;

                        // Waiting for new items to be processed before the next iteration. This should apply to Desktop and WebAssembly projects.
                        // Look at https://github.com/unoplatform/uno/issues/19887
                        await Task.Yield();
                    }
                    else
                    {
                        HasMoreItems = false;
                    }
                }
            }
            finally
            {
                IsLoading = false;

                if (_refreshOnLoad)
                {
                    _refreshOnLoad = false;
                    await RefreshAsync().ConfigureAwait(true);
                }
            }

            return new LoadMoreItemsResult { Count = resultCount };
        }
    }
}
