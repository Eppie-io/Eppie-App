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
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public class ExternalContentDeciderTests
    {
        [Test]
        public void AlwaysAllowAllowsAnyUri()
        {
            // Arrange
            var decider = new ExternalContentDecider(ExternalContentPolicy.AlwaysAllow);

            // Act
            var d1 = decider.Decide("http://example.com/image.png", allowOnce: false);
            var d2 = decider.Decide("https://cdn.example.com/script.js", allowOnce: false);
            var d3 = decider.Decide("", allowOnce: false);

            // Assert
            Assert.That(d1.ShouldBlock, Is.False);
            Assert.That(d2.ShouldBlock, Is.False);
            Assert.That(d3.ShouldBlock, Is.False);
        }

        [Test]
        public void AllowOnceOverridesBlocking()
        {
            // Arrange
            var decider = new ExternalContentDecider(ExternalContentPolicy.Block);

            // Act
            var d = decider.Decide("http://example.com/img.png", allowOnce: true);

            // Assert
            Assert.That(d.ShouldBlock, Is.False);
            Assert.That(d.ShowBanner, Is.False);
        }

        [Test]
        public void EmptyUriBlocksAndBannerForAskEachTime()
        {
            // Arrange
            var ask = new ExternalContentDecider(ExternalContentPolicy.AskEachTime);
            var block = new ExternalContentDecider(ExternalContentPolicy.Block);

            // Act
            var dAsk = ask.Decide("", allowOnce: false);
            var dBlock = block.Decide("", allowOnce: false);

            // Assert
            Assert.That(dAsk.ShouldBlock, Is.True);
            Assert.That(dAsk.ShowBanner, Is.True);

            Assert.That(dBlock.ShouldBlock, Is.True);
            Assert.That(dBlock.ShowBanner, Is.False);
        }

        [TestCase("blob:abc")]
        [TestCase("cid:12345")]
        [TestCase("data:text/html;base64,PGgxPkhUTUw8L2gxPiI=")]
        [TestCase("about:blank")]
        [TestCase("BLOB:something")]
        public void EmbeddedSchemesAreAllowedRegardlessOfPolicy(System.Uri uri)
        {
            // Arrange
            var ask = new ExternalContentDecider(ExternalContentPolicy.AskEachTime);
            var block = new ExternalContentDecider(ExternalContentPolicy.Block);
            var uriString = uri?.ToString() ?? string.Empty;

            // Act
            var dAsk = ask.Decide(uriString, allowOnce: false);
            var dBlock = block.Decide(uriString, allowOnce: false);

            // Assert
            Assert.That(dAsk.ShouldBlock, Is.False);
            Assert.That(dAsk.ShowBanner, Is.False);
            Assert.That(dBlock.ShouldBlock, Is.False);
            Assert.That(dBlock.ShowBanner, Is.False);
        }

        [Test]
        public void ExternalHttpUriBlocksWithBannerOnlyForAskEachTime()
        {
            // Arrange
            var ask = new ExternalContentDecider(ExternalContentPolicy.AskEachTime);
            var block = new ExternalContentDecider(ExternalContentPolicy.Block);

            // Act
            var dAsk = ask.Decide("http://example.com/content.css", allowOnce: false);
            var dBlock = block.Decide("https://example.com/content.css", allowOnce: false);

            // Assert
            Assert.That(dAsk.ShouldBlock, Is.True);
            Assert.That(dAsk.ShowBanner, Is.True);
            Assert.That(dBlock.ShouldBlock, Is.True);
            Assert.That(dBlock.ShowBanner, Is.False);
        }
    }
}
