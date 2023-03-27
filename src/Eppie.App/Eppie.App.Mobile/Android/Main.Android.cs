// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie(https://eppie.io)                                     //
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

using Android.Runtime;
using Com.Nostra13.Universalimageloader.Core;
using Microsoft.UI.Xaml.Media;

namespace Eppie.App.Droid
{
    [global::Android.App.ApplicationAttribute(
        Label = "@string/ApplicationName",
        Icon = "@mipmap/iconapp",
        LargeHeap = true,
        HardwareAccelerated = true,
        Theme = "@style/AppTheme"
    )]
    public class Application : Microsoft.UI.Xaml.NativeApplication
    {
        public Application(IntPtr javaReference, JniHandleOwnership transfer)
            : base(() => new AppHead(), javaReference, transfer)
        {
            ConfigureUniversalImageLoader();
        }

        private static void ConfigureUniversalImageLoader()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            // ImageLoader.Instance.Init(config) calls 'global::System.GC.KeepAlive(configuration)'

            // Create global configuration and initialize ImageLoader with this config
            ImageLoaderConfiguration config = new ImageLoaderConfiguration
                .Builder(Context)
                .Build();

#pragma warning restore CA2000 // Dispose objects before losing scope

            ImageLoader.Instance.Init(config);

            ImageSource.DefaultImageLoader = ImageLoader.Instance.LoadImageAsync;
        }
    }
}
