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

using Eppie.App.ViewModels.Services;
using NUnit.Framework;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    [SetCulture("en-US")]
    public sealed class AttachmentSizeTests
    {
        [TestCase(0, "0 B")]
        [TestCase(1, "1 B")]
        [TestCase(1023, "1023 B")]
        [TestCase(1024, "1 KB")]
        [TestCase(1536, "1.5 KB")]
        [TestCase(1048575, "1024 KB")]
        [TestCase(1048576, "1 MB")]
        public void IndividualAttachmentSizesKeepExistingFormatting(int bytes, string expected)
        {
            var attachment = new DownloadableAttachment { Data = new byte[bytes] };

            Assert.That(attachment.FileSize, Is.EqualTo(expected));
        }

        [TestCase(0, 0, "Attachments (0 B)")]
        [TestCase(512, 0, "Attachments (512 B)")]
        [TestCase(512, 512, "Attachments (1 KB)")]
        [TestCase(1024, 512, "Attachments (1.5 KB)")]
        [TestCase(524288, 524288, "Attachments (1 MB)")]
        public void TotalIncludesAllAttachmentsIncludingEmptyAndIdenticalFiles(int firstSize, int secondSize, string expected)
        {
            var vm = CreateViewModel();
            vm.Attachments.Add(new Attachment { FileName = "same.bin", Data = new byte[firstSize] });
            vm.Attachments.Add(new Attachment { FileName = "same.bin", Data = new byte[secondSize] });

            Assert.That(vm.AttachmentsHeader, Is.EqualTo(expected));
        }

        [Test]
        public void SingleAttachmentHasATotalSize()
        {
            var vm = CreateViewModel();
            vm.Attachments.Add(new Attachment { Data = new byte[1536] });

            Assert.That(vm.AttachmentsHeader, Is.EqualTo("Attachments (1.5 KB)"));
        }

        [Test]
        public void NoAttachmentsHaveNoHeader()
        {
            var vm = CreateViewModel();

            Assert.That(vm.AttachmentsHeader, Is.Empty);
        }

        [Test]
        public void TotalDoesNotOverflowAtTwoGigabytes()
        {
            var vm = CreateViewModel();
            var data = new byte[1024 * 1024];
            for (int index = 0; index < 2048; index++)
            {
                vm.Attachments.Add(new Attachment { Data = data });
            }

            Assert.That(vm.AttachmentsHeader, Is.EqualTo("Attachments (2 GB)"));
        }

        [Test]
        [SetCulture("fr-FR")]
        public void SizeFormattingUsesTheCurrentCulture()
        {
            var vm = CreateViewModel();
            var attachment = new DownloadableAttachment { Data = new byte[1536] };
            vm.Attachments.Add(attachment);

            Assert.Multiple(() =>
            {
                Assert.That(attachment.FileSize, Is.EqualTo("1,5 KB"));
                Assert.That(vm.AttachmentsHeader, Is.EqualTo("Attachments (1,5 KB)"));
            });
        }

        [Test]
        public void LocalizedTemplateControlsThePositionOfTheSize()
        {
            var vm = CreateViewModel("{0} in attachments");
            vm.Attachments.Add(new Attachment { Data = new byte[1024] });

            Assert.That(vm.AttachmentsHeader, Is.EqualTo("1 KB in attachments"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void HeaderIsUpdatedWhenAttachmentsAreAddedRemovedOrCleared(bool isComposer)
        {
            BaseMessageViewModel vm = isComposer ? new ComposeMessagePageViewModel() : new MessagePageViewModel();
            vm.SetLocalizationService(new AttachmentLocalizationService("{0} in attachments"));
            var totals = new List<string>();
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(BaseMessageViewModel.AttachmentsHeader))
                {
                    totals.Add(vm.AttachmentsHeader);
                }
            };
            var selected = new RemovableAttachment { FileName = "selected.bin", Data = new byte[1536] };

            Assert.That(vm.AttachmentsHeader, Is.Empty);

            vm.Attachments.Add(selected);
            vm.Attachments.Add(new Attachment { FileName = "other.bin", Data = new byte[512] });
            vm.Attachments.Remove(selected);
            vm.Attachments.Clear();

            Assert.That(totals, Is.EqualTo(new[] { "1.5 KB in attachments", "2 KB in attachments", "512 B in attachments", string.Empty }));
        }

        private static MessagePageViewModel CreateViewModel(string template = "Attachments ({0})")
        {
            var vm = new MessagePageViewModel();
            vm.SetLocalizationService(new AttachmentLocalizationService(template));
            return vm;
        }

        private sealed class AttachmentLocalizationService : ILocalizationService
        {
            private readonly string _template;

            public AttachmentLocalizationService(string template)
            {
                _template = template;
            }

            public IReadOnlyList<string> ManifestLanguages { get; } = Array.Empty<string>();

            public string GetString(string resource)
            {
                Assert.That(resource, Is.EqualTo("AttachmentsHeader"));
                return _template;
            }
        }
    }
}
