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

using System.ComponentModel;
using NUnit.Framework;
using Tuvi.App.ViewModels;
using static Eppie.App.ViewModels.Tests.ManagedCollectionTests.ManagedCollectionTestHelpers;

namespace Eppie.App.ViewModels.Tests.ManagedCollectionTestSuite
{
    public partial class ManagedCollectionTests
    {
        private sealed class NullableStartsWithSearchFilter : ISearchFilter<string?>
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            private string _searchText = string.Empty;
            public string SearchText
            {
                get => _searchText;
                set
                {
                    if (!string.Equals(_searchText, value, StringComparison.Ordinal))
                    {
                        _searchText = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                    }
                }
            }

            public bool ItemPassedFilter(string? item)
                => item is not null && (string.IsNullOrEmpty(SearchText) || item.StartsWith(SearchText, StringComparison.Ordinal));
        }

        private sealed class NullablePredicateFilter : IFilter<string?>
        {
            private readonly Func<string?, bool> _predicate;
            public NullablePredicateFilter(Func<string?, bool> predicate) => _predicate = predicate;
            public bool ItemPassedFilter(string? item) => _predicate(item);
        }

        [Test]
        public void ItemsFilterFiltersVisibleItems()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A1", "B1", "A2" });

            col.ItemsFilter = new PredicateFilter(x => x.StartsWith("A", StringComparison.Ordinal));

            Assert.That(col.OriginalItems, Has.Count.EqualTo(3));
            Assert.That(col, Is.EqualTo(new[] { "A1", "A2" }));
        }

        [Test]
        public void SearchFilterFiltersVisibleItems()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A1", "B1", "A2" });

            var searchFilter = new StartsWithFilter { SearchText = "B" };

            col.SearchFilter = searchFilter;

            Assert.That(col, Is.EqualTo(new[] { "B1" }));
        }

        [Test]
        public void CombinedFilteringRequiresBothToPass()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A1", "A2", "AB1", "B1" });

            var searchFilter = new StartsWithFilter { SearchText = "AB" };
            col.SearchFilter = searchFilter;

            col.ItemsFilter = new PredicateFilter(x => x.StartsWith("A", StringComparison.Ordinal));

            Assert.That(col, Is.EqualTo(new[] { "AB1" }));
        }

        [Test]
        public void ChangeFilterRefreshesItems()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A1", "B1" });

            col.ItemsFilter = new PredicateFilter(x => x.StartsWith("A", StringComparison.Ordinal));
            Assert.That(col, Is.EqualTo(new[] { "A1" }));

            col.ItemsFilter = new PredicateFilter(x => x.StartsWith("B", StringComparison.Ordinal));

            Assert.That(col, Is.EqualTo(new[] { "B1" }));
        }

        [Test]
        public void SearchFilterPropertyChangedRefilters()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A1", "B1" });

            var filter = new StartsWithFilter { SearchText = "A" };
            col.SearchFilter = filter;
            Assert.That(col, Is.EqualTo(new[] { "A1" }));

            filter.SearchText = "B";

            Assert.That(col, Is.EqualTo(new[] { "B1" }));
        }

        [Test]
        public void NullFiltersShowAllItems()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A1", "B1" });

            col.ItemsFilter = new PredicateFilter(x => x.StartsWith("A", StringComparison.Ordinal));
            Assert.That(col, Has.Count.EqualTo(1));

            col.ItemsFilter = null;

            Assert.That(col, Is.EqualTo(new[] { "A1", "B1" }));
        }

        [Test]
        public void SettingSearchFilterToNullShowsAllItems()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string> { SearchFilter = filter };
            col.AddRange(new[] { "A1", "B1", "A2" });

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "A2" }));

            col.SearchFilter = null;

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "B1", "A2" }));
        }

        [Test]
        public void ReplacingSearchFilterUpdatesView()
        {
            var filterA = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string> { SearchFilter = filterA };
            col.AddRange(new[] { "A1", "B1", "C1" });

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1" }));

            var filterB = new StartsWithFilter { SearchText = "B" };
            col.SearchFilter = filterB;

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B1" }));
        }

        [Test]
        public void NullElementsAreHiddenBySearchFilter()
        {
            var filter = new NullableStartsWithSearchFilter { SearchText = "A" };
            var col = new ManagedCollection<string?> { SearchFilter = filter };

            col.AddRange(new string?[] { null, "A1", "B1" });

            Assert.That(col.OriginalItems, Is.EqualTo(new string?[] { null, "A1", "B1" }));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1" }));
        }

        [Test]
        public void NullElementsAreHiddenByItemsFilter()
        {
            var col = new ManagedCollection<string?>();
            col.ItemsFilter = new NullablePredicateFilter(s => s != null && s.StartsWith("A", StringComparison.Ordinal));

            col.AddRange(new string?[] { null, "A1", "B1" });

            Assert.That(col.OriginalItems, Is.EqualTo(new string?[] { null, "A1", "B1" }));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1" }));
        }

        [Test]
        public void SearchFilterNullUnsubscribesFromOldFilterChanges()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string> { SearchFilter = filter };
            col.AddRange(new[] { "A1", "B1" });

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1" }));

            col.SearchFilter = null;
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "B1" }));

            filter.SearchText = "B";

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "B1" }));
        }
    }
}
