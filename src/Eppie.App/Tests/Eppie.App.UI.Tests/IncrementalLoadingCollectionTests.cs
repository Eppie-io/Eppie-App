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

using NUnit.Framework;
using Tuvi.App.IncrementalLoading;
using Tuvi.App.ViewModels;
using System.ComponentModel;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

namespace Eppie.App.UI.Tests.IncrementalLoading
{
    [TestFixture]
    public class IncrementalLoadingCollectionTests
    {
        private sealed class PagedIntSource : IIncrementalSource<int>
        {
            private readonly IReadOnlyList<int> _data;
            private int _index;

            public int LoadCalls { get; private set; }
            public int ResetCalls { get; private set; }
            public List<int> RequestedCounts { get; } = new();

            public PagedIntSource()
            {
                _data = Array.Empty<int>();
            }

            public PagedIntSource(IEnumerable<int> data)
            {
                _data = data?.ToList() ?? throw new ArgumentNullException(nameof(data));
            }

            public Task<IEnumerable<int>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
            {
                LoadCalls++;
                RequestedCounts.Add(count);

                cancellationToken.ThrowIfCancellationRequested();

                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                if (_index >= _data.Count)
                {
                    return Task.FromResult(Enumerable.Empty<int>());
                }

                var take = Math.Min(count, _data.Count - _index);
                var result = _data.Skip(_index).Take(take).ToList();
                _index += take;
                return Task.FromResult<IEnumerable<int>>(result);
            }

            public void Reset()
            {
                ResetCalls++;
                _index = 0;
            }
        }

        private sealed class TestIncrementalLoadingCollection : IncrementalLoadingCollection<PagedIntSource, int>
        {
            public TestIncrementalLoadingCollection(PagedIntSource source, CancellationTokenSource cts)
                : base(source, cts)
            {
            }

            public Task RefreshForTestAsync() => RefreshAsync();
        }

        [Test]
        public async Task LoadMoreItemsAsyncCountZeroLoadsDefaultPage()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            LoadMoreItemsResult result = await collection.LoadMoreItemsAsync(0).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(collection.Count, Is.EqualTo(result.Count));
            Assert.That(source.RequestedCounts.Last(), Is.EqualTo(30));
            Assert.That(collection.HasMoreItems, Is.True);
        }

        [Test]
        public async Task LoadMoreItemsAsyncLoadsUntilTargetVisibleIncreaseReached()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 10));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            var r1 = await collection.LoadMoreItemsAsync(3).AsTask().ConfigureAwait(false);
            Assert.That(r1.Count, Is.EqualTo(3));
            Assert.That(collection.Count, Is.EqualTo(3));

            var r2 = await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);

            Assert.That(collection.Count, Is.EqualTo(8));
            Assert.That(r2.Count, Is.EqualTo(5));
            Assert.That(source.LoadCalls, Is.EqualTo(2));
            Assert.That(source.RequestedCounts, Is.EqualTo(new[] { 3, 5 }));
            Assert.That(collection.HasMoreItems, Is.True);
        }

        [Test]
        public async Task LoadMoreItemsAsyncSourceReturnsEmptySetsHasMoreItemsFalse()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Array.Empty<int>());
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(collection.Count, Is.Zero);
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncCancellationRequestedHasMoreItemsFalse()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(collection.Count, Is.Zero);
            Assert.That(collection.HasMoreItems, Is.False);
            Assert.That(source.LoadCalls, Is.EqualTo(0));
        }

        [Test]
        public async Task RefreshAsyncWhileLoadingTriggersReloadAfterLoadCompletes()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollection(source, cts);

            // Load initial data
            var loadResult = await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            Assert.That(loadResult.Count, Is.EqualTo(5));
            Assert.That(collection.Count, Is.EqualTo(5));

            // Refresh after load completes
            await collection.RefreshForTestAsync().ConfigureAwait(false);

            Assert.That(source.ResetCalls, Is.GreaterThanOrEqualTo(1));
            Assert.That(collection.Count, Is.GreaterThan(0));
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(collection.Count));
        }

        [Test]
        public async Task RefreshAsyncOnEmptyCollectionForcesInitialLoad()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 50));
            var collection = new TestIncrementalLoadingCollection(source, cts);

            Assert.That(collection.Count, Is.EqualTo(0));

            await collection.RefreshForTestAsync().ConfigureAwait(false);

            Assert.That(collection.Count, Is.GreaterThan(0));
            Assert.That(source.ResetCalls, Is.GreaterThanOrEqualTo(1));
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(collection.Count));
        }

        [Test]
        public async Task LoadMoreItemsAsyncInvokesOnStartAndOnEnd()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 10));

            int start = 0;
            int end = 0;

            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                onStartLoading: () => Interlocked.Increment(ref start),
                onEndLoading: () => Interlocked.Increment(ref end));

            await collection.LoadMoreItemsAsync(1).AsTask().ConfigureAwait(false);

            Assert.That(start, Is.EqualTo(1));
            Assert.That(end, Is.EqualTo(1));
            Assert.That(collection.IsLoading, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncOnErrorInvokedWhenSourceThrows()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 10));
            var collection = new NegativePageSizeCollection(source, cts);

            Exception? observed = null;
            collection.OnError = ex => observed = ex;

            var result = await collection.LoadMoreItemsAsync(1).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(observed, Is.Not.Null);
            Assert.That(observed, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        private sealed class NegativePageSizeCollection : IncrementalLoadingCollection<PagedIntSource, int>
        {
            public NegativePageSizeCollection(PagedIntSource source, CancellationTokenSource cts)
                : base(source, cts)
            {
            }

            protected override Task<IEnumerable<int>> LoadDataAsync(int count, CancellationToken cancellationToken)
            {
                return base.LoadDataAsync(-1, cancellationToken);
            }
        }

        private sealed class NullListSource : IIncrementalSource<string?>
        {
            public Task<IEnumerable<string?>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
            {
                return Task.FromResult<IEnumerable<string?>>(null!);
            }

            public void Reset()
            {
            }
        }

        private sealed class NullFirstItemSource : IIncrementalSource<string?>
        {
            public Task<IEnumerable<string?>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
            {
                return Task.FromResult<IEnumerable<string?>>(new string?[] { null, "ok" });
            }

            public void Reset()
            {
            }
        }

        private sealed class OceSource : IIncrementalSource<int>
        {
            public Task<IEnumerable<int>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            public void Reset()
            {
            }
        }

        private sealed class OdeSource : IIncrementalSource<int>
        {
            public Task<IEnumerable<int>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
            {
                throw new ObjectDisposedException(nameof(OdeSource));
            }

            public void Reset()
            {
            }
        }

        private sealed class ThrowingSource : IIncrementalSource<int>
        {
            public Task<IEnumerable<int>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("boom");
            }

            public void Reset()
            {
            }
        }

        private sealed class TestIncrementalLoadingCollectionWithSource<TSource, TItem> : IncrementalLoadingCollection<TSource, TItem>
            where TSource : class, IIncrementalSource<TItem>, new()
        {
            public TestIncrementalLoadingCollectionWithSource(TSource source, CancellationTokenSource cts)
                : base(source, cts)
            {
            }

            public Task RefreshForTestAsync() => RefreshAsync();
        }

        [Test]
        public async Task LoadMoreItemsAsyncSequentialCallsLoadDataCorrectly()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 10));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            // First load
            var first = await collection.LoadMoreItemsAsync(3).AsTask().ConfigureAwait(false);
            Assert.That(first.Count, Is.EqualTo(3));
            Assert.That(collection.Count, Is.EqualTo(3));
            Assert.That(collection.IsLoading, Is.False);

            // Second load after first completes
            var second = await collection.LoadMoreItemsAsync(3).AsTask().ConfigureAwait(false);
            Assert.That(second.Count, Is.EqualTo(3));
            Assert.That(collection.Count, Is.EqualTo(6));
            Assert.That(collection.IsLoading, Is.False);

            Assert.That(source.LoadCalls, Is.EqualTo(2));
        }

        [Test]
        // This test ensures that if the source returns null, we treat it as the end of the collection (HasMoreItems = false).
        public async Task LoadMoreItemsAsyncSourceReturnsNullSetsHasMoreItemsFalse()
        {
            using var cts = new CancellationTokenSource();
            var source = new NullListSource();
            var collection = new TestIncrementalLoadingCollectionWithSource<NullListSource, string?>(source, cts);

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(collection.Count, Is.Zero);
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        // This test verifies specific behavior in IncrementalLoadingCollection where a page is considered empty/invalid 
        // if the first item is null, even if the user provided type allows nulls. 
        // Logic: if (data != null && data.Any() && data.First() != null)
        public async Task LoadMoreItemsAsyncSourceReturnsNullFirstItemSetsHasMoreItemsFalse()
        {
            using var cts = new CancellationTokenSource();
            var source = new NullFirstItemSource();
            var collection = new TestIncrementalLoadingCollectionWithSource<NullFirstItemSource, string?>(source, cts);

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(collection.Count, Is.Zero);
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncOperationCanceledExceptionDoesNotInvokeOnError()
        {
            using var cts = new CancellationTokenSource();
            var source = new OceSource();
            var collection = new TestIncrementalLoadingCollectionWithSource<OceSource, int>(source, cts);

            int errorCalls = 0;
            collection.OnError = _ => Interlocked.Increment(ref errorCalls);

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(errorCalls, Is.Zero);
            Assert.That(collection.IsLoading, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncObjectDisposedExceptionDoesNotInvokeOnError()
        {
            using var cts = new CancellationTokenSource();
            var source = new OdeSource();
            var collection = new TestIncrementalLoadingCollectionWithSource<OdeSource, int>(source, cts);

            int errorCalls = 0;
            collection.OnError = _ => Interlocked.Increment(ref errorCalls);

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(errorCalls, Is.Zero);
            Assert.That(collection.IsLoading, Is.False);
        }

        [Test]
        public void LoadMoreItemsAsyncWhenOnErrorIsNullPropagatesException()
        {
            using var cts = new CancellationTokenSource();
            var source = new ThrowingSource();
            var collection = new TestIncrementalLoadingCollectionWithSource<ThrowingSource, int>(source, cts);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                _ = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);
            });
        }

        [Test]
        public async Task LoadMoreItemsAsyncWhenOnErrorIsSetSwallowsExceptionAndInvokesOnError()
        {
            using var cts = new CancellationTokenSource();
            var source = new ThrowingSource();
            var collection = new TestIncrementalLoadingCollectionWithSource<ThrowingSource, int>(source, cts);

            Exception? observed = null;
            collection.OnError = ex => observed = ex;

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(observed, Is.Not.Null);
            Assert.That(observed, Is.TypeOf<InvalidOperationException>());
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        public void ConstructorThrowsArgumentNullExceptionWhenSourceIsNull()
        {
            using var cts = new CancellationTokenSource();
            Assert.Throws<ArgumentNullException>(() =>
                new IncrementalLoadingCollection<PagedIntSource, int>(null!, cts));
        }

        [Test]
        public async Task RefreshAsyncWhenNotLoadingClearsResetsAndLoadsInitialPage()
        {
            using var cts = new CancellationTokenSource();

            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollection(source, cts);

            // Preload some items
            var preload = await collection.LoadMoreItemsAsync(3).AsTask().ConfigureAwait(false);
            Assert.That(preload.Count, Is.EqualTo(3));
            Assert.That(collection.Count, Is.EqualTo(3));

            await collection.RefreshForTestAsync().ConfigureAwait(false);

            Assert.That(source.ResetCalls, Is.GreaterThanOrEqualTo(1));
            Assert.That(collection.Count, Is.GreaterThan(0));
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(collection.Count));
            Assert.That(collection.HasMoreItems, Is.True);
        }

        [Test]
        public async Task LoadMoreItemsAsyncPartialLastPageSetsHasMoreItemsFalseWhenSourceExhausted()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 7));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            // Request 10, but only 7 exist. Implementation will attempt another fetch and then set HasMoreItems=false.
            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.EqualTo(7));
            Assert.That(collection.Count, Is.EqualTo(7));
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncPropertyChangedEventsRaisedForIsLoading()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 10));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            var propertyChangedEvents = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                {
                    propertyChangedEvents.Add(e.PropertyName);
                }
            };

            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);

            Assert.That(propertyChangedEvents, Contains.Item(nameof(collection.IsLoading)));
            Assert.That(propertyChangedEvents.Count(p => p == nameof(collection.IsLoading)), Is.GreaterThanOrEqualTo(2)); // Started and Stopped
        }

        [Test]
        public async Task LoadMoreItemsAsyncPropertyChangedEventsRaisedForHasMoreItems()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 5));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            var propertyChangedEvents = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                {
                    propertyChangedEvents.Add(e.PropertyName);
                }
            };

            // Load all items so HasMoreItems becomes false
            await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(propertyChangedEvents, Contains.Item(nameof(collection.HasMoreItems)));
        }

        [Test]
        public async Task LoadMoreItemsAsyncMultiplePartialPagesUntilExhausted()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 23));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            // Request 10, source returns 10
            var r1 = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);
            Assert.That(r1.Count, Is.EqualTo(10));
            Assert.That(collection.Count, Is.EqualTo(10));
            Assert.That(collection.HasMoreItems, Is.True);

            // Request 10, source returns 10
            var r2 = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);
            Assert.That(r2.Count, Is.EqualTo(10));
            Assert.That(collection.Count, Is.EqualTo(20));
            Assert.That(collection.HasMoreItems, Is.True);

            // Request 10, source returns 3 (partial page)
            var r3 = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);
            Assert.That(r3.Count, Is.EqualTo(3));
            Assert.That(collection.Count, Is.EqualTo(23));
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        public async Task RefreshAsyncMultipleSequentialCallsWorkCorrectly()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollection(source, cts);

            // Initial load
            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            Assert.That(collection.Count, Is.EqualTo(5));

            // Multiple sequential refresh calls
            await collection.RefreshForTestAsync().ConfigureAwait(false);
            Assert.That(collection.Count, Is.GreaterThan(0));

            await collection.RefreshForTestAsync().ConfigureAwait(false);
            Assert.That(collection.Count, Is.GreaterThan(0));

            await collection.RefreshForTestAsync().ConfigureAwait(false);
            Assert.That(collection.Count, Is.GreaterThan(0));

            // Should complete without issues
            Assert.That(source.ResetCalls, Is.GreaterThanOrEqualTo(3));
            Assert.That(collection.HasMoreItems, Is.True);
        }

        [Test]
        public async Task LoadMoreItemsAsyncSequentialCallsAfterCompletionWork()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 30));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            var r1 = await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            Assert.That(r1.Count, Is.EqualTo(5));
            Assert.That(collection.IsLoading, Is.False);

            var r2 = await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            Assert.That(r2.Count, Is.EqualTo(5));
            Assert.That(collection.IsLoading, Is.False);

            var r3 = await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            Assert.That(r3.Count, Is.EqualTo(5));
            Assert.That(collection.IsLoading, Is.False);

            Assert.That(collection.Count, Is.EqualTo(15));
            Assert.That(source.LoadCalls, Is.EqualTo(3));
        }

        [Test]
        public async Task LoadMoreItemsAsyncCancellationBeforeLoadCausesHasMoreItemsFalse()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            // Cancel before loading
            cts.Cancel();

            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            // HasMoreItems returns false because the cancellation token is checked in the getter
            Assert.That(collection.HasMoreItems, Is.False);
            Assert.That(result.Count, Is.Zero);
            Assert.That(source.LoadCalls, Is.Zero);
        }

        [Test]
        public async Task LoadMoreItemsAsyncStartChangingAndEndChangingCalledCorrectly()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 10));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            int changingStartCount = 0;
            int changingEndCount = 0;

            ((INotifyPropertyChanged)collection).PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(collection.IsChanging))
                {
                    if (collection.IsChanging)
                        changingStartCount++;
                    else
                        changingEndCount++;
                }
            };

            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);

            Assert.That(changingStartCount, Is.GreaterThan(0));
            Assert.That(changingEndCount, Is.GreaterThan(0));
            Assert.That(collection.IsChanging, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncWithFilterOnlyVisibleItemsCount()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 20));
            var filter = new EvenNumberFilter();
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                itemsFilter: filter);

            // Request 5 visible items. The implementation will keep loading from the source
            // until it has enough items that pass the filter to reach the target visible count.
            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);

            // Should have loaded enough items to get 5 visible ones after filtering
            Assert.That(collection.Count, Is.EqualTo(5)); // Only even numbers visible
            Assert.That(collection.OriginalItems.Count, Is.GreaterThanOrEqualTo(10)); // At least 10 loaded to get 5 evens (numbers 1-10)
        }

        [Test]
        public async Task LoadMoreItemsAsyncWithSortingMaintainsSortOrder()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(new[] { 5, 1, 9, 3, 7, 2, 8, 4, 6 });
            var comparer = new IntComparer();
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                itemsComparer: comparer);

            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);

            // Check that items are sorted
            var items = collection.ToList();
            for (int i = 0; i < items.Count - 1; i++)
            {
                Assert.That(items[i], Is.LessThanOrEqualTo(items[i + 1]));
            }
        }

        [Test]
        public async Task RefreshAsyncAfterLoadCompleteClearsAndReloads()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollection(source, cts);

            // Initial load
            await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);
            Assert.That(collection.Count, Is.EqualTo(10));
            int firstResetCalls = source.ResetCalls;

            // Refresh when not loading
            await collection.RefreshForTestAsync().ConfigureAwait(false);

            Assert.That(source.ResetCalls, Is.GreaterThan(firstResetCalls));
            Assert.That(collection.Count, Is.GreaterThan(0)); // Should have reloaded
            Assert.That(collection.HasMoreItems, Is.True);
        }

        private sealed class ControllableSource : IIncrementalSource<int>
        {
            private readonly IReadOnlyList<int> _data;
            private int _index;
            private TaskCompletionSource<bool>? _loadStarted;
            private TaskCompletionSource<bool>? _canComplete;
            private TaskCompletionSource<bool>? _inFlightCanComplete;

            public int LoadCalls { get; private set; }

            public ControllableSource()
            {
                _data = Array.Empty<int>();
            }

            public ControllableSource(IEnumerable<int> data)
            {
                _data = data?.ToList() ?? throw new ArgumentNullException(nameof(data));
            }

            public void PrepareForLoad()
            {
                // Re-create TCS for a new "session" of controlled loads
                _loadStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _canComplete = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public Task WaitForLoadStartedAsync() => _loadStarted?.Task ?? Task.CompletedTask;

            public void AllowCompletion()
            {
                _inFlightCanComplete?.TrySetResult(true);
                _canComplete?.TrySetResult(true);
            }

            public async Task<IEnumerable<int>> LoadMoreItemsAsync(int count, CancellationToken cancellationToken)
            {
                LoadCalls++;

                var loadStarted = _loadStarted;
                var canComplete = _canComplete;
                _inFlightCanComplete = canComplete;

                // Clear for next load
                _loadStarted = null;
                _canComplete = null;

                loadStarted?.TrySetResult(true);

                if (canComplete != null)
                {
                    // Check if AllowCompletion() was already called
                    if (!canComplete.Task.IsCompleted)
                    {
                        using var registration = cancellationToken.Register(() => canComplete.TrySetCanceled());
                        try
                        {
                            await canComplete.Task.ConfigureAwait(false);
                        }
                        catch (TaskCanceledException)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            throw;
                        }
                    }
                }

                _inFlightCanComplete = null;

                cancellationToken.ThrowIfCancellationRequested();

                if (_index >= _data.Count)
                {
                    return Enumerable.Empty<int>();
                }

                var take = Math.Min(count, _data.Count - _index);
                var result = _data.Skip(_index).Take(take).ToList();
                _index += take;
                return result;
            }

            public void Reset()
            {
                _index = 0;
            }
        }

        private sealed class EvenNumberFilter : IFilter<int>
        {
            public bool ItemPassedFilter(int item)
            {
                return item % 2 == 0;
            }
        }

        private sealed class IntComparer : IExtendedComparer<int>
        {
            public int Compare(int x, int y)
            {
                return x.CompareTo(y);
            }

            public bool Equals(int x, int y)
            {
                return x == y;
            }

            public int GetHashCode(int obj)
            {
                return obj;
            }
        }

        [Test]
        public async Task LoadMoreItemsAsyncWhileLoadingReturnsZero()
        {
            using var cts = new CancellationTokenSource();
            var source = new ControllableSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollectionWithSource<ControllableSource, int>(source, cts);

            source.PrepareForLoad();

            var firstLoadTask = collection.LoadMoreItemsAsync(10).AsTask();

            await source.WaitForLoadStartedAsync().ConfigureAwait(false);
            Assert.That(collection.IsLoading, Is.True);

            // Second load should return immediately with 0 because IsLoading is true
            // Use timeout to detect if it hangs
            var secondLoadTask = collection.LoadMoreItemsAsync(5).AsTask();
            var completedTask = await Task.WhenAny(secondLoadTask, Task.Delay(1000)).ConfigureAwait(false);
            Assert.That(completedTask, Is.SameAs(secondLoadTask), "Second LoadMoreItemsAsync should return immediately");

            var secondResult = await secondLoadTask.ConfigureAwait(false);
            Assert.That(secondResult.Count, Is.Zero);

            source.AllowCompletion();
            var firstResult = await firstLoadTask.ConfigureAwait(false);

            Assert.That(firstResult.Count, Is.EqualTo(10));
            Assert.That(source.LoadCalls, Is.EqualTo(1));
            Assert.That(collection.Count, Is.EqualTo(10));
            Assert.That(collection.IsLoading, Is.False);
        }

        [Test]
        public async Task RefreshAsyncDuringActiveLoadCancelsAndReloadsAfterCompletion()
        {
            using var cts = new CancellationTokenSource();
            var source = new ControllableSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollectionWithSource<ControllableSource, int>(source, cts);

            source.PrepareForLoad();

            var loadTask = collection.LoadMoreItemsAsync(10).AsTask();

            await source.WaitForLoadStartedAsync().ConfigureAwait(false);
            Assert.That(collection.IsLoading, Is.True);

            var refreshTask = collection.RefreshForTestAsync();

            source.AllowCompletion();

            await loadTask.ConfigureAwait(false);
            await refreshTask.ConfigureAwait(false);

            Assert.That(collection.IsLoading, Is.False);
            Assert.That(collection.Count, Is.GreaterThan(0));
            Assert.That(collection.HasMoreItems, Is.True);
        }

        [Test]
        public void ConstructorWithoutSourceCreatesInstanceViaActivator()
        {
            using var cts = new CancellationTokenSource();

            // This constructor uses Activator.CreateInstance<TSource>()
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(cts);

            Assert.That(collection, Is.Not.Null);
            Assert.That(collection.HasMoreItems, Is.True);
            Assert.That(collection.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task LoadMoreItemsAsyncSourceExhaustedBeforeTargetVisibleCountWithFilter()
        {
            using var cts = new CancellationTokenSource();
            // Source has 10 items, but only 5 are even (2, 4, 6, 8, 10)
            var source = new PagedIntSource(Enumerable.Range(1, 10));
            var filter = new EvenNumberFilter();
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                itemsFilter: filter);

            // Request 10 visible items, but only 5 exist
            await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            // Should have loaded all 10 items to OriginalItems
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(10));
            // But only 5 visible (even numbers)
            Assert.That(collection.Count, Is.EqualTo(5));
            // Source is exhausted
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncCancellationDuringLoadPreservesHasMoreItems()
        {
            using var cts = new CancellationTokenSource();
            var source = new ControllableSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollectionWithSource<ControllableSource, int>(source, cts);

            source.PrepareForLoad();

            var loadTask = collection.LoadMoreItemsAsync(10).AsTask();

            await source.WaitForLoadStartedAsync().ConfigureAwait(false);
            Assert.That(collection.IsLoading, Is.True);

            cts.Cancel();

            source.AllowCompletion();

            var result = await loadTask.ConfigureAwait(false);

            Assert.That(result.Count, Is.Zero);
            Assert.That(collection.IsLoading, Is.False);
            Assert.That(collection.HasMoreItems, Is.False);
        }

        [Test]
        public async Task LoadMoreItemsAsyncWithBothItemsFilterAndSearchFilterApplied()
        {
            using var cts = new CancellationTokenSource();
            // Source has numbers 1-20
            var source = new PagedIntSource(Enumerable.Range(1, 20));
            var itemsFilter = new EvenNumberFilter(); // Only even numbers
            var searchFilter = new GreaterThanTenFilter(); // Only numbers > 10
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                itemsFilter: itemsFilter,
                searchFilter: searchFilter);

            // Request items - should get only even numbers > 10 (12, 14, 16, 18, 20)
            await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            // All 20 items loaded to OriginalItems
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(20));
            // Only 5 visible (even numbers > 10)
            Assert.That(collection.Count, Is.EqualTo(5));

            // Verify all visible items pass both filters
            foreach (var item in collection)
            {
                Assert.That(item % 2, Is.Zero, "Item should be even");
                Assert.That(item, Is.GreaterThan(10), "Item should be > 10");
            }
        }

        private sealed class GreaterThanTenFilter : ISearchFilter<int>
        {
            private string _searchText = string.Empty;
            public string SearchText
            {
                get => _searchText;
                set
                {
                    if (_searchText != value)
                    {
                        _searchText = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public bool ItemPassedFilter(int item)
            {
                return item > 10;
            }
        }

        [Test]
        public async Task RefreshAsyncClearsBothCollectionAndOriginalItems()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new TestIncrementalLoadingCollection(source, cts);

            // Initial load
            await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);
            Assert.That(collection.Count, Is.EqualTo(10));
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(10));

            // Store first item to verify collection was cleared and reloaded
            var firstItemBefore = collection.First();

            // Refresh
            await collection.RefreshForTestAsync().ConfigureAwait(false);

            // Collection should be cleared and reloaded (starts from beginning)
            Assert.That(collection.Count, Is.GreaterThan(0));
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(collection.Count));
            // First item should be 1 again (source was reset)
            Assert.That(collection.First(), Is.EqualTo(1));
        }

        [Test]
        public async Task LoadMoreItemsAsyncWithSortingMaintainsOrderAcrossMultipleLoads()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(new[] { 9, 3, 7, 1, 5, 8, 2, 6, 4, 10, 15, 11, 13, 12, 14 });
            var comparer = new IntComparer();
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                itemsComparer: comparer);

            // First load
            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            var itemsAfterFirstLoad = collection.ToList();

            // Second load
            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            var itemsAfterSecondLoad = collection.ToList();

            // Third load
            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            var itemsAfterThirdLoad = collection.ToList();

            // Verify sort order is maintained after each load
            for (int i = 0; i < itemsAfterFirstLoad.Count - 1; i++)
            {
                Assert.That(itemsAfterFirstLoad[i], Is.LessThanOrEqualTo(itemsAfterFirstLoad[i + 1]));
            }

            for (int i = 0; i < itemsAfterSecondLoad.Count - 1; i++)
            {
                Assert.That(itemsAfterSecondLoad[i], Is.LessThanOrEqualTo(itemsAfterSecondLoad[i + 1]));
            }

            for (int i = 0; i < itemsAfterThirdLoad.Count - 1; i++)
            {
                Assert.That(itemsAfterThirdLoad[i], Is.LessThanOrEqualTo(itemsAfterThirdLoad[i + 1]));
            }

            Assert.That(collection.Count, Is.EqualTo(15));
        }

        [Test]
        public async Task SearchFilterPropertyChangedTriggersRefilter()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 20));
            var searchFilter = new ConfigurableSearchFilter(item => item > 5);
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                searchFilter: searchFilter);

            // Load items - initially filter shows items > 5
            await collection.LoadMoreItemsAsync(20).AsTask().ConfigureAwait(false);

            Assert.That(collection.OriginalItems.Count, Is.EqualTo(20));
            Assert.That(collection.Count, Is.EqualTo(15)); // Items 6-20

            // Change filter to show items > 10 by changing SearchText (triggers PropertyChanged)
            searchFilter.FilterFunc = item => item > 10;
            searchFilter.SearchText = "changed"; // This triggers PropertyChanged and ReFilterItems

            // Collection should now show only items > 10
            Assert.That(collection.Count, Is.EqualTo(10)); // Items 11-20
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(20)); // OriginalItems unchanged
        }

        private sealed class ConfigurableSearchFilter : ISearchFilter<int>
        {
            private string _searchText = string.Empty;
            public Func<int, bool> FilterFunc { get; set; }

            public ConfigurableSearchFilter(Func<int, bool> filterFunc)
            {
                FilterFunc = filterFunc;
            }

            public string SearchText
            {
                get => _searchText;
                set
                {
                    if (_searchText != value)
                    {
                        _searchText = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public bool ItemPassedFilter(int item)
            {
                return FilterFunc(item);
            }
        }

        [Test]
        public async Task FilterVariantsCanBeSwitchedAndApplied()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 20));
            var evenFilter = new EvenNumberFilter();
            var oddFilter = new OddNumberFilter();
            var filterVariants = new IFilter<int>[] { evenFilter, oddFilter };

            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                filterVariants: filterVariants,
                itemsFilter: evenFilter);

            // Load items with even filter
            await collection.LoadMoreItemsAsync(20).AsTask().ConfigureAwait(false);

            Assert.That(collection.OriginalItems.Count, Is.EqualTo(20));
            Assert.That(collection.Count, Is.EqualTo(10)); // Only even numbers (2,4,6,8,10,12,14,16,18,20)

            // Switch to odd filter
            collection.ItemsFilter = oddFilter;

            // Now should show only odd numbers
            Assert.That(collection.Count, Is.EqualTo(10)); // Only odd numbers (1,3,5,7,9,11,13,15,17,19)
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(20)); // OriginalItems unchanged

            // Verify FilterVariants is set correctly
            Assert.That(collection.FilterVariants, Is.EqualTo(filterVariants));
        }

        private sealed class OddNumberFilter : IFilter<int>
        {
            public bool ItemPassedFilter(int item)
            {
                return item % 2 != 0;
            }
        }

        [Test]
        public async Task LoadMoreItemsAsyncWithBothSortingAndFilterApplied()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(new[] { 15, 3, 12, 7, 20, 1, 18, 5, 10, 2, 14, 9, 16, 4, 8 });
            var comparer = new IntComparer();
            var filter = new EvenNumberFilter();
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                itemsComparer: comparer,
                itemsFilter: filter);

            await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            // Should have only even numbers, sorted
            var items = collection.ToList();

            // Verify all items are even
            foreach (var item in items)
            {
                Assert.That(item % 2, Is.Zero, $"Item {item} should be even");
            }

            // Verify items are sorted
            for (int i = 0; i < items.Count - 1; i++)
            {
                Assert.That(items[i], Is.LessThanOrEqualTo(items[i + 1]),
                    $"Items should be sorted: {items[i]} <= {items[i + 1]}");
            }
        }

        [Test]
        public async Task RefreshAsyncInvokesOnStartAndOnEndLoading()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));

            int startCalls = 0;
            int endCalls = 0;

            var collection = new TestIncrementalLoadingCollectionWithCallbacks(
                source,
                cts,
                onStartLoading: () => Interlocked.Increment(ref startCalls),
                onEndLoading: () => Interlocked.Increment(ref endCalls));

            // Initial load
            await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            Assert.That(startCalls, Is.EqualTo(1));
            Assert.That(endCalls, Is.EqualTo(1));

            // Refresh should also invoke callbacks
            await collection.RefreshForTestAsync().ConfigureAwait(false);

            Assert.That(startCalls, Is.GreaterThanOrEqualTo(2));
            Assert.That(endCalls, Is.GreaterThanOrEqualTo(2));
        }

        private sealed class TestIncrementalLoadingCollectionWithCallbacks : IncrementalLoadingCollection<PagedIntSource, int>
        {
            public TestIncrementalLoadingCollectionWithCallbacks(
                PagedIntSource source,
                CancellationTokenSource cts,
                Action onStartLoading,
                Action onEndLoading)
                : base(source, cts, onStartLoading: onStartLoading, onEndLoading: onEndLoading)
            {
            }

            public Task RefreshForTestAsync() => RefreshAsync();
        }

        [Test]
        public async Task LoadMoreItemsAsyncPartialLoadKeepsHasMoreItemsTrue()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 100));
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(source, cts);

            // Load only 10 items from 100 available
            var result = await collection.LoadMoreItemsAsync(10).AsTask().ConfigureAwait(false);

            Assert.That(result.Count, Is.EqualTo(10));
            Assert.That(collection.Count, Is.EqualTo(10));
            Assert.That(collection.HasMoreItems, Is.True); // Source still has 90 more items
        }

        [Test]
        public async Task OriginalItemsContainsAllLoadedItemsRegardlessOfFilter()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 20));
            var filter = new EvenNumberFilter(); // Only shows even numbers
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                itemsFilter: filter);

            await collection.LoadMoreItemsAsync(20).AsTask().ConfigureAwait(false);

            // OriginalItems should contain ALL loaded items (1-20)
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(20));
            Assert.That(collection.OriginalItems, Is.EquivalentTo(Enumerable.Range(1, 20)));

            // But visible collection should only have even numbers
            Assert.That(collection.Count, Is.EqualTo(10));
            Assert.That(collection.ToList(), Is.EquivalentTo(new[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 }));
        }

        [Test]
        public void LoadMoreItemsAsyncOnStartLoadingThrowsDoesNotBreakLoad()
        {
            using var cts = new CancellationTokenSource();
            var source = new PagedIntSource(Enumerable.Range(1, 10));

            int endCalls = 0;
            var collection = new IncrementalLoadingCollection<PagedIntSource, int>(
                source,
                cts,
                onStartLoading: () => throw new InvalidOperationException("OnStartLoading failed"),
                onEndLoading: () => Interlocked.Increment(ref endCalls));

            // OnStartLoading throws, but the exception should propagate (it's not caught)
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await collection.LoadMoreItemsAsync(5).AsTask().ConfigureAwait(false);
            });
        }
    }
}
