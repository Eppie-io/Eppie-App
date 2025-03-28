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
using Windows.Storage.Pickers;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Eppie.App.UI.Tools
{
    public abstract class CommonBuilder<T>
        where T : new()
    {
        private readonly T _client = new T();
        internal Action<T> PreBuildAction { get; set; }

        public CommonBuilder<T> Configure(Action<T> configurator)
        {
            configurator?.Invoke(_client);
            return this;
        }

        public T Build()
        {
            PreBuildAction?.Invoke(_client);
            return _client;
        }

        protected CommonBuilder()
        { }
    }

    public abstract class ItemPickerBuilder<TPicker, TBuilder> : CommonBuilder<TPicker>
        where TPicker : new()
        where TBuilder : CommonBuilder<TPicker>, new()
    {
#if WINDOWS10_0_19041_0_OR_GREATER
        protected static void InitializePickerWindow(object picker, Window window)
        {
            nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        }
#endif

        public static TBuilder CreateBuilder(Window window)
        {
            TBuilder builder = new TBuilder();

#if WINDOWS10_0_19041_0_OR_GREATER
            builder.PreBuildAction = (picker) => InitializePickerWindow(picker, window);
#endif

            return builder;
        }
    }

    public sealed class FileSavePickerBuilder : ItemPickerBuilder<FileSavePicker, FileSavePickerBuilder>
    { }

    public sealed class FileOpenPickerBuilder : ItemPickerBuilder<FileOpenPicker, FileOpenPickerBuilder>
    { }

    public sealed class FolderPickerBuilder : ItemPickerBuilder<FolderPicker, FolderPickerBuilder>
    { }
}
