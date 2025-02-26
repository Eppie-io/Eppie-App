using System;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

// ToDo: Change namespace
namespace Tuvi.App.Converters
{
    public class JoinConverter<TObject, TParam> : IValueConverter
    {
        public TParam Parameter { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TObject obj)
            {
                return new Tuple<TObject, TParam>(obj, Parameter);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
