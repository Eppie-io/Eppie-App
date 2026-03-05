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
using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Services
{
    public class NavigationService : INavigationService
    {
        protected Frame MainFrame { get; }
        protected Frame ContentFrame { get; private set; }
        protected string PageTypePrefix { get; }

        public NavigationService(Frame mainFrame, string pageTypePrefix)
        {
            MainFrame = mainFrame;
            PageTypePrefix = pageTypePrefix;
        }

        public void SetContentFrame(Frame contentFrame)
        {
            ContentFrame = contentFrame;
        }

        private Frame GetGoBackFrame()
        {
            if (ContentFrame != null && ContentFrame.IsLoaded && ContentFrame.CanGoBack)
            {
                return ContentFrame;
            }

            return MainFrame;
        }

        public bool CanGoBack()
        {
            var frame = GetGoBackFrame();
            return frame != null && frame.CanGoBack;
        }

        public void GoBack()
        {
            var frame = GetGoBackFrame();
            if (frame != null && frame.CanGoBack)
            {
                frame.GoBack();
            }
        }

        [SuppressMessage("ILLink", "IL2057:Unrecognized value passed to the parameter 'typeName' of method 'System.Type.GetType(System.String)'. It's not possible to guarantee the availability of the target type.", Justification = "Pages are located in the same assembly; resolving by name at runtime is intended and safe in this app.")]
        // TODO: Need to change Navigation mechanism to avoid using Type.GetType
        public void Navigate(string pageKey, object parameter = null)
        {
            string pageTypeName = GetPageTypeNameFromKey(pageKey);
            Type pageType = Type.GetType(pageTypeName);
            if (pageType != null)
            {
                MainFrame?.Navigate(pageType, parameter);
            }
        }

        [SuppressMessage("ILLink", "IL2057:Unrecognized value passed to the parameter 'typeName' of method 'System.Type.GetType(System.String)'. It's not possible to guarantee the availability of the target type.", Justification = "Pages are located in the same assembly; resolving by name at runtime is intended and safe in this app.")]
        public void NavigateContent(string pageKey, object parameter = null)
        {
            var frame = ContentFrame ?? MainFrame;
            string pageTypeName = GetPageTypeNameFromKey(pageKey);
            Type pageType = Type.GetType(pageTypeName);
            if (pageType != null)
            {
                frame?.Navigate(pageType, parameter);
            }
        }

        public void ExitApplication()
        {
            Application.Current.Exit(); // ToDo: Uno0001
        }

        protected string GetPageTypeNameFromKey(string pageKey)
        {
            if (pageKey == null)
            {
                throw new ArgumentNullException(nameof(pageKey));
            }

            const string ending = "viewmodel";

            if (pageKey.ToLower(CultureInfo.InvariantCulture).EndsWith(ending, StringComparison.Ordinal))
            {
                int indexToRemoveFrom = pageKey.Length - ending.Length;
                return PageTypePrefix + pageKey.Remove(indexToRemoveFrom);
            }
            else
            {
                return PageTypePrefix + pageKey;
            }
        }
    }
}
