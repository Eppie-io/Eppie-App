using System;
using Windows.UI.Xaml.Data;

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
            return value.ToString();
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

}