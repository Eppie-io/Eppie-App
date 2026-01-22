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

using System.Diagnostics;
using NUnit.Framework;
using Tuvi.App.ViewModels;

namespace Eppie.App.ViewModels.Tests.ManagedCollectionTestSuite
{
    public partial class ManagedCollectionTests
    {
        [Test]
        [Explicit("Performance benchmark for optimization study")]
        [Category("Performance")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Benchmark randomization")]
        public void ReconcileOriginalItemsPerformance5000ItemsShuffleAndModify()
        {
            // Arrange
            int count = 5000;
            var collection = new ManagedCollection<string>();
            // Use Guids to simulate realistic string data
            var initialItems = Enumerable.Range(0, count).Select(i => Guid.NewGuid().ToString()).ToList();
            collection.AddRange(initialItems);

            // Prepare target list:
            // 1. Remove ~5% of items
            // 2. Add ~5% new items
            // 3. Shuffle the rest to simulate significant reordering
            var rand = new Random(42);
            var targetList = new List<string>(initialItems);

            // Remove 250 items
            for (int i = 0; i < 250; i++)
            {
                if (targetList.Count > 0)
                {
                    targetList.RemoveAt(rand.Next(targetList.Count));
                }
            }

            // Add 250 items
            for (int i = 0; i < 250; i++)
            {
                targetList.Add(Guid.NewGuid().ToString());
            }

            // Shuffle
            targetList = targetList.OrderBy(x => rand.Next()).ToList();

            // Cleanup before measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Act
            var sw = Stopwatch.StartNew();

            collection.ReconcileOriginalItems(targetList);

            sw.Stop();

            // Output
            TestContext.Out.WriteLine($"ReconcileOriginalItems (5000 items) Time: {sw.ElapsedMilliseconds} ms");

            // Assert
            Assert.That(collection.OriginalItems, Is.EqualTo(targetList));
            Assert.That(collection, Is.EqualTo(targetList));
        }
    }
}
