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

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Logging;

namespace Tuvi.App.Shared.Services
{
    public class ErrorHandler : IErrorHandler
    {
        private IMessageService MessageService { get; set; }

        private readonly DispatcherService _dispatcher = new DispatcherService();
        protected DispatcherService Dispatcher { get => _dispatcher; }

        public ErrorHandler()
        {
        }

        public void SetMessageService(IMessageService messageService)
        {
            MessageService = messageService;
        }


        public async void OnError(Exception ex, bool silent)
        {
            LoggingExtension.Log(this).LogError(ex, "An error has occurred");
            try
            {
                await Dispatcher.RunAsync(async () =>
                {
                    try
                    {
                        await OnErrorAsync(ex, silent).ConfigureAwait(true);
                    }
                    catch
                    {
                    }
                });
            }
            catch
            {
            }
        }

        private async Task OnErrorAsync(Exception e, bool silent)
        {
            if (e is OperationCanceledException)
            {
                return;
            }

            if (!silent && MessageService != null)
            {
                await MessageService.ShowErrorMessageAsync(e).ConfigureAwait(true);
            }
        }
    }
}
