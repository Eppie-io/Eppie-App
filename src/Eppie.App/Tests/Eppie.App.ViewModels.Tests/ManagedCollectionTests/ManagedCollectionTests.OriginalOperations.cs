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
        [Category("OriginalOperations")]
        [Category("Filtering")]
        public void ClearWithFilterEmptiesView()
        {
            var filter = new StartsWithFilter { SearchText = "A" };
            var col = new ManagedCollection<string> { SearchFilter = filter };
            col.Add("A1");
            col.Add("B1");
            col.Add("A2");

            Assert.That(col.Count, Is.EqualTo(2));

            col.Clear();

            Assert.That(col.OriginalItems, Is.Empty);
            Assert.That(col, Is.Empty);
        }
    }
}
