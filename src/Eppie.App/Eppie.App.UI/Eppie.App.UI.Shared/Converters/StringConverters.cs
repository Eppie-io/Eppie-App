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
using Tuvi.App.ViewModels.Services;
using Eppie.App.UI.Resources;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

// ToDo: Change namespace
namespace Tuvi.App.Converters
{
    public abstract class ValueToStringConverter<T> : IValueConverter
    {
        public T DefaultValue { get; set; }

        public string DefaultString { get; set; }

        protected ValueToStringConverter()
        {
            DefaultValue = default(T);
            DefaultString = "";
        }

        public virtual string Convert(T value)
        {
            return value?.ToString();
        }
        public abstract T ConvertBack(string value);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null ? Convert((T)value) : DefaultString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value != null ? ConvertBack((string)value) : DefaultValue;
        }
    }

    public class IntToStringConverter : ValueToStringConverter<int>
    {
        public override int ConvertBack(string value)
        {
            int result = DefaultValue;
            if (!Int32.TryParse(value, out result))
            {
                result = DefaultValue;
            }

            return result;
        }
    }

    public sealed class AppScaleToStringConverter : IValueConverter
    {
        private static readonly StringProvider _loader = StringProvider.GetInstance();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AppScale scale)
            {
                switch (scale)
                {
                    case AppScale.SystemDefault:
                        return _loader.GetString("UIScaleOptionSystemDefault");
                    case AppScale.Scale100:
                        return "100%";
                    case AppScale.Scale150:
                        return "150%";
                    case AppScale.Scale200:
                        return "200%";
                    case AppScale.Scale250:
                        return "250%";
                    case AppScale.Scale300:
                        return "300%";
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
