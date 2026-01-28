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
using Tuvi.App.ViewModels;
using static Eppie.App.ViewModels.Tests.ManagedCollectionTests.ManagedCollectionTestHelpers;

namespace Eppie.App.ViewModels.Tests.ManagedCollectionTestSuite
{
    public partial class ManagedCollectionTests
    {
        [Test]
        public void ItemsComparerSortsItems()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "B", "A", "C" });

            col.ItemsComparer = new StubExtendedComparer();

            Assert.That(col, Is.EqualTo(new[] { "A", "B", "C" }));
        }

        [Test]
        public void SortingVariantsSelection()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "B", "A" });

            var forward = new StubExtendedComparer();
            var reverse = new ReverseStubExtendedComparer();

            col.SortingVariants = new IExtendedComparer<string>[] { forward, reverse };

            col.SelectedSortingIndex = 0;

            Assert.That(col.ItemsComparer, Is.SameAs(forward));
            Assert.That(col, Is.EqualTo(new[] { "A", "B" }));

            col.SelectedSortingIndex = 1;

            Assert.That(col.ItemsComparer, Is.SameAs(reverse));
            Assert.That(col, Is.EqualTo(new[] { "B", "A" }));
        }

        [Test]
        public void AddItemWithSortingInsertsAtCorrectPosition()
        {
            var col = new ManagedCollection<string>();
            col.ItemsComparer = new StubExtendedComparer();
            col.AddRange(new[] { "A", "C" });

            col.Add("B");

            Assert.That(col, Is.EqualTo(new[] { "A", "B", "C" }));
        }

        [Test]
        public void SortingRemoveDuplicates()
        {
            var col = new ManagedCollection<string>();
            col.ItemsComparer = new StubExtendedComparer();

            col.AddRange(new[] { "A", "A", "B" });

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A", "A", "B" }));
            Assert.That(col, Is.EqualTo(new[] { "A", "B" }));
        }

        [Test]
        public void SortingAndFilteringWorkTogether()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string>
            {
                SearchFilter = filter,
                ItemsComparer = new StubExtendedComparer()
            };

            col.AddRange(new[] { "B1", "A3", "A1", "B2", "A2" });

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "B1", "A3", "A1", "B2", "A2" }));
            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "A2", "A3" }));
        }

        [Test]
        public void ChangingSortingWithActiveFilterResortsFilteredItems()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string> { SearchFilter = filter };
            col.AddRange(new[] { "A3", "B1", "A1", "A2" });

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A3", "A1", "A2" }));

            col.ItemsComparer = new StubExtendedComparer();

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "A2", "A3" }));
        }

        [Test]
        public void ChangingFilterWithActiveSortingRefiltersAndSorts()
        {
            var col = new ManagedCollection<string>
            {
                ItemsComparer = new StubExtendedComparer()
            };
            col.AddRange(new[] { "B2", "A1", "B1", "A2" });

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A1", "A2", "B1", "B2" }));

            col.ItemsFilter = new PredicateFilter(x => x.StartsWith("B", StringComparison.Ordinal));

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "B1", "B2" }));
        }

        [Test]
        public void AddRangeWithSortingSortsAllItems()
        {
            var col = new ManagedCollection<string>
            {
                ItemsComparer = new StubExtendedComparer()
            };
            col.Add("B");

            col.AddRange(new[] { "D", "A", "C" });

            Assert.That(col.ToArray(), Is.EqualTo(new[] { "A", "B", "C", "D" }));
        }
    }
}
