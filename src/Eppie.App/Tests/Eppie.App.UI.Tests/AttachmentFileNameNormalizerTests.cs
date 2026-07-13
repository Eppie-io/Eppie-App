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

using Eppie.App.Common;
using NUnit.Framework;

namespace Eppie.App.UI.Tests.Services
{
    [TestFixture]
    public class AttachmentFileNameNormalizerTests
    {
        [TestCase(null, "attachment")]
        [TestCase("", "attachment")]
        [TestCase(" ", "attachment")]
        [TestCase("   ", "attachment")]
        [TestCase("\t", "attachment")]
        [TestCase("\r\n", "attachment")]
        [TestCase("report", "report")]
        [TestCase("report.pdf", "report.pdf")]
        [TestCase("archive.tar.gz", "archive.tar.gz")]
        [TestCase("file.", "file")]
        [TestCase("file..", "file")]
        [TestCase(" report .pdf ", "report.pdf")]
        public void NormalizeReturnsExpectedValueForGeneralInputs(string? fileName, string expected)
        {
            var normalized = AttachmentFileNameNormalizer.Normalize(fileName!);

            Assert.That(normalized, Is.EqualTo(expected));
        }

        [TestCase("folder\\report.pdf", "report.pdf")]
        [TestCase("folder\\nested\\report.pdf", "report.pdf")]
        [TestCase("C:\\temp\\report.pdf", "report.pdf")]
        [TestCase("folder/report.pdf", "report.pdf")]
        [TestCase("folder/sub/report.pdf", "report.pdf")]
        [TestCase("./report.pdf", "report.pdf")]
        [TestCase("../report.pdf", "report.pdf")]
        [TestCase("folder\\report", "report")]
        [TestCase("folder/sub/archive.tar.gz", "archive.tar.gz")]
        [TestCase("C:/Temp Folder/monthly report.pdf", "monthly report.pdf")]
        public void NormalizeWhenPathProvidedReturnsFileNameOnly(string fileName, string expected)
        {
            var normalized = AttachmentFileNameNormalizer.Normalize(fileName);

            Assert.That(normalized, Is.EqualTo(expected));
        }

        [TestCase(".txt", "attachment.txt")]
        [TestCase(".pdf", "attachment.pdf")]
        [TestCase(".json", "attachment.json")]
        [TestCase(".xml", "attachment.xml")]
        [TestCase(".zip", "attachment.zip")]
        [TestCase(".gz", "attachment.gz")]
        [TestCase(".png", "attachment.png")]
        [TestCase(".jpg", "attachment.jpg")]
        [TestCase(".docx", "attachment.docx")]
        [TestCase(".tar", "attachment.tar")]
        public void NormalizeWhenBaseNameMissingPreservesExtensionWithDefaultName(string fileName, string expected)
        {
            var normalized = AttachmentFileNameNormalizer.Normalize(fileName);

            Assert.That(normalized, Is.EqualTo(expected));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void NormalizeWhenInvalidCharactersPresentReplacesThemAndPreservesExtension(int invalidCharIndex)
        {
            var invalidFileNameChar = InvalidFileNameTestChars[invalidCharIndex % InvalidFileNameTestChars.Length];
            var fileName = $"re{invalidFileNameChar}port.p{invalidFileNameChar}df";

            var normalized = AttachmentFileNameNormalizer.Normalize(fileName);

            Assert.That(normalized, Is.EqualTo("re_port.p_df"));
        }

        private static readonly char[] InvalidFileNameTestChars = Path.GetInvalidFileNameChars()
            .Where(ch => ch != '.' && ch != Path.DirectorySeparatorChar && ch != Path.AltDirectorySeparatorChar)
            .Distinct()
            .DefaultIfEmpty('\0')
            .ToArray();
    }
}
