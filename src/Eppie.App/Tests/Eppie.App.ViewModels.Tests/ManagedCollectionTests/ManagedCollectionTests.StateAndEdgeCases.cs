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

using System.Collections.Specialized;
using System.ComponentModel;
using NUnit.Framework;
using Tuvi.App.ViewModels;
using static Eppie.App.ViewModels.Tests.ManagedCollectionTests.ManagedCollectionTestHelpers;

namespace Eppie.App.ViewModels.Tests.ManagedCollectionTestSuite
{
    public partial class ManagedCollectionTests
    {
        [Test]
        public void RefilterItemHidesItemIfConditionFails()
        {
            var col = new ManagedCollection<string>();
            col.Add("A");

            bool allowA = true;
            col.ItemsFilter = new PredicateFilter(_ => allowA);
            Assert.That(col, Contains.Item("A"));

            allowA = false;

            col.RefilterItem("A");

            Assert.That(col, Does.Not.Contain("A"));
            Assert.That(col.OriginalItems, Contains.Item("A"));
        }

        [Test]
        public void RefilterItemShowsItemIfConditionPasses()
        {
            var col = new ManagedCollection<string>();
            col.Add("A");

            bool allowA = false;
            col.ItemsFilter = new PredicateFilter(_ => allowA);
            Assert.That(col, Is.Empty);

            allowA = true;

            col.RefilterItem("A");

            Assert.That(col, Contains.Item("A"));
        }

        [Test]
        public void IsChangingNotifiesOnStartAndEnd()
        {
            var col = new ManagedCollection<string>();
            bool isChanging = false;
            int changeCount = 0;
            ((INotifyPropertyChanged)col).PropertyChanged += (s, e) =>
            {
                if (e != null && e.PropertyName == nameof(ManagedCollection<string>.IsChanging))
                {
                    isChanging = col.IsChanging;
                    changeCount++;
                }
            };

            col.StartChanging();

            Assert.That(isChanging, Is.True);
            Assert.That(changeCount, Is.EqualTo(1));

            col.EndChanging();

            Assert.That(isChanging, Is.False);
            Assert.That(changeCount, Is.EqualTo(2));
        }

        [Test]
        public void NestedLockOnlyNotifiesOnOuterEnd()
        {
            var col = new ManagedCollection<string>();
            int changeCount = 0;
            ((INotifyPropertyChanged)col).PropertyChanged += (s, e) =>
            {
                if (e != null && e.PropertyName == nameof(ManagedCollection<string>.IsChanging))
                {
                    changeCount++;
                }
            };

            col.StartChanging();
            col.StartChanging();
            col.EndChanging();

            Assert.That(col.IsChanging, Is.True);
            Assert.That(changeCount, Is.EqualTo(1));

            col.EndChanging();

            Assert.That(col.IsChanging, Is.False);
            Assert.That(changeCount, Is.EqualTo(2));
        }

        [Test]
        public void NullItemCanBeAddedIfGenericAllows()
        {
            var col = new ManagedCollection<string?>();

            col.Add(null);

            Assert.That(col.OriginalItems, Has.Count.EqualTo(1));
            Assert.That(col.OriginalItems[0], Is.Null);
            Assert.That(col, Has.Count.EqualTo(1));
            Assert.That(col[0], Is.Null);
        }

        [Test]
        public void EmptyOperationsDoNotCrash()
        {
            var col = new ManagedCollection<string>();

            col.RefilterItems(Array.Empty<string>());
            col.RefilterItems(null);
            col.AddRange(null);
            Assert.DoesNotThrow(() => col.RefreshAsync().GetAwaiter().GetResult());
        }

        [Test]
        public void ChangingFilterDuringStartChangingDoesNotRefilterUntilEndChanging()
        {
            var col = new ManagedCollection<string>();
            col.Add("A");
            col.Add("B");

            var filter = new StartsWithFilter { SearchText = "A" };
            col.SearchFilter = filter;
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A" }));

            col.StartChanging();
            // Setting properties on the existing filter is deferred until EndChanging,
            // but replacing the SearchFilter with a new instance is allowed during changing
            // and should apply immediately.
            col.SearchFilter = new StartsWithFilter { SearchText = "B" };

            // New filter should be applied immediately even while changing
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B" }));

            col.EndChanging();

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B" }));
        }

        [Test]
        public void FilterVariantsCanBeSetAndUsedManually()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A1", "B1", "A2" });

            var filterA = new PredicateFilter(x => x.StartsWith("A", StringComparison.Ordinal));
            var filterB = new PredicateFilter(x => x.StartsWith("B", StringComparison.Ordinal));

            col.FilterVariants = new IFilter<string>[] { filterA, filterB };
            col.ItemsFilter = filterA;

            Assert.That(col.FilterVariants, Has.Length.EqualTo(2));
            Assert.That(col.ItemsFilter, Is.SameAs(filterA));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "A2" }));

            col.ItemsFilter = filterB;

            Assert.That(col.ItemsFilter, Is.SameAs(filterB));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B1" }));
        }

        [Test]
        public void AddDuringStartChangingAddsToOriginal()
        {
            var col = new ManagedCollection<string>();
            col.Add("A");

            col.StartChanging();
            col.Add("B");

            Assert.That(col.OriginalItems, Contains.Item("B"));

            col.EndChanging();

            Assert.That(col.ToArray(), Contains.Item("B"));
        }

        [Test]
        public void SettingFilterDuringCollectionChangeKeepsCollectionConsistent()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A", "B" });

            col.StartChanging();

            // Add an item while changing
            col.Add("C");

            // Replace the search filter during the change. New filter should apply immediately
            col.SearchFilter = new StartsWithFilter { SearchText = "C" };

            // View should contain only matching items and OriginalItems should contain all items in insertion order
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "C" }));
            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A", "B", "C" }));

            // Add another matching item while still changing
            col.Add("C2");

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "C", "C2" }));
            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A", "B", "C", "C2" }));

            col.EndChanging();

            // After ending changes the collection remains consistent
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "C", "C2" }));
            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A", "B", "C", "C2" }));
        }

        [Test]
        public void MultipleOperationsDuringStartChangingBatchUpdate()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A", "B", "C" });

            var filter = new StartsWithFilter { SearchText = "" };
            col.SearchFilter = filter;

            col.StartChanging();
            filter.SearchText = "A";
            col.Add("A2");
            col.Add("B2");

            Assert.That(col.IsChanging, Is.True);

            col.EndChanging();

            Assert.That(col.IsChanging, Is.False);
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A", "A2" }));
        }

        [Test]
        public void SelectedSortingIndexIgnoresInvalidIndices()
        {
            var col = new ManagedCollection<string>();
            var comparer = new StubExtendedComparer();
            col.SortingVariants = new[] { comparer };

            col.SelectedSortingIndex = 0;
            Assert.That(col.ItemsComparer, Is.SameAs(comparer));

            col.SelectedSortingIndex = 100;

            Assert.That(col.ItemsComparer, Is.SameAs(comparer));
            Assert.That(col.SelectedSortingIndex, Is.Zero);

            col.SelectedSortingIndex = -1;

            Assert.That(col.ItemsComparer, Is.SameAs(comparer));
            Assert.That(col.SelectedSortingIndex, Is.Zero);
        }

        [Test]
        public void RefilterItemUpdatesVisibilityWhenItemPropertyChanges()
        {
            var itemA = new TestItem("A");
            var itemB = new TestItem("B");

            var col = new ManagedCollection<TestItem>();
            col.ItemsFilter = new TestItemFilter(x => x.Name.StartsWith("A", StringComparison.Ordinal));

            col.Add(itemA);
            col.Add(itemB);

            Assert.That(col, Contains.Item(itemA));
            Assert.That(col, Does.Not.Contain(itemB));

            itemB.Name = "A2";
            col.RefilterItem(itemB);

            Assert.That(col, Contains.Item(itemB));
            Assert.That(col.Count, Is.EqualTo(2));

            itemA.Name = "B2";
            col.RefilterItem(itemA);

            Assert.That(col, Does.Not.Contain(itemA));
            Assert.That(col.Count, Is.EqualTo(1));
        }

        [Test]
        public void RefilterItemsUpdatesMultipleItems()
        {
            var itemA = new TestItem("A");
            var itemB = new TestItem("B");
            var itemC = new TestItem("C");

            var col = new ManagedCollection<TestItem>();
            col.ItemsFilter = new TestItemFilter(x => x.Name.StartsWith("True", StringComparison.Ordinal));

            col.Add(itemA);
            col.Add(itemB);
            col.Add(itemC);

            Assert.That(col, Is.Empty);

            itemA.Name = "TrueA";
            itemC.Name = "TrueC";
            col.RefilterItems(new[] { itemA, itemB, itemC });

            Assert.That(col, Is.EquivalentTo(new[] { itemA, itemC }));
            Assert.That(col, Does.Not.Contain(itemB));
        }

        [Test]
        public void RefilterItemResortsIfItemAlreadyVisible()
        {
            var itemA = new TestItem("A");
            var itemB = new TestItem("B");

            var col = new ManagedCollection<TestItem>
            {
                ItemsComparer = new TestItemComparer()
            };

            col.Add(itemA);
            col.Add(itemB);

            Assert.That(col, Is.EqualTo(new[] { itemA, itemB }));

            itemA.Name = "Z";

            col.RefilterItem(itemA);

            Assert.That(col[0], Is.SameAs(itemB));
            Assert.That(col[1], Is.SameAs(itemA));
            Assert.That(col[1].Name, Is.EqualTo("Z"));
        }

        [Test]
        public void ItemsFilterChangeRaisesPropertyChanged()
        {
            var col = new ManagedCollection<string>();
            bool propertyChangedRaised = false;
            string? changedProperty = null;

            ((INotifyPropertyChanged)col).PropertyChanged += (s, e) =>
            {
                if (e?.PropertyName == nameof(ManagedCollection<string>.ItemsFilter))
                {
                    propertyChangedRaised = true;
                    changedProperty = e.PropertyName;
                }
            };

            col.ItemsFilter = new PredicateFilter(_ => true);

            Assert.That(propertyChangedRaised, Is.True);
            Assert.That(changedProperty, Is.EqualTo(nameof(ManagedCollection<string>.ItemsFilter)));
        }

        [Test]
        public void ItemsComparerChangeRaisesPropertyChanged()
        {
            var col = new ManagedCollection<string>();
            bool propertyChangedRaised = false;
            string? changedProperty = null;

            ((INotifyPropertyChanged)col).PropertyChanged += (s, e) =>
            {
                if (e?.PropertyName == nameof(ManagedCollection<string>.ItemsComparer))
                {
                    propertyChangedRaised = true;
                    changedProperty = e.PropertyName;
                }
            };

            col.ItemsComparer = new StubExtendedComparer();

            Assert.That(propertyChangedRaised, Is.True);
            Assert.That(changedProperty, Is.EqualTo(nameof(ManagedCollection<string>.ItemsComparer)));
        }

        [Test]
        public async Task RefreshAsyncRebuildsViewFromOriginalAsync()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A", "B", "C" });
            col.ItemsComparer = new StubExtendedComparer();

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A", "B", "C" }));

            // Directly modify OriginalItems to bypass automatic updates
            col.OriginalItems.Add("D");
            col.OriginalItems.Remove("A");

            // Verify view is stale
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A", "B", "C" }));

            await col.RefreshAsync().ConfigureAwait(false);

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B", "C", "D" }));
        }

        [Test]
        public void CountReflectsVisibleItemsNotOriginal()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string> { SearchFilter = filter };
            col.AddRange(new[] { "A1", "B1", "A2", "B2" });

            Assert.That(col.Count, Is.EqualTo(2));
            Assert.That(col.OriginalItems.Count, Is.EqualTo(4));
        }

        [Test]
        public void AddRangeWithFilterAddsOnlyMatchingItemsToView()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string> { SearchFilter = filter };

            col.AddRange(new[] { "A1", "B1", "A2", "B2" });

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A1", "B1", "A2", "B2" }));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "A2" }));
        }

        [Test]
        public void RefilterItemInsertsNewlyVisibleItemInSortedPosition()
        {
            var itemA = new TestItem("A");
            var itemB = new TestItem("B");
            var itemHidden = new TestItem("Z");

            var col = new ManagedCollection<TestItem>
            {
                ItemsComparer = new TestItemComparer(),
                ItemsFilter = new TestItemFilter(x => x.Name.StartsWith("A", StringComparison.Ordinal) || x.Name.StartsWith("B", StringComparison.Ordinal))
            };

            col.Add(itemA);
            col.Add(itemB);
            col.Add(itemHidden);

            Assert.That(col.ToArray(), Is.EqualTo(new[] { itemA, itemB }));
            Assert.That(col.OriginalItems, Is.EquivalentTo(new[] { itemA, itemB, itemHidden }));

            itemHidden.Name = "AA";
            col.RefilterItem(itemHidden);

            Assert.That(col.ToArray(), Is.EqualTo(new[] { itemA, itemHidden, itemB }));
        }

        [Test]
        public void SearchFilterChangeRaisesPropertyChanged()
        {
            var col = new ManagedCollection<string>();
            bool propertyChangedRaised = false;

            ((INotifyPropertyChanged)col).PropertyChanged += (s, e) =>
            {
                if (e?.PropertyName == nameof(ManagedCollection<string>.SearchFilter))
                {
                    propertyChangedRaised = true;
                }
            };

            col.SearchFilter = new StartsWithFilter { SearchText = "A" };

            Assert.That(propertyChangedRaised, Is.True);
        }

        [Test]
        public void ReplacingSearchFilterUnsubscribesFromOldFilterChanges()
        {
            var filterA = new StartsWithFilter { SearchText = "A" };
            var filterB = new StartsWithFilter { SearchText = "B" };

            var col = new ManagedCollection<string> { SearchFilter = filterA };
            col.AddRange(new[] { "A1", "B1" });

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1" }));

            col.SearchFilter = filterB;
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B1" }));

            filterA.SearchText = "B";

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B1" }));
        }

        [Test]
        public void SelectedSortingIndexChangeRaisesPropertyChanged()
        {
            var col = new ManagedCollection<string>();
            var forward = new StubExtendedComparer();
            var reverse = new ReverseStubExtendedComparer();
            col.SortingVariants = new IExtendedComparer<string>[] { forward, reverse };

            int selectedIndexChangeCount = 0;
            ((INotifyPropertyChanged)col).PropertyChanged += (s, e) =>
            {
                if (e?.PropertyName == nameof(ManagedCollection<string>.SelectedSortingIndex))
                {
                    selectedIndexChangeCount++;
                }
            };

            col.SelectedSortingIndex = 0;
            col.SelectedSortingIndex = 1;

            Assert.That(col.ItemsComparer, Is.SameAs(reverse));
            Assert.That(col.SelectedSortingIndex, Is.EqualTo(1));
            Assert.That(selectedIndexChangeCount, Is.EqualTo(2));
        }

        [Test]
        public void SelectedSortingIndexInvalidValueDoesNotRaisePropertyChanged()
        {
            var col = new ManagedCollection<string>();
            var forward = new StubExtendedComparer();
            col.SortingVariants = new IExtendedComparer<string>[] { forward };
            col.SelectedSortingIndex = 0;

            int selectedIndexChangeCount = 0;
            ((INotifyPropertyChanged)col).PropertyChanged += (s, e) =>
            {
                if (e?.PropertyName == nameof(ManagedCollection<string>.SelectedSortingIndex))
                {
                    selectedIndexChangeCount++;
                }
            };

            col.SelectedSortingIndex = -1;
            col.SelectedSortingIndex = 100;

            Assert.That(col.SelectedSortingIndex, Is.Zero);
            Assert.That(selectedIndexChangeCount, Is.Zero);
        }

        [Test]
        public void SearchFilterRefiltersOnAnyPropertyChanged()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A", "B" });

            var filter = new ProbeFilter { AllowAll = false };
            col.SearchFilter = filter;

            Assert.That(col, Is.Empty);

            filter.AllowAll = true;
            filter.RaiseChanged("SomeOtherProperty");

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A", "B" }));
        }

        [Test]
        public void RemoveAtInvalidIndexThrows()
        {
            var col = new ManagedCollection<string>();
            col.Add("A");

            Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveAt(1));
        }

        [Test]
        public void CollectionChangedIsRaisedDuringStartChanging()
        {
            var col = new ManagedCollection<string>();
            int changeEvents = 0;

            ((INotifyCollectionChanged)col).CollectionChanged += (s, e) => changeEvents++;

            col.StartChanging();
            col.Add("A");
            col.Add("B");
            col.EndChanging();

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A", "B" }));
            Assert.That(changeEvents, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void CollectionChangedIsRaisedWhenNotChanging()
        {
            var col = new ManagedCollection<string>();
            int changeEvents = 0;

            ((INotifyCollectionChanged)col).CollectionChanged += (s, e) => changeEvents++;

            col.Add("A");

            Assert.That(col, Is.EqualTo(new[] { "A" }));
            Assert.That(changeEvents, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void AddRangeWithFilterAndSortingAddsOnlyMatchingItemsToSortedView()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string>
            {
                SearchFilter = filter,
                ItemsComparer = new StubExtendedComparer()
            };

            col.AddRange(new[] { "B2", "A3", "A1", "B1", "A2" });

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "B2", "A3", "A1", "B1", "A2" }));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "A2", "A3" }));
        }

        [Test]
        public void RefilterItemForUnknownItemDoesNotAddToView()
        {
            var col = new ManagedCollection<string>();
            col.Add("A");

            col.ItemsFilter = new PredicateFilter(x => x.StartsWith("A", StringComparison.Ordinal));

            col.RefilterItem("AX");

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A" }));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A" }));
        }

        [Test]
        public void RefilterItemsNullDoesNotThrow()
        {
            var col = new ManagedCollection<string>();

            Assert.DoesNotThrow(() => col.RefilterItems(null));
        }

        [Test]
        public void AddRangeRaisesCollectionChangedInInsertOrder()
        {
            var col = new ManagedCollection<string>();

            var added = new List<string>();
            ((INotifyCollectionChanged)col).CollectionChanged += (s, e) =>
            {
                if (e?.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var x in e.NewItems)
                    {
                        added.Add((string)x);
                    }
                }
            };

            col.AddRange(new[] { "A", "B", "C" });

            Assert.That(added, Is.EqualTo(new[] { "A", "B", "C" }));
        }
    }
}
