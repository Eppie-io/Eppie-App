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
using System.Globalization;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#endif

// ToDo: Change namespace
namespace Tuvi.App.Converters
{
    public class TimeSpanToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null ? ((TimeSpan)value).TotalSeconds : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new TimeSpan(0, 0, (int)(double)value);
        }
    }

    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan duration)
            {
                if (duration.Hours > 0)
                {
                    return duration.ToString("h\\:mm\\:ss", CultureInfo.CurrentCulture);
                }
                else
                {
                    return duration.ToString("m\\:ss", CultureInfo.CurrentCulture);
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class TotalSecondsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var converter = new TimeSpanToStringConverter();
            return converter.Convert(new TimeSpan(0, 0, (int)(double)value), targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
