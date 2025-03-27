using System;
using Eppie.App.UI.Resources;
using Windows.UI.Text;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
#endif

// ToDo: Change namespace
namespace Tuvi.App.Converters
{
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return FalseValue;

            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class BoolToBrushConverter : BoolToValueConverter<Brush> { }
    public class BoolToVisibilityConverter : BoolToValueConverter<Visibility> { }
    public class BoolToStringConverter : BoolToValueConverter<String> { }
    public class BoolToDoubleConverter : BoolToValueConverter<double> { }
    public class BoolToIntConverter : BoolToValueConverter<int> { }
    public class BoolConverter : BoolToValueConverter<bool> { }
    public class BoolToFontWeightConverter : BoolToValueConverter<FontWeight>
    {
        public BoolToFontWeightConverter()
        {
            TrueValue = FontWeights.ExtraBold;
            FalseValue = FontWeights.Normal;
        }
    }
    public class BoolToHorizontalAlignConverter : BoolToValueConverter<HorizontalAlignment> { }
    public class BoolToThicknessConverter : BoolToValueConverter<Thickness> { }

    public class BoolToResourceConverter : BoolToStringConverter
    {
        private StringProvider _stringProvider;
        private string _resource;
        public String Resource
        {
            get { return _resource; }
            set
            {
                _resource = value;
                _stringProvider = null;
                FalseValue = null;
                TrueValue = null;
                Update();
            }
        }

        private string _falseKey;
        public String FalseKey
        {
            get { return _falseKey; }
            set
            {
                _falseKey = value;
                FalseValue = null;
                Update();
            }
        }

        private string _trueKey;
        public String TrueKey
        {
            get { return _trueKey; }
            set
            {
                _trueKey = value;
                TrueValue = null;
                Update();
            }
        }

        private void UpdateResourceLoader()
        {
            if (_stringProvider != null) return;

            if (string.IsNullOrWhiteSpace(Resource))
            {
                _stringProvider = StringProvider.GetInstance();
            }
            else
            {
                _stringProvider = StringProvider.GetInstance(Resource);
            }
        }

        private void Update()
        {
            UpdateResourceLoader();

            if (TrueKey != null && TrueValue == null)
            {
                TrueValue = _stringProvider?.GetString(TrueKey);
            }

            if (FalseKey != null && FalseValue == null)
            {
                FalseValue = _stringProvider?.GetString(FalseKey);
            }
        }
    }

    public class BoolToSelectionModeConverter : BoolToValueConverter<ListViewSelectionMode> { }
}
