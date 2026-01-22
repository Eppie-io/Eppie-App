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
        public void AddRangeLargeCollectionBaselineKeepsAllItems()
        {
            var col = new ManagedCollection<int>();
            var items = Enumerable.Range(0, 50_000).ToArray();

            var sw = Stopwatch.StartNew();
            col.AddRange(items);
            sw.Stop();

            Assert.That(col.OriginalItems.Count, Is.EqualTo(items.Length));
            Assert.That(col.Count, Is.EqualTo(items.Length));
            Assert.That(col.OriginalItems[0], Is.Zero);
            Assert.That(col.OriginalItems[^1], Is.EqualTo(items.Length - 1));

            TestContext.Out.WriteLine($"AddRange(50k) took {sw.ElapsedMilliseconds} ms");
        }
    }
}
