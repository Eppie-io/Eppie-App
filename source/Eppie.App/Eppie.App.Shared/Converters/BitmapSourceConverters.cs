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

namespace Tuvi.App.Shared.Converters
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
