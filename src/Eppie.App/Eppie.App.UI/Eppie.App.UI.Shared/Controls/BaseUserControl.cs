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

using Tuvi.Core.Entities;
using System;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public partial class BaseUserControl : UserControl
    {
        public event EventHandler<ExceptionEventArgs> ExceptionOccurred;

        public virtual void OnError(Exception ex)
        {
            ExceptionOccurred?.Invoke(this, new ExceptionEventArgs(ex));
        }
    }
}
