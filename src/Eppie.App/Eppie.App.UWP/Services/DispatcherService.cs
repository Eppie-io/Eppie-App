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

using System;
using System.Threading.Tasks;
using Tuvi.App.ViewModels.Services;
using Windows.UI.Core;

namespace Eppie.App.Services
{
    public class DispatcherService : IDispatcherService
    {
        private readonly CoreDispatcher _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

        public async Task RunAsync(Action action)
        {
            if (_dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
            }
        }
    }
}
