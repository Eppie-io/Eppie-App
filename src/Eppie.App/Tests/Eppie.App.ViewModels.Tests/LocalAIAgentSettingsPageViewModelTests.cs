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
using Eppie.App.ViewModels.Tests.TestDoubles;
using NUnit.Framework;
using Tuvi.App.ViewModels;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests
{
    [TestFixture]
    public sealed class LocalAIAgentSettingsPageViewModelTests
    {
        private sealed class TestAIService : IAIService
        {
            public event EventHandler<LocalAIAgentEventArgs>? AgentAdded
            {
                add { }
                remove { }
            }

            public event EventHandler<LocalAIAgentEventArgs>? AgentDeleted
            {
                add { }
                remove { }
            }

            public event EventHandler<LocalAIAgentEventArgs>? AgentUpdated
            {
                add { }
                remove { }
            }

            public event EventHandler<ExceptionEventArgs>? ExceptionOccurred
            {
                add { }
                remove { }
            }

            public bool IsModelImported { get; set; }

            public int IsLocalAIModelImportedCalls { get; private set; }

            public Task<string> ProcessTextAsync(LocalAIAgent agent, string text, CancellationToken cancellationToken, Action<string>? onTextUpdate = null)
                => throw new NotImplementedException();

            public bool IsAvailable() => true;

            public Task<bool> IsEnabledAsync() => Task.FromResult(IsModelImported);

            public Task<bool> IsLocalAIModelImportedAsync()
            {
                IsLocalAIModelImportedCalls++;
                return Task.FromResult(IsModelImported);
            }

            public Task DeleteModelAsync()
            {
                IsModelImported = false;
                return Task.CompletedTask;
            }

            public Task ImportModelAsync()
            {
                IsModelImported = true;
                return Task.CompletedTask;
            }

            public Task AddAgentAsync(LocalAIAgent agent)
            {
                return Task.CompletedTask;
            }

            public Task RemoveAgentAsync(LocalAIAgent agent)
            {
                return Task.CompletedTask;
            }

            public Task<IReadOnlyList<LocalAIAgent>> GetAgentsAsync() => Task.FromResult<IReadOnlyList<LocalAIAgent>>(Array.Empty<LocalAIAgent>());

            public Task UpdateAgentAsync(LocalAIAgent agent)
            {
                return Task.CompletedTask;
            }
        }

        private static LocalAIAgentSettingsPageViewModel CreateViewModel(TestAIService aiService)
        {
            var vm = new LocalAIAgentSettingsPageViewModel();
            vm.SetAIServiceProvider(() => aiService);
            vm.SetCoreProvider(() => new FakeTuviMail());
            vm.SetLocalizationService(new TestLocalizationService());
            vm.SetErrorHandler(new RecorderErrorHandler());
            return vm;
        }

        private static async Task WaitForAsync(Func<bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            for (var i = 0; i < 50; i++)
            {
                if (predicate())
                {
                    return;
                }

                await Task.Delay(10).ConfigureAwait(false);
            }

            Assert.Fail("Condition was not met in time.");
        }

        [Test]
        public async Task OnNavigatedToWhenModelIsNotImportedDisablesApply()
        {
            var aiService = new TestAIService { IsModelImported = false };
            using var vm = CreateViewModel(aiService);

            vm.OnNavigatedTo(new object());
            await WaitForAsync(() => aiService.IsLocalAIModelImportedCalls > 0).ConfigureAwait(false);

            Assert.That(vm.CanApply, Is.False);
            Assert.That(vm.ApplySettingsCommand.CanExecute(null), Is.False);
        }

        [Test]
        public async Task OnNavigatedToWhenModelIsImportedEnablesApply()
        {
            var aiService = new TestAIService { IsModelImported = true };
            using var vm = CreateViewModel(aiService);

            vm.OnNavigatedTo(new object());
            await WaitForAsync(() => aiService.IsLocalAIModelImportedCalls > 0 && vm.CanApply).ConfigureAwait(false);

            Assert.That(vm.CanApply, Is.True);
            Assert.That(vm.ApplySettingsCommand.CanExecute(null), Is.True);
        }
    }
}
