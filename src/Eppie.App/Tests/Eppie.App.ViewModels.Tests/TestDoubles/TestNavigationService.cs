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
using Tuvi.App.ViewModels.Services;

namespace Eppie.App.ViewModels.Tests.TestDoubles
{
    internal sealed class TestNavigationService : INavigationService
    {
        public string? LastNavigatedPage { get; private set; }
        public object? LastNavigationData { get; private set; }

        public void Navigate(string pageKey, object? data = null)
        {
            LastNavigatedPage = pageKey;
            LastNavigationData = data;
        }

        public bool CanGoBack()
        {
            return false;
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public bool CanGoBackTo(string pageKey)
        {
            _ = pageKey;
            return false;
        }

        public void GoBackTo(string pageKey)
        {
            _ = pageKey;
            throw new NotImplementedException();
        }

        public void GoBackOrNavigate(string pageKey, object? parameter = null)
        {
            Navigate(pageKey, parameter);
        }

        public void GoBackToOrNavigate(string pageKey, object? parameter = null)
        {
            Navigate(pageKey, parameter);
        }

        public void ExitApplication()
        {
            throw new NotImplementedException();
        }

        public void ClearHistory()
        {
            throw new NotImplementedException();
        }
    }
}
