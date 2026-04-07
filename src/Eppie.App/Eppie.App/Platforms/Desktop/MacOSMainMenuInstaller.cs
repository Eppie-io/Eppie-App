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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Eppie.App.UI.Resources;

namespace Eppie.App.Platforms.Desktop
{
    internal static class MacOSMainMenuInstaller
    {
        private const string ObjectiveCLibrary = "/usr/lib/libobjc.A.dylib";
        private const long NSApplicationActivationPolicyRegular = 0;
        private const ulong CommandModifierMask = 1UL << 20;
        private const ulong OptionModifierMask = 1UL << 19;

        private static bool _isInstalled;

        public static void InstallIfNeeded(string appName)
        {
            if (_isInstalled || !OperatingSystem.IsMacOS())
            {
                return;
            }

            IntPtr sharedApplication = SendIntPtr(Interop.NSApplicationClass, Interop.SharedApplicationSelector);
            if (sharedApplication == IntPtr.Zero)
            {
                return;
            }

            SetActivationPolicy(sharedApplication, NSApplicationActivationPolicyRegular);

            StringProvider strings = StringProvider.GetInstance();

            IntPtr mainMenu = CreateMenu();
            IntPtr appMenuHostItem = CreateMenuItem(appName, IntPtr.Zero, string.Empty);
            AddItem(mainMenu, appMenuHostItem);

            IntPtr appMenu = CreateTitledMenu(appName);
            AddItem(appMenu, CreateMenuItem(strings.GetString("MacOSMenuAboutApp"), Interop.AboutSelector, string.Empty));
            AddItem(appMenu, CreateSeparatorItem());
            AddItem(appMenu, CreateMenuItem(strings.GetString("MacOSMenuHideApp"), Interop.HideSelector, "h"));

            IntPtr hideOthersItem = CreateMenuItem(strings.GetString("MacOSMenuHideOthers"), Interop.HideOthersSelector, "h");
            SetKeyEquivalentModifierMask(hideOthersItem, CommandModifierMask | OptionModifierMask);
            AddItem(appMenu, hideOthersItem);

            AddItem(appMenu, CreateMenuItem(strings.GetString("MacOSMenuShowAll"), Interop.ShowAllSelector, string.Empty));
            AddItem(appMenu, CreateSeparatorItem());
            AddItem(appMenu, CreateMenuItem(strings.GetString("MacOSMenuQuitApp"), Interop.QuitSelector, "q"));
            SetSubmenu(appMenuHostItem, appMenu);

            string fileMenuTitle = strings.GetString("MacOSMenuFile");
            IntPtr fileMenuHostItem = CreateMenuItem(fileMenuTitle, IntPtr.Zero, string.Empty);
            IntPtr fileMenu = CreateTitledMenu(fileMenuTitle);
            AddItem(fileMenu, CreateMenuItem(strings.GetString("MacOSMenuCloseWindow"), Interop.PerformCloseSelector, "w"));
            SetSubmenu(fileMenuHostItem, fileMenu);
            AddItem(mainMenu, fileMenuHostItem);

            SetMainMenu(sharedApplication, mainMenu);
            _isInstalled = true;
        }

        private static IntPtr CreateMenu()
        {
            IntPtr menu = SendIntPtr(Interop.NSMenuClass, Interop.AllocSelector);
            return SendIntPtr(menu, Interop.InitSelector);
        }

        private static IntPtr CreateTitledMenu(string title)
        {
            IntPtr menu = SendIntPtr(Interop.NSMenuClass, Interop.AllocSelector);
            return SendIntPtr(menu, Interop.InitWithTitleSelector, CreateNSString(title));
        }

        private static IntPtr CreateMenuItem(string title, IntPtr actionSelector, string keyEquivalent)
        {
            IntPtr menuItem = SendIntPtr(Interop.NSMenuItemClass, Interop.AllocSelector);
            return SendIntPtr(
                menuItem,
                Interop.InitWithTitleActionKeyEquivalentSelector,
                CreateNSString(title),
                actionSelector,
                CreateNSString(keyEquivalent));
        }

        private static IntPtr CreateSeparatorItem()
        {
            return SendIntPtr(Interop.NSMenuItemClass, Interop.SeparatorItemSelector);
        }

        private static IntPtr CreateNSString(string value)
        {
            IntPtr utf8Text = Marshal.StringToCoTaskMemUTF8(value ?? string.Empty);

            try
            {
                IntPtr nsString = SendIntPtr(Interop.NSStringClass, Interop.AllocSelector);
                return SendIntPtr(nsString, Interop.InitWithUtf8StringSelector, utf8Text);
            }
            finally
            {
                Marshal.FreeCoTaskMem(utf8Text);
            }
        }

        private static void AddItem(IntPtr menu, IntPtr menuItem)
        {
            SendVoid(menu, Interop.AddItemSelector, menuItem);
        }

        private static void SetSubmenu(IntPtr menuItem, IntPtr submenu)
        {
            SendVoid(menuItem, Interop.SetSubmenuSelector, submenu);
        }

        private static void SetMainMenu(IntPtr application, IntPtr mainMenu)
        {
            SendVoid(application, Interop.SetMainMenuSelector, mainMenu);
        }

        private static void SetActivationPolicy(IntPtr application, long activationPolicy)
        {
            SendVoid(application, Interop.SetActivationPolicySelector, activationPolicy);
        }

        private static void SetKeyEquivalentModifierMask(IntPtr menuItem, ulong modifierMask)
        {
            SendVoid(menuItem, Interop.SetKeyEquivalentModifierMaskSelector, modifierMask);
        }

        private static class Interop
        {
            internal static readonly IntPtr NSApplicationClass = ObjcGetClass("NSApplication");
            internal static readonly IntPtr NSStringClass = ObjcGetClass("NSString");
            internal static readonly IntPtr NSMenuClass = ObjcGetClass("NSMenu");
            internal static readonly IntPtr NSMenuItemClass = ObjcGetClass("NSMenuItem");

            internal static readonly IntPtr SharedApplicationSelector = SelRegisterName("sharedApplication");
            internal static readonly IntPtr AllocSelector = SelRegisterName("alloc");
            internal static readonly IntPtr InitSelector = SelRegisterName("init");
            internal static readonly IntPtr InitWithUtf8StringSelector = SelRegisterName("initWithUTF8String:");
            internal static readonly IntPtr InitWithTitleSelector = SelRegisterName("initWithTitle:");
            internal static readonly IntPtr InitWithTitleActionKeyEquivalentSelector = SelRegisterName("initWithTitle:action:keyEquivalent:");
            internal static readonly IntPtr AddItemSelector = SelRegisterName("addItem:");
            internal static readonly IntPtr SetSubmenuSelector = SelRegisterName("setSubmenu:");
            internal static readonly IntPtr SetMainMenuSelector = SelRegisterName("setMainMenu:");
            internal static readonly IntPtr SetActivationPolicySelector = SelRegisterName("setActivationPolicy:");
            internal static readonly IntPtr SeparatorItemSelector = SelRegisterName("separatorItem");
            internal static readonly IntPtr SetKeyEquivalentModifierMaskSelector = SelRegisterName("setKeyEquivalentModifierMask:");

            internal static readonly IntPtr AboutSelector = SelRegisterName("orderFrontStandardAboutPanel:");
            internal static readonly IntPtr HideSelector = SelRegisterName("hide:");
            internal static readonly IntPtr HideOthersSelector = SelRegisterName("hideOtherApplications:");
            internal static readonly IntPtr ShowAllSelector = SelRegisterName("unhideAllApplications:");
            internal static readonly IntPtr QuitSelector = SelRegisterName("terminate:");
            internal static readonly IntPtr PerformCloseSelector = SelRegisterName("performClose:");
        }

        [SuppressMessage("Interoperability", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Objective-C runtime APIs expect UTF-8 C strings.")]
        [DllImport(ObjectiveCLibrary, EntryPoint = "objc_getClass")]
        private static extern IntPtr ObjcGetClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [SuppressMessage("Interoperability", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Objective-C runtime APIs expect UTF-8 C strings.")]
        [DllImport(ObjectiveCLibrary, EntryPoint = "sel_registerName")]
        private static extern IntPtr SelRegisterName([MarshalAs(UnmanagedType.LPUTF8Str)] string selectorName);

        [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr SendIntPtr(IntPtr receiver, IntPtr selector);

        [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, IntPtr arg3);

        [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void SendVoid(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void SendVoid(IntPtr receiver, IntPtr selector, long arg1);

        [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void SendVoid(IntPtr receiver, IntPtr selector, ulong arg1);
    }
}
