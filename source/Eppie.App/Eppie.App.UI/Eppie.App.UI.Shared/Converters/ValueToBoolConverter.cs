using System;
using System.Collections;
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
    public class ValueToBoolConverter<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null ? System.Convert.ToBoolean((T)value, CultureInfo.CurrentCulture) : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class IntToBoolConverter : ValueToBoolConverter<int> { }
    public class DoubleToBoolConverter : ValueToBoolConverter<double> { }
    public class NullableBoolToBoolConverter : ValueToBoolConverter<bool?> { }

    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class CollectionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch(value)
            {
                case IList list:
                    return list.Count > 0;
                case ICollection collection:
                    return collection.Count > 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
