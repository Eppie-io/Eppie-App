using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
#endif

// ToDo: Change namespace
namespace Tuvi.App.Converters
{
    [ContentProperty(Name = "Converters")]
    public class ConverterChain : IValueConverter
    {
        public ICollection<IValueConverter> Converters { get; }

        public ConverterChain()
        {
            Converters = new List<IValueConverter>();
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            foreach (var converter in Converters)
            {
                value = converter.Convert(value, targetType, parameter, language);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
