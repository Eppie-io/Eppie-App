﻿// ---------------------------------------------------------------------------- //
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
using System.Runtime.InteropServices.WindowsRuntime;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
#else 
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace Tuvi.App.Converters
{
    public class ImageInfoToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is ImageInfo imageInfo && !imageInfo.IsEmpty)
                {
                    WriteableBitmap bitmap = new WriteableBitmap(imageInfo.Width, imageInfo.Height);
                    bitmap.PixelBuffer.AsStream().Write(imageInfo.Bytes, 0, imageInfo.Bytes.Length);
                    return bitmap;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
