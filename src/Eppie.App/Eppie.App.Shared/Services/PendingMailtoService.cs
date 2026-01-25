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
using CommunityToolkit.Mvvm.Messaging;
using Tuvi.App.ViewModels.Messages;
using Tuvi.App.ViewModels.Services;

namespace Eppie.App.Services
{
    public sealed class PendingMailtoService : IPendingMailtoService
    {
        private Uri _pending;

        public void SetPendingMailtoUri(Uri mailtoUri)
        {
            _pending = mailtoUri;
            WeakReferenceMessenger.Default.Send(new MailtoActivationMessage());
        }

        public bool TryDequeuePendingMailtoUri(out Uri mailtoUri)
        {
            mailtoUri = _pending;
            _pending = null;
            return mailtoUri != null;
        }
    }
}
