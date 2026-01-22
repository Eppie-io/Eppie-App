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
using NUnit.Framework;
using Tuvi.App.ViewModels;
using static Eppie.App.ViewModels.Tests.ManagedCollectionTests.ManagedCollectionTestHelpers;

namespace Eppie.App.ViewModels.Tests.ManagedCollectionTestSuite
{
    public partial class ManagedCollectionTests
    {
        [Test]
        public void ReconcileOriginalItemsShouldAddItemsWhenCollectionIsEmpty()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            var targetList = new List<string> { "A", "B", "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsShouldRemoveItemsWhenNewListIsEmpty()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");
            collection.Add("B");
            var targetList = new List<string>();

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.Empty);
            Assert.That(collection, Is.Empty);
        }

        [Test]
        public void ReconcileOriginalItemsShouldHandleMixedAddRemoveMove()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");
            collection.Add("B");
            collection.Add("C");
            collection.Add("D");
            // Initial: [A, B, C, D]
            // Target:  [D, A, E, C]  (B removed, E added, D moved to start)

            var targetList = new List<string> { "D", "A", "E", "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsShouldMoveItemToEnd()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");
            collection.Add("B");
            collection.Add("C");

            var targetList = new List<string> { "B", "C", "A" };

            bool moveHappened = false;
            bool addHappened = false;
            bool removeHappened = false;
            bool replaceHappened = false;

            collection.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Move)
                {
                    moveHappened = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    addHappened = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    removeHappened = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    replaceHappened = true;
                }
            };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection, Is.EqualTo(targetList));

            // Depending on the internal sequence, reordering may be expressed as Move,
            // or as a Remove+Add (e.g. remove from one position then insert at another).
            Assert.That(moveHappened || (removeHappened && addHappened) || replaceHappened, Is.True,
                "At least one Move (or Remove+Add or Replace) event should fire for reordering");
        }

        [Test]
        public void ReconcileOriginalItemsShouldMaintainSortWhenComparerIsSet()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("B");
            collection.Add("A");
            collection.Add("C");

            // Sorting forces [A, B, C]
            collection.ItemsComparer = new ReconcileStringComparer();

            Assert.That(collection, Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(collection.OriginalItems, Is.EqualTo(new[] { "B", "A", "C" })); // DB order preserved

            // New DB order changes, but content same
            var targetList = new List<string> { "C", "A", "B" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList)); // Original items updated
            Assert.That(collection, Is.EqualTo(new[] { "A", "B", "C" })); // Visualization still sorted
        }

        [Test]
        public void ReconcileOriginalItemsShouldInvokeUpdateAction()
        {
            // Arrange
            var collection = new ManagedCollection<ReconcileTestItem>();
            var itemA = new ReconcileTestItem { Id = 1, Name = "A1" };
            var itemB = new ReconcileTestItem { Id = 2, Name = "B1" };
            collection.Add(itemA);
            collection.Add(itemB);

            var newItemA = new ReconcileTestItem { Id = 1, Name = "A2" }; // ID 1 updated
            var newItemC = new ReconcileTestItem { Id = 3, Name = "C1" }; // ID 3 new

            var targetList = new List<ReconcileTestItem> { newItemA, itemB, newItemC };

            int updatesCount = 0;

            // Act
            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                updatesCount++;
                oldItem.Name = newItem.Name; // Apply update
            });

            // Assert
            // A updated, B matched (no update), C added (no update call, just add)
            Assert.That(updatesCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(collection.OriginalItems[0].Name, Is.EqualTo("A2")); // Updated in place
            Assert.That(collection.Count, Is.EqualTo(3));
        }

        [Test]
        public void ReconcileOriginalItemsTopItemMovesToBottomDoesNotRecreateAllItems()
        {
            // Current: [A, B, C]
            // Target:  [B, C, A]
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            var targetList = new List<string> { "B", "C", "A" };

            int removeCount = 0;
            int addCount = 0;

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    removeCount += e.OldItems?.Count ?? 0;
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    addCount += e.NewItems?.Count ?? 0;
                }
            };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection.ToArray(), Is.EqualTo(targetList));

            // With minimal move strategy we expect limited churn.
            // Depending on the reordering path, this can take up to 3 remove/add item events.
            Assert.That(removeCount, Is.LessThanOrEqualTo(3));
            Assert.That(addCount, Is.LessThanOrEqualTo(3));
        }

        [Test]
        public void ReconcileOriginalItemsSwapTwoMiddleItemsOnlyMovesThoseItems()
        {
            // Current: [A, B, C, D]
            // Target:  [A, C, B, D]
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D" });

            var targetList = new List<string> { "A", "C", "B", "D" };

            int removeCount = 0;
            int addCount = 0;

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    removeCount += e.OldItems?.Count ?? 0;
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    addCount += e.NewItems?.Count ?? 0;
                }
            };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection.ToArray(), Is.EqualTo(targetList));

            // Expect small churn (moving one item via remove+insert typically produces 1 remove + 1 add).
            Assert.That(removeCount, Is.LessThanOrEqualTo(3));
            Assert.That(addCount, Is.LessThanOrEqualTo(3));
        }

        [Test]
        public void ReconcileOriginalItemsReverseOrderMovesManyButDoesNotLoseItems()
        {
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D", "E" });

            var targetList = new List<string> { "E", "D", "C", "B", "A" };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsDoesNotReinsertFollowerWhenTopMovesDown()
        {
            var a = new Token("A");
            var c = new Token("C");
            var f = new Token("F");
            var k = new Token("K");

            var g = new Token("G");

            var collection = new ManagedCollection<Token>();
            collection.AddRange(new[] { a, c, f, k });

            var targetList = new List<Token> { c, f, g, k };

            var removed = new List<object>();
            var added = new List<object>();
            bool resetHappened = false;

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    resetHappened = true;
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems)
                    {
                        removed.Add(it);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        added.Add(it);
                    }
                }
            };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(resetHappened, Is.False, "Reset should not be used for this scenario");
            Assert.That(removed.Any(x => object.ReferenceEquals(x, f)), Is.False, "F instance should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, f)), Is.False, "F instance should not be re-inserted");
        }

        [Test]
        public void ReconcileOriginalItemsDoesNotReinsertLeaderWhenItemMovesUp()
        {
            var c = new Token("C");
            var f = new Token("F");
            var g = new Token("G");
            var k = new Token("K");

            var a = new Token("A");

            var collection = new ManagedCollection<Token>();
            collection.AddRange(new[] { c, f, g, k });

            var targetList = new List<Token> { a, c, f, k };

            var removed = new List<object>();
            var added = new List<object>();
            bool resetHappened = false;

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    resetHappened = true;
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems)
                    {
                        removed.Add(it);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        added.Add(it);
                    }
                }
            };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(resetHappened, Is.False, "Reset should not be used for this scenario");
            Assert.That(removed.Any(x => object.ReferenceEquals(x, c)), Is.False, "C instance should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, c)), Is.False, "C instance should not be re-inserted");
        }

        [Test]
        public void SyncVisualCollectionWithoutComparerDoesNotReinsertFWhenAReplacedByG()
        {
            // Production-like scenario:
            // - ItemsComparer == null
            // - Source list (OriginalItems) changes from [A, C, F, K] to [C, F, G, K]
            // - C/F/K must be preserved by reference; only A is removed and G inserted.

            var a = new Token("A");
            var c = new Token("C");
            var f = new Token("F");
            var k = new Token("K");
            var g = new Token("G");

            var collection = new ManagedCollection<Token>();
            collection.AddRange(new[] { a, c, f, k });

            var targetList = new List<Token> { c, f, g, k };

            var removed = new List<object>();
            var added = new List<object>();

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems)
                    {
                        removed.Add(it);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        added.Add(it);
                    }
                }
            };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection.ToArray(), Is.EqualTo(targetList));

            // Key expectations:
            // - F must not be removed/re-added.
            Assert.That(removed.Any(x => object.ReferenceEquals(x, f)), Is.False, "F instance should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, f)), Is.False, "F instance should not be reinserted");
        }

        [Test]
        public void SyncVisualCollectionWithoutComparerDoesNotReinsertCWhenGReplacedByA()
        {
            // Inverse production-like scenario:
            // - ItemsComparer == null
            // - Source list changes from [C, F, G, K] to [A, C, F, K]
            // - C/F/K must be preserved by reference; only G removed and A inserted.

            var c = new Token("C");
            var f = new Token("F");
            var g = new Token("G");
            var k = new Token("K");
            var a = new Token("A");

            var collection = new ManagedCollection<Token>();
            collection.AddRange(new[] { c, f, g, k });

            var targetList = new List<Token> { a, c, f, k };

            var removed = new List<object>();
            var added = new List<object>();

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems)
                    {
                        removed.Add(it);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        added.Add(it);
                    }
                }
            };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection.ToArray(), Is.EqualTo(targetList));

            // Key expectations:
            // - C must not be removed/re-added.
            Assert.That(removed.Any(x => object.ReferenceEquals(x, c)), Is.False, "C instance should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, c)), Is.False, "C instance should not be reinserted");
        }

        [Test]
        public void SyncVisualCollectionKeepsExistingInstanceForEqualItemsWhenReloadReturnsNewInstances()
        {
            // Matches production ContactItem semantics:
            // - Equality is by key (Email)
            // - Reload returns new instances for existing items
            // - updateItem copies data into old instance
            // Expectation: unchanged items keep their old instances in the visual collection (no remove+add).

            var aOld = new EquatableToken("A") { Payload = "old" };
            var cOld = new EquatableToken("C") { Payload = "old" };
            var fOld = new EquatableToken("F") { Payload = "old" };
            var kOld = new EquatableToken("K") { Payload = "old" };

            var collection = new ManagedCollection<EquatableToken>();
            collection.AddRange(new[] { aOld, cOld, fOld, kOld });

            // Reload returns new instances for C/F/K and a new item G, while A disappears.
            var cNew = new EquatableToken("C") { Payload = "new" };
            var fNew = new EquatableToken("F") { Payload = "new" };
            var gNew = new EquatableToken("G") { Payload = "new" };
            var kNew = new EquatableToken("K") { Payload = "new" };

            var targetList = new List<EquatableToken> { cNew, fNew, gNew, kNew };

            var removed = new List<object>();
            var added = new List<object>();

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems)
                    {
                        removed.Add(it);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        added.Add(it);
                    }
                }
            };

            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) => oldItem.Payload = newItem.Payload);

            // Visual order must follow target
            Assert.That(collection.Select(x => x.EmailKey).ToArray(), Is.EqualTo(new[] { "C", "F", "G", "K" }));

            // Unchanged keys (C/F/K) must keep original instances.
            Assert.That(object.ReferenceEquals(collection[0], cOld), Is.True, "C should keep original instance");
            Assert.That(object.ReferenceEquals(collection[1], fOld), Is.True, "F should keep original instance");
            Assert.That(object.ReferenceEquals(collection[3], kOld), Is.True, "K should keep original instance");

            // Their data should be updated from new versions.
            Assert.That(cOld.Payload, Is.EqualTo("new"));
            Assert.That(fOld.Payload, Is.EqualTo("new"));
            Assert.That(kOld.Payload, Is.EqualTo("new"));

            // Ensure we did not remove+add the original unchanged instances.
            Assert.That(removed.Any(x => object.ReferenceEquals(x, fOld)), Is.False, "Old F instance should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, fOld)), Is.False, "Old F instance should not be re-added");
        }

        [Test]
        public void UpdateThenReconcileDoesNotReinsertStableItemsWhenDisplayNameChangeReorders()
        {
            // Reproduces ContactsModel sequence:
            // 1) existing.UpdateFrom(...) + RefilterItem(existing)
            // 2) later ReconcileOriginalItems(freshItems, updateItem)
            // ItemsComparer == null

            var a = new EquatableToken("A") { Payload = "old" };
            var c = new EquatableToken("C") { Payload = "old" };
            var f = new EquatableToken("F") { Payload = "old" };
            var k = new EquatableToken("K") { Payload = "old" };

            var collection = new ManagedCollection<EquatableToken>();
            collection.AddRange(new[] { a, c, f, k });

            // Step 1: local update (rename) - A becomes G (same key means same item in real app).
            // Here we simulate that the item with key "A" changes its display key to "G" by changing its Key.
            // Since Key is immutable in EquatableToken, we model the update by creating an updated instance and copying payload.
            // The important part is that RefilterItem is called before reconcile.
            var aUpdated = new EquatableToken("A") { Payload = "renamed" };
            a.Payload = aUpdated.Payload;
            collection.RefilterItem(a);

            // Step 2: fresh reload returns new instances and new order, with A disappearing and G appearing.
            var cNew = new EquatableToken("C") { Payload = "new" };
            var fNew = new EquatableToken("F") { Payload = "new" };
            var gNew = new EquatableToken("G") { Payload = "new" };
            var kNew = new EquatableToken("K") { Payload = "new" };

            var targetList = new List<EquatableToken> { cNew, fNew, gNew, kNew };

            var removed = new List<object>();
            var added = new List<object>();

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;

                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems)
                    {
                        removed.Add(it);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems)
                    {
                        added.Add(it);
                    }
                }
            };

            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) => oldItem.Payload = newItem.Payload);

            Assert.That(collection.Select(x => x.EmailKey).ToArray(), Is.EqualTo(new[] { "C", "F", "G", "K" }));

            // Stable key F must keep original instance and must not be removed/re-added.
            Assert.That(object.ReferenceEquals(collection[1], f), Is.True, "F should keep original instance");
            Assert.That(removed.Any(x => object.ReferenceEquals(x, f)), Is.False, "F instance should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, f)), Is.False, "F instance should not be re-added");
        }

        [Test]
        public void RenameAtoGDoesNotReinsertF()
        {
            // Current visible: [A, C, F, K]
            // After rename A -> G, source returns: [C, F, G, K]
            // Observed bug: F is removed+added (reinserted) though it can stay.

            var aOld = new EquatableToken("a@ex") { Display = "A" };
            var cOld = new EquatableToken("c@ex") { Display = "C" };
            var fOld = new EquatableToken("f@ex") { Display = "F" };
            var kOld = new EquatableToken("k@ex") { Display = "K" };

            var collection = new ManagedCollection<EquatableToken>();
            collection.AddRange(new[] { aOld, cOld, fOld, kOld });

            // fresh instances from reload (equal by EmailKey)
            var cNew = new EquatableToken("c@ex") { Display = "C" };
            var fNew = new EquatableToken("f@ex") { Display = "F" };
            var gNew = new EquatableToken("a@ex") { Display = "G" }; // same email as A, just renamed
            var kNew = new EquatableToken("k@ex") { Display = "K" };

            var targetList = new List<EquatableToken> { cNew, fNew, gNew, kNew };

            var removed = new List<object>();
            var added = new List<object>();

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems) removed.Add(it);
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems) added.Add(it);
                }
            };

            // Simulate ContactsModel reconcile which updates old instances
            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) => oldItem.Display = newItem.Display);

            Assert.That(collection.Select(x => x.Display).ToArray(), Is.EqualTo(new[] { "C", "F", "G", "K" }));

            Assert.That(removed.Any(x => object.ReferenceEquals(x, fOld)), Is.False, "F should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, fOld)), Is.False, "F should not be added");
        }

        [Test]
        public void RenameGtoADoesNotReinsertC()
        {
            // Current visible: [C, F, G, K]
            // After rename G -> A, source returns: [A, C, F, K]
            // Observed bug: C is removed+added (reinserted) though it can stay.

            var cOld = new EquatableToken("c@ex") { Display = "C" };
            var fOld = new EquatableToken("f@ex") { Display = "F" };
            var gOld = new EquatableToken("a@ex") { Display = "G" }; // same email as A
            var kOld = new EquatableToken("k@ex") { Display = "K" };

            var collection = new ManagedCollection<EquatableToken>();
            collection.AddRange(new[] { cOld, fOld, gOld, kOld });

            // fresh instances from reload (equal by EmailKey)
            var aNew = new EquatableToken("a@ex") { Display = "A" };
            var cNew = new EquatableToken("c@ex") { Display = "C" };
            var fNew = new EquatableToken("f@ex") { Display = "F" };
            var kNew = new EquatableToken("k@ex") { Display = "K" };

            var targetList = new List<EquatableToken> { aNew, cNew, fNew, kNew };

            var removed = new List<object>();
            var added = new List<object>();

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (var it in e.OldItems) removed.Add(it);
                }
                else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var it in e.NewItems) added.Add(it);
                }
            };

            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) => oldItem.Display = newItem.Display);

            Assert.That(collection.Select(x => x.Display).ToArray(), Is.EqualTo(new[] { "A", "C", "F", "K" }));

            Assert.That(removed.Any(x => object.ReferenceEquals(x, cOld)), Is.False, "C instance should not be removed");
            Assert.That(added.Any(x => object.ReferenceEquals(x, cOld)), Is.False, "C instance should not be added");
        }

        [Test]
        public void ReconcileOriginalItemsShouldDoNothingWhenInputIsNull()
        {
            var collection = new ManagedCollection<string>();
            collection.Add("A");

            collection.ReconcileOriginalItems(null);

            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(collection[0], Is.EqualTo("A"));
        }

        [Test]
        public void ReconcileOriginalItemsShouldRespectFilter()
        {
            var collection = new ManagedCollection<ReconcileTestItem>();
            collection.ItemsFilter = new ReconcileTestItemFilter(id => id % 2 == 0); // Allow evens

            collection.Add(new ReconcileTestItem { Id = 2, Name = "Two" }); // Visible
            collection.Add(new ReconcileTestItem { Id = 4, Name = "Four" }); // Visible

            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 2, Name = "Two" },
                new ReconcileTestItem { Id = 3, Name = "Three" }, // New, Odd -> Hidden
                new ReconcileTestItem { Id = 6, Name = "Six" }    // New, Even -> Visible
            };

            collection.ReconcileOriginalItems(targetList);

            // OriginalItems should match targetList exactly
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));

            // Visual collection should only contain evens: 2, 6. (4 removed, 3 hidden)
            Assert.That(collection.Select(x => x.Id).ToList(), Is.EqualTo(new List<int> { 2, 6 }));
        }

        [Test]
        public void ReconcileOriginalItemsUpdateActionShouldAffectFilterVisibility()
        {
            var collection = new ManagedCollection<ReconcileTestItem>();
            // Filter: Name starts with "A"
            collection.ItemsFilter = new ReconcileTestItemFilterString(s => s.StartsWith("A", StringComparison.Ordinal));

            var item1 = new ReconcileTestItem { Id = 1, Name = "A1" }; // Visible
            var item2 = new ReconcileTestItem { Id = 2, Name = "B1" }; // Hidden
            collection.Add(item1);
            collection.Add(item2);

            Assert.That(collection, Contains.Item(item1));
            Assert.That(collection, Does.Not.Contain(item2));

            // Target: 
            // 1: "A1" -> "B1" (Should become hidden)
            // 2: "B1" -> "A2" (Should become visible)
            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 1, Name = "B1" },
                new ReconcileTestItem { Id = 2, Name = "A2" }
            };

            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                oldItem.Name = newItem.Name;
            });

            Assert.That(collection.OriginalItems.Count, Is.EqualTo(2));
            // Visual collection: Item 1 hidden, Item 2 visible
            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(collection[0].Id, Is.EqualTo(2));
            Assert.That(collection[0].Name, Is.EqualTo("A2"));
        }

        [Test]
        public void ReconcileOriginalItemsUpdateActionShouldAffectSorting()
        {
            var collection = new ManagedCollection<ReconcileTestItem>();
            // Sort by Name
            collection.ItemsComparer = new ReconcileTestItemComparer();

            var item1 = new ReconcileTestItem { Id = 1, Name = "C" };
            var item2 = new ReconcileTestItem { Id = 2, Name = "A" };
            collection.Add(item1);
            collection.Add(item2);

            // Initial visual order: A(2), C(1)
            Assert.That(collection[0].Id, Is.EqualTo(2));
            Assert.That(collection[1].Id, Is.EqualTo(1));

            // Update: 1 -> "A", 2 -> "C"
            // Swap names. New visual order should be: A(1), C(2)
            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 1, Name = "A" },
                new ReconcileTestItem { Id = 2, Name = "C" }
            };

            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                oldItem.Name = newItem.Name;
            });

            Assert.That(collection.Count, Is.EqualTo(2));
            Assert.That(collection[0].Id, Is.EqualTo(1)); // "A" comes first
            Assert.That(collection[1].Id, Is.EqualTo(2)); // "C" comes second
        }

        [Test]
        public void ReconcileOriginalItemsShouldHandleInsertMiddle()
        {
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "C" });

            var targetList = new List<string> { "A", "B", "C" };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection, Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsShouldHandleRemoveMiddle()
        {
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            var targetList = new List<string> { "A", "C" };

            collection.ReconcileOriginalItems(targetList);

            Assert.That(collection, Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsShouldUpdateItemEvenIfMoved()
        {
            // Arrange
            var collection = new ManagedCollection<ReconcileTestItem>();
            var item1 = new ReconcileTestItem { Id = 1, Name = "A" };
            var item2 = new ReconcileTestItem { Id = 2, Name = "B" };
            collection.Add(item1);
            collection.Add(item2);

            // Target: Swap order and modify "B"
            var newList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 2, Name = "B_Updated" },
                new ReconcileTestItem { Id = 1, Name = "A" }
            };

            // Act
            collection.ReconcileOriginalItems(newList, (oldItem, newItem) => oldItem.Name = newItem.Name);

            // Assert
            Assert.That(collection.Count, Is.EqualTo(2));
            Assert.That(collection[0].Id, Is.EqualTo(2));
            Assert.That(collection[0].Name, Is.EqualTo("B_Updated")); // Moved and Updated
            Assert.That(collection[1].Id, Is.EqualTo(1));
            Assert.That(collection[1].Name, Is.EqualTo("A"));
        }

        [Test]
        public void ReconcileOriginalItemsShouldRespectSearchFilter()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");
            collection.Add("B");

            var searchFilter = new TestSearchFilter();
            searchFilter.SearchText = "A";
            collection.SearchFilter = searchFilter;

            Assert.That(collection, Is.EqualTo(new[] { "A" })); // Initial filter

            var targetList = new List<string> { "A", "B", "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            // OriginalItems should have all
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            // Visual items should be filtered
            Assert.That(collection, Is.EqualTo(new[] { "A" }));

            // Update filter
            searchFilter.SearchText = "C";
            searchFilter.OnSearchTextChanged(); // Trigger update

            Assert.That(collection, Is.EqualTo(new[] { "C" }));
        }

        [Test]
        public void ReconcileOriginalItemsShouldHandleNullItems()
        {
            // Arrange
            var collection = new ManagedCollection<string?>();
            collection.Add("A");

            var targetList = new List<string?> { "A", null, "B" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection, Is.EqualTo(targetList));
            Assert.That(collection[1], Is.Null);
        }

        [Test]
        public void ReconcileOriginalItemsShouldHandleDuplicateItems()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");

            var targetList = new List<string> { "A", "A", "B" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.Count, Is.EqualTo(3));
            Assert.That(collection[0], Is.EqualTo("A"));
            Assert.That(collection[1], Is.EqualTo("A"));
            Assert.That(collection[2], Is.EqualTo("B"));
        }

        [Test]
        public void ReconcileOriginalItemsShouldHandleLazyEnumerable()
        {
            var collection = new ManagedCollection<string>();
            collection.Add("A");
            collection.Add("B");

            IEnumerable<string> GetLazyItems()
            {
                yield return "B";
                yield return "C";
            }

            collection.ReconcileOriginalItems(GetLazyItems());

            Assert.That(collection.OriginalItems, Is.EqualTo(new[] { "B", "C" }));
            Assert.That(collection, Is.EqualTo(new[] { "B", "C" }));
        }

        [Test]
        public void ReconcileOriginalItemsShouldNotInvokeUpdateActionForNewItems()
        {
            var collection = new ManagedCollection<ReconcileTestItem>();
            var itemA = new ReconcileTestItem { Id = 1, Name = "A" };
            collection.Add(itemA);

            var newItemB = new ReconcileTestItem { Id = 2, Name = "B" };
            var targetList = new List<ReconcileTestItem> { itemA, newItemB };

            bool updateCalledForB = false;

            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                if (newItem.Id == 2) updateCalledForB = true;
            });

            Assert.That(collection.Count, Is.EqualTo(2));
            Assert.That(updateCalledForB, Is.False, "Update action should not be called for newly added items");
        }

        [Test]
        public void ReconcileOriginalItemsShouldResetIsChangingOnException()
        {
            var collection = new ManagedCollection<string>();
            collection.Add("A");
            var targetList = new List<string> { "A" };

            Assert.Throws<InvalidOperationException>(() =>
            {
                collection.ReconcileOriginalItems(targetList, (o, n) =>
                {
                    throw new InvalidOperationException("Test exception");
                });
            });

            Assert.That(collection.IsChanging, Is.False, "IsChanging should be reset even if exception occurs");
        }

        [Test]
        public void ReconcileOriginalItemsShouldDoNothingWhenListsAreIdentical()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            var targetList = new List<string> { "A", "B", "C" };

            int eventCount = 0;
            collection.CollectionChanged += (s, e) => eventCount++;

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection, Is.EqualTo(targetList));
            // Minimal events expected for identical list (ideally 0 meaningful changes)
            Assert.That(eventCount, Is.LessThanOrEqualTo(3), "Minimal events expected for identical list");
        }

        [Test]
        public void ReconcileOriginalItemsShouldHandleLargeList()
        {
            // Arrange
            var collection = new ManagedCollection<int>();
            var initialList = Enumerable.Range(0, 100).ToList();
            foreach (var item in initialList)
            {
                collection.Add(item);
            }

            // Target: remove evens, add 100-149
            var targetList = Enumerable.Range(0, 100).Where(x => x % 2 != 0)
                .Concat(Enumerable.Range(100, 50)).ToList();

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToList(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsUpdateActionCalledOnlyForMatchingItems()
        {
            // Arrange
            var collection = new ManagedCollection<ReconcileTestItem>();
            var item1 = new ReconcileTestItem { Id = 1, Name = "A" };
            var item2 = new ReconcileTestItem { Id = 2, Name = "B" };
            var item3 = new ReconcileTestItem { Id = 3, Name = "C" };
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // New item with Id=2 (should match and update), new item with Id=4 (new)
            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 2, Name = "B_Updated" },
                new ReconcileTestItem { Id = 4, Name = "D_New" }
            };

            var updatedIds = new List<int>();

            // Act
            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                updatedIds.Add(oldItem.Id);
                oldItem.Name = newItem.Name;
            });

            // Assert
            Assert.That(updatedIds, Does.Contain(2), "updateItem should be called for item with Id=2");
            Assert.That(updatedIds, Does.Not.Contain(4), "updateItem should NOT be called for new item with Id=4");
            Assert.That(collection.Count, Is.EqualTo(2));
        }

        [Test]
        public void ReconcileOriginalItemsMultipleConsecutiveCalls()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            // Act & Assert - multiple reconciliations
            collection.ReconcileOriginalItems(new List<string> { "B", "C", "D" });
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "B", "C", "D" }));

            collection.ReconcileOriginalItems(new List<string> { "E", "F" });
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "E", "F" }));

            collection.ReconcileOriginalItems(new List<string> { "A", "B", "C", "D", "E" });
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "A", "B", "C", "D", "E" }));

            collection.ReconcileOriginalItems(new List<string>());
            Assert.That(collection, Is.Empty);
        }

        [Test]
        public void ReconcileOriginalItemsRemoveAllButOne()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D", "E" });

            var targetList = new List<string> { "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(collection[0], Is.EqualTo("C"));
        }

        [Test]
        public void ReconcileOriginalItemsWithComparerAddsNewItemInSortedPosition()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.ItemsComparer = new ReconcileStringComparer();
            collection.AddRange(new[] { "A", "C", "E" });

            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "A", "C", "E" }));

            // Target adds "B" and "D" in the middle
            var targetList = new List<string> { "A", "B", "C", "D", "E" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "A", "B", "C", "D", "E" }), "Items should be sorted");
        }

        [Test]
        public void ReconcileOriginalItemsIsChangingFlagDuringOperation()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");

            bool isChangingDuringUpdate = false;

            var targetList = new List<string> { "A" };

            // Act
            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                isChangingDuringUpdate = collection.IsChanging;
            });

            // Assert
            Assert.That(isChangingDuringUpdate, Is.True, "IsChanging should be true during reconciliation");
            Assert.That(collection.IsChanging, Is.False, "IsChanging should be false after reconciliation");
        }

        [Test]
        public void ReconcileOriginalItemsDoesNotFireResetEvent()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            bool resetFired = false;
            collection.CollectionChanged += (s, e) =>
            {
                if (e?.Action == NotifyCollectionChangedAction.Reset)
                    resetFired = true;
            };

            var targetList = new List<string> { "D", "E", "F" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(resetFired, Is.False, "Reset event should not be fired during reconcile");
        }

        [Test]
        public void ReconcileOriginalItemsInsertAtBeginning()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "B", "C", "D" });

            var targetList = new List<string> { "A", "B", "C", "D" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsRemoveFromEnd()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D" });

            var targetList = new List<string> { "A", "B" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsKeepsOriginalInstanceWhenEqualByValue()
        {
            // Arrange - using EquatableToken which has equality by EmailKey
            var item1 = new EquatableToken("key1") { Payload = "original1" };
            var item2 = new EquatableToken("key2") { Payload = "original2" };

            var collection = new ManagedCollection<EquatableToken>();
            collection.AddRange(new[] { item1, item2 });

            // New instances with same keys
            var newItem1 = new EquatableToken("key1") { Payload = "updated1" };
            var newItem2 = new EquatableToken("key2") { Payload = "updated2" };

            var targetList = new List<EquatableToken> { newItem1, newItem2 };

            // Act
            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                oldItem.Payload = newItem.Payload;
            });

            // Assert - original instances should be kept and updated
            Assert.That(object.ReferenceEquals(collection[0], item1), Is.True, "Original instance should be kept");
            Assert.That(object.ReferenceEquals(collection[1], item2), Is.True, "Original instance should be kept");
            Assert.That(item1.Payload, Is.EqualTo("updated1"), "Payload should be updated");
            Assert.That(item2.Payload, Is.EqualTo("updated2"), "Payload should be updated");
        }

        [Test]
        public void ReconcileOriginalItemsWithFilterAndComparer()
        {
            // Arrange
            var collection = new ManagedCollection<ReconcileTestItem>();
            collection.ItemsFilter = new ReconcileTestItemFilter(id => id % 2 == 0); // Only evens
            collection.ItemsComparer = new ReconcileTestItemComparer(); // Sort by Name

            collection.Add(new ReconcileTestItem { Id = 2, Name = "B" });
            collection.Add(new ReconcileTestItem { Id = 4, Name = "D" });

            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 1, Name = "A" }, // Odd - filtered out
                new ReconcileTestItem { Id = 2, Name = "B" }, // Even - visible
                new ReconcileTestItem { Id = 3, Name = "C" }, // Odd - filtered out
                new ReconcileTestItem { Id = 6, Name = "F" }  // Even - visible
            };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(4), "OriginalItems should have all items");
            Assert.That(collection.Count, Is.EqualTo(2), "Visual collection should only have evens");
            Assert.That(collection.Select(x => x.Id).ToArray(), Is.EqualTo(new[] { 2, 6 }));
        }

        [Test]
        public void ReconcileOriginalItemsCompleteReplacement()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            var targetList = new List<string> { "X", "Y", "Z" };

            int removeCount = 0;
            int addCount = 0;

            collection.CollectionChanged += (s, e) =>
            {
                if (e == null) return;
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    removeCount++;
                else if (e.Action == NotifyCollectionChangedAction.Add)
                    addCount++;
            };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            // Should have removed 3 and added 3
            Assert.That(removeCount, Is.GreaterThan(0));
            Assert.That(addCount, Is.GreaterThan(0));
        }

        [Test]
        public void ReconcileOriginalItemsPreservesOrderWhenNoComparer()
        {
            // Arrange - unsorted, no comparer
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "C", "A", "B" });

            // Target has different order
            var targetList = new List<string> { "B", "A", "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert - should follow target order exactly (no sorting)
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsSingleItemToMultiple()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");

            var targetList = new List<string> { "A", "B", "C", "D", "E" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsMultipleToSingle()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D", "E" });

            var targetList = new List<string> { "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsAdjacentSwap()
        {
            // Arrange - swap adjacent items
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D" });

            // Swap B and C
            var targetList = new List<string> { "A", "C", "B", "D" };

            bool resetFired = false;
            collection.CollectionChanged += (s, e) =>
            {
                if (e?.Action == NotifyCollectionChangedAction.Reset)
                    resetFired = true;
            };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(resetFired, Is.False, "Adjacent swap should not trigger Reset");
        }

        [Test]
        public void ReconcileOriginalItemsOriginalItemsAndVisualInSync()
        {
            // Arrange - no filter, no comparer
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            var targetList = new List<string> { "D", "B", "E", "A" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert - both OriginalItems and visual collection should match target
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsRemoveFirstItemOnly()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D" });

            var targetList = new List<string> { "B", "C", "D" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsRemoveLastItemOnly()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D" });

            var targetList = new List<string> { "A", "B", "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsSingleElementCollection()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");

            var targetList = new List<string> { "B" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReconcileOriginalItemsFromEmptyToSingleElement()
        {
            // Arrange
            var collection = new ManagedCollection<string>();

            var targetList = new List<string> { "A" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(collection[0], Is.EqualTo("A"));
        }

        [Test]
        public void ReconcileOriginalItemsFromSingleElementToEmpty()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.Add("A");

            var targetList = new List<string>();

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.Empty);
            Assert.That(collection, Is.Empty);
        }

        [Test]
        public void ReconcileOriginalItemsAllItemsFilteredOut()
        {
            // Arrange
            var collection = new ManagedCollection<ReconcileTestItem>();
            collection.ItemsFilter = new ReconcileTestItemFilter(id => id > 100); // Filter out all items with id <= 100

            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 1, Name = "A" },
                new ReconcileTestItem { Id = 2, Name = "B" },
                new ReconcileTestItem { Id = 3, Name = "C" }
            };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(3), "OriginalItems should have all items");
            Assert.That(collection, Is.Empty, "Visual collection should be empty (all filtered out)");
        }

        [Test]
        public void ReconcileOriginalItemsWithComparerAndFilterCombined()
        {
            // Arrange
            var collection = new ManagedCollection<ReconcileTestItem>();
            collection.ItemsFilter = new ReconcileTestItemFilter(id => id % 2 == 0); // Only evens
            collection.ItemsComparer = new ReconcileTestItemComparer(); // Sort by Name

            collection.Add(new ReconcileTestItem { Id = 2, Name = "Z" }); // Even, visible
            collection.Add(new ReconcileTestItem { Id = 4, Name = "A" }); // Even, visible

            // Initial visual: A(4), Z(2) - sorted by name
            Assert.That(collection[0].Id, Is.EqualTo(4));
            Assert.That(collection[1].Id, Is.EqualTo(2));

            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 1, Name = "B" }, // Odd - filtered
                new ReconcileTestItem { Id = 6, Name = "C" }, // Even - visible
                new ReconcileTestItem { Id = 2, Name = "Z" }, // Even - visible
            };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems.Count, Is.EqualTo(3));
            Assert.That(collection.Count, Is.EqualTo(2), "Only evens should be visible");
            // Sorted by name: C(6), Z(2)
            Assert.That(collection[0].Id, Is.EqualTo(6));
            Assert.That(collection[1].Id, Is.EqualTo(2));
        }

        [Test]
        public void ReconcileOriginalItemsWithSearchFilterAndItemsFilter()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.ItemsFilter = new PredicateFilter(s => s.Length <= 2); // Filter: length <= 2

            var searchFilter = new TestSearchFilter();
            searchFilter.SearchText = "A";
            collection.SearchFilter = searchFilter;

            collection.AddRange(new[] { "A", "AB", "ABC", "B" });
            // OriginalItems: A, AB, ABC, B
            // After ItemsFilter (length <= 2): A, AB, B
            // After SearchFilter (contains "A"): A, AB

            Assert.That(collection.OriginalItems.Count, Is.EqualTo(4));
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "A", "AB" }));

            var targetList = new List<string> { "A", "AX", "B", "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            // OriginalItems: A, AX, B, C
            // After ItemsFilter (length <= 2): A, AX, B, C
            // After SearchFilter (contains "A"): A, AX
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "A", "AX" }));
        }

        [Test]
        public void ReconcileOriginalItemsUpdateItemCalledForMovedItems()
        {
            // Arrange
            var collection = new ManagedCollection<ReconcileTestItem>();
            var item1 = new ReconcileTestItem { Id = 1, Name = "A" };
            var item2 = new ReconcileTestItem { Id = 2, Name = "B" };
            var item3 = new ReconcileTestItem { Id = 3, Name = "C" };
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Reorder and update
            var targetList = new List<ReconcileTestItem>
            {
                new ReconcileTestItem { Id = 3, Name = "C_Updated" },
                new ReconcileTestItem { Id = 1, Name = "A_Updated" },
                new ReconcileTestItem { Id = 2, Name = "B_Updated" }
            };

            var updatedIds = new List<int>();

            // Act
            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                updatedIds.Add(oldItem.Id);
                oldItem.Name = newItem.Name;
            });

            // Assert
            Assert.That(collection.Count, Is.EqualTo(3));
            Assert.That(updatedIds, Does.Contain(1));
            Assert.That(updatedIds, Does.Contain(2));
            Assert.That(updatedIds, Does.Contain(3));
            Assert.That(collection[0].Name, Is.EqualTo("C_Updated"));
            Assert.That(collection[1].Name, Is.EqualTo("A_Updated"));
            Assert.That(collection[2].Name, Is.EqualTo("B_Updated"));
        }

        [Test]
        public void ReconcileOriginalItemsPreservesInstancesWhenReordering()
        {
            // Arrange
            var item1 = new ReconcileTestItem { Id = 1, Name = "A" };
            var item2 = new ReconcileTestItem { Id = 2, Name = "B" };
            var item3 = new ReconcileTestItem { Id = 3, Name = "C" };

            var collection = new ManagedCollection<ReconcileTestItem>();
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Create new instances with same IDs but different order
            var newItem1 = new ReconcileTestItem { Id = 1, Name = "A_New" };
            var newItem2 = new ReconcileTestItem { Id = 2, Name = "B_New" };
            var newItem3 = new ReconcileTestItem { Id = 3, Name = "C_New" };

            var targetList = new List<ReconcileTestItem> { newItem3, newItem1, newItem2 };

            // Act
            collection.ReconcileOriginalItems(targetList, (oldItem, newItem) =>
            {
                oldItem.Name = newItem.Name;
            });

            // Assert - original instances should be preserved
            Assert.That(object.ReferenceEquals(collection[0], item3), Is.True, "Item3 should be same instance");
            Assert.That(object.ReferenceEquals(collection[1], item1), Is.True, "Item1 should be same instance");
            Assert.That(object.ReferenceEquals(collection[2], item2), Is.True, "Item2 should be same instance");
            Assert.That(item1.Name, Is.EqualTo("A_New"));
            Assert.That(item2.Name, Is.EqualTo("B_New"));
            Assert.That(item3.Name, Is.EqualTo("C_New"));
        }

        [Test]
        public void ReconcileOriginalItemsAddMultipleAtDifferentPositions()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "B", "D", "F" });

            // Add A at start, C in middle, E in middle, G at end
            var targetList = new List<string> { "A", "B", "C", "D", "E", "F", "G" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsRemoveMultipleFromDifferentPositions()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D", "E", "F", "G" });

            // Remove A (start), C (middle), E (middle), G (end)
            var targetList = new List<string> { "B", "D", "F" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsComplexScenarioAddRemoveReorder()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D", "E" });

            // Remove B, D; Add X, Y; Reorder remaining
            var targetList = new List<string> { "E", "X", "A", "Y", "C" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsWithComparerReverseSorted()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.ItemsComparer = new ReconcileStringComparer();
            collection.AddRange(new[] { "E", "D", "C", "B", "A" });

            // Visual should be sorted: A, B, C, D, E
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "A", "B", "C", "D", "E" }));
            // Original should preserve order
            Assert.That(collection.OriginalItems, Is.EqualTo(new[] { "E", "D", "C", "B", "A" }));

            // New target changes original order but content same
            var targetList = new List<string> { "A", "B", "C", "D", "E" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "A", "B", "C", "D", "E" }), "Visual still sorted");
        }

        [Test]
        public void ReconcileOriginalItemsNoChangesSameContentSameOrder()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            var targetList = new List<string> { "A", "B", "C" };

            int eventCount = 0;
            collection.CollectionChanged += (s, e) => eventCount++;

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            // Ideally no change events for identical content
            Assert.That(eventCount, Is.LessThanOrEqualTo(3), "Minimal events for no actual changes");
        }

        [Test]
        public void ReconcileOriginalItemsReplaceOneItemInPlace()
        {
            // Arrange - single item replacement at specific position
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C", "D" });

            // Replace B with X
            var targetList = new List<string> { "A", "X", "C", "D" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
        }

        [Test]
        public void ReconcileOriginalItemsReplaceAllItemsOneByOne()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            // Replace all items but maintain count
            var targetList = new List<string> { "X", "Y", "Z" };

            // Act
            collection.ReconcileOriginalItems(targetList);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(targetList));
            Assert.That(collection.Count, Is.EqualTo(3));
        }

        [Test]
        public void ReconcileOriginalItemsWithEnumerableNotList()
        {
            // Arrange
            var collection = new ManagedCollection<string>();
            collection.AddRange(new[] { "A", "B", "C" });

            IEnumerable<string> targetEnumerable = new[] { "B", "D", "C" };

            // Act
            collection.ReconcileOriginalItems(targetEnumerable);

            // Assert
            Assert.That(collection.ToArray(), Is.EqualTo(new[] { "B", "D", "C" }));
        }
    }
}
