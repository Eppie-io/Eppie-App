using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Collections.Generic;
using Windows.UI.Xaml.Markup;

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
