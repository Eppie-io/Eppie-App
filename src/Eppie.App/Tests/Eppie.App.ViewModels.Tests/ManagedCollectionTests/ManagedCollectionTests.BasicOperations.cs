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

namespace Eppie.App.ViewModels.Tests.ManagedCollectionTestSuite
{
    public partial class ManagedCollectionTests
    {
        [Test]
        public void AddItemAddsToOriginalAndVisible()
        {
            var col = new ManagedCollection<string>();

            col.Add("A");

            Assert.That(col.OriginalItems, Has.Count.EqualTo(1));
            Assert.That(col, Has.Count.EqualTo(1));
            Assert.That(col.OriginalItems[0], Is.EqualTo("A"));
            Assert.That(col[0], Is.EqualTo("A"));
        }

        [Test]
        public void AddRangeAddsAllToOriginalAndVisible()
        {
            var col = new ManagedCollection<string>();
            var items = new[] { "A", "B", "C" };

            col.AddRange(items);

            Assert.That(col.OriginalItems, Is.EquivalentTo(items));
            Assert.That(col, Is.EquivalentTo(items));
            Assert.That(col.OriginalItems, Is.EqualTo(items), "Order should be preserved in OriginalItems");
        }

        [Test]
        public void RemoveItemRemovesFromOriginalAndVisible()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A", "B", "C" });

            col.RemoveAt(1);

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A", "C" }));
            Assert.That(col, Is.EqualTo(new[] { "A", "C" }));
        }

        [Test]
        public void RemoveAtRemovesCorrectItemWhenDuplicatesExist()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A", "B", "B", "C" });

            col.RemoveAt(1);

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(col, Is.EqualTo(new[] { "A", "B", "C" }));

            col.RemoveAt(1);

            Assert.That(col.OriginalItems, Is.EqualTo(new[] { "A", "C" }));
            Assert.That(col, Is.EqualTo(new[] { "A", "C" }));
        }

        [Test]
        public void ClearEmptiesOriginalAndVisible()
        {
            var col = new ManagedCollection<string>();
            col.AddRange(new[] { "A", "B" });

            col.Clear();

            Assert.That(col.OriginalItems, Is.Empty);
            Assert.That(col, Is.Empty);
        }

        [Test]
        public void InitializationIsEmpty()
        {
            var col = new ManagedCollection<string>();

            Assert.That(col.OriginalItems, Is.Empty);
            Assert.That(col, Is.Empty);
            Assert.That(col.SortingVariants, Is.Not.Null.And.Empty);
            Assert.That(col.FilterVariants, Is.Not.Null.And.Empty);
        }
    }
}
