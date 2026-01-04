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

namespace Eppie.App.Helpers
{
    [Flags]
    internal enum Platform : uint
    {
        Unknown = 0x0,

        UWP = 0x1,
        Android = 0x2,
        iOS = 0x4,
        Catalyst = 0x8,
        tvOS = 0x10,
        WebAssembly = 0x20,
        Desktop = 0x40,
        WinUI = 0x80,

        // Groups
        Apple = 0x20000000,
        Uno = 0x10000000,
    }

    internal class PlatformTools
    {

#if WINDOWS_UWP
        static public Platform PlatformInfo = Platform.UWP;
#elif __UNO__ || HAS_UNO

#if __ANDROID__
        static public Platform PlatformInfo = Platform.Uno | Platform.Android;
#elif __IOS__
        static public Platform PlatformInfo = Platform.Uno | Platform.Apple | Platform.iOS;
#elif __TVOS__
        static public Platform PlatformInfo = Platform.Uno | Platform.Apple | Platform.tvOS;
#elif __MACCATALYST__
        static public Platform PlatformInfo = Platform.Uno | Platform.Apple | Platform.Catalyst;
#elif __WASM__ || HAS_UNO_WASM
        static public Platform PlatformInfo = Platform.Uno | Platform.WebAssembly;
#elif __UNO_SKIA__ || HAS_UNO_SKIA
        static public Platform PlatformInfo = Platform.Uno | Platform.Desktop;
#endif

#else
        static public Platform PlatformInfo = Platform.WinUI;
#endif

        static public bool IsUno = (PlatformInfo & Platform.Uno) != 0;
        static public bool IsApple = (PlatformInfo & Platform.Apple) != 0;

        static public Platform PlatformGroupMask = Platform.Apple | Platform.Uno;
        static public Platform PlatformMask = (Platform)0xffffffff ^ PlatformGroupMask;

        static public Platform CurrentPlatform = PlatformInfo & PlatformMask;
    }
}
