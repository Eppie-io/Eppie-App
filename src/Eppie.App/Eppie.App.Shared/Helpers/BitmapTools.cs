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
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Tuvi.App.Shared.Helpers
{
    public static class BitmapTools
    {
        /// <summary>
        /// Gets the cropped pixel data asynchronously.
        /// </summary>
        /// <param name="sourceFile">The storage file with original image.</param>
        /// <param name="requestedWidth">Width of the cropped image.</param>
        /// <param name="requestedHeight">Height of the cropped image.</param>
        /// <returns>Cropped image pixel data in bytes</returns>
        public static async Task<byte[]> GetThumbnailPixelDataAsync(StorageFile sourceFile, uint requestedWidth, uint requestedHeight)
        {
            using (var imageStream = await sourceFile.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(imageStream); // ToDo: warning Uno0001: Windows.Graphics.Imaging.BitmapDecoder.*

                var originalPixelWidth = decoder.PixelWidth;
                var originalPixelHeight = decoder.PixelHeight;

                if (originalPixelWidth > 0 && originalPixelHeight > 0)
                {
                    double widthRatio = (double)requestedWidth / originalPixelWidth;
                    double heightRatio = (double)requestedHeight / originalPixelHeight;
                    double originalRatio = (double)originalPixelWidth / originalPixelHeight;
                    double requestedRatio = (double)requestedWidth / requestedHeight;

                    var aspectWidth = requestedWidth;
                    var aspectHeight = requestedHeight;
                    uint cropX = 0, cropY = 0;
                    if (originalRatio > requestedRatio)
                    {
                        aspectWidth = (uint)(heightRatio * originalPixelWidth);
                        cropX = (aspectWidth - requestedWidth) / 2;
                    }
                    else
                    {
                        aspectHeight = (uint)(widthRatio * originalPixelHeight);
                        cropY = (aspectHeight - requestedHeight) / 2;
                    }

                    return await GetPixelDataAsync(decoder, cropX, cropY, requestedWidth, requestedHeight, aspectWidth, aspectHeight).ConfigureAwait(false);
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Gets the pixel data.
        /// </summary>
        /// <remarks>
        /// If you want to get the pixel data of a scaled image, set the scaledWidth and scaledHeight
        /// of the scaled image.
        /// </remarks>
        /// <param name="decoder">The bitmap decoder.</param>
        /// <param name="startPointX">The X coordinate of the start point.</param>
        /// <param name="startPointY">The Y coordinate of the start point.</param>
        /// <param name="width">The width of the source rect.</param>
        /// <param name="height">The height of the source rect.</param>
        /// <param name="scaledWidth">The desired width.</param>
        /// <param name="scaledHeight">The desired height.</param>
        /// <returns>The image data.</returns>
        private static async Task<byte[]> GetPixelDataAsync(BitmapDecoder decoder, uint startPointX, uint startPointY,
            uint width, uint height, uint scaledWidth, uint scaledHeight)
        {
            var transform = new BitmapTransform(); // ToDo: warning Uno0001: Windows.Graphics.Imaging.BitmapTransform.*
            transform.InterpolationMode = BitmapInterpolationMode.Linear;

            var bounds = new BitmapBounds(); // ToDo: warning Uno0001: Windows.Graphics.Imaging.BitmapBounds.*
            bounds.X = startPointX;
            bounds.Y = startPointY;
            bounds.Height = height;
            bounds.Width = width;
            transform.Bounds = bounds;

            transform.ScaledWidth = scaledWidth;
            transform.ScaledHeight = scaledHeight;

            // Get the cropped pixels within the bounds of transform.
            var pix = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.ColorManageToSRgb);
            var pixels = pix.DetachPixelData(); // ToDo: warning Uno0001: Windows.Graphics.Imaging.PixelDataProvider.DetachPixelData()
            return pixels;
        }
    }
}
