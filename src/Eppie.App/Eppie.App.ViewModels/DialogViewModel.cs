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

namespace Tuvi.App.ViewModels
{

    public class DialogViewModel : BaseViewModel
    {
        public enum Result
        {
            None,
            Primary,
            Secondary,
        }

        public virtual void OnOpened()
        { }

        public virtual void OnClosing(Result result, out bool cancel)
        {
            cancel = false;
        }

        public virtual void OnClosed(Result result)
        { }
    }

    public class DialogViewModel<TData> : DialogViewModel
    {
        public virtual void InitializeData(TData data)
        { }
    }
}
