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

using Tuvi.App.ViewModels.Services;

namespace Eppie.App.ViewModels.Tests.TestDoubles
{
    internal sealed class RecorderErrorHandler : IErrorHandler
    {
        public readonly List<Exception> Errors = new List<Exception>();
        public event EventHandler<Exception>? ErrorRecorded;
        public void SetMessageService(IMessageService messageService) { }
        public void OnError(Exception ex, bool silent = false)
        {
            if (ex != null)
            {
                Errors.Add(ex);
                ErrorRecorded?.Invoke(this, ex);
            }
        }
    }
}
