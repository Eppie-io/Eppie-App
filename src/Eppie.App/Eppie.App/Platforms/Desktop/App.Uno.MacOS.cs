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

#if HAS_UNO_SKIA_MACOS || __UNO_SKIA_MACOS__

namespace Eppie.App
{
    public partial class App
    {
        static partial void InstallPlatformMainMenu(string appName)
        {
            if (!OperatingSystem.IsMacOS())
            {
                return;
            }

            Platforms.Desktop.MacOSMainMenuInstaller.InstallIfNeeded(appName);
        }
    }
}

#endif
