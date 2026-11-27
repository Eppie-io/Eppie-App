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

using System.Runtime.CompilerServices;
using NUnit.Framework;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public sealed class BaseMessageViewModelTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void MessageDoesNotKeepViewModelAlive(bool replaceMessage)
        {
            var message = new MessageInfo(new Message());
            var viewModel = CreateViewModel(message, replaceMessage);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.That(viewModel.IsAlive, Is.False);
            GC.KeepAlive(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateViewModel(MessageInfo message, bool replaceMessage)
        {
            var vm = new BaseMessageViewModel { MessageInfo = message };
            if (replaceMessage)
            {
                vm.MessageInfo = new MessageInfo(new Message());
            }

            return new WeakReference(vm);
        }
    }
}
