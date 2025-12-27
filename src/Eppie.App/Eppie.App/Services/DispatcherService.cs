// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
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

using Microsoft.UI.Dispatching;
using Tuvi.App.ViewModels.Services;

namespace Eppie.App.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public async Task RunAsync(Action action)
        {
            if (_dispatcherQueue.HasThreadAccess)
            {
                action();
            }
            else
            {
                var completionSource = new TaskCompletionSource<bool>();
                _dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        action();
                        completionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        completionSource.SetException(ex);
                    }
                });
                await completionSource.Task;
            }
        }
    }
}
