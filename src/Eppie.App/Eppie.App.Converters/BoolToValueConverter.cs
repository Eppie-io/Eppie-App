using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

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
        private ResourceLoader _resourceLoader;
        private string _resource;
        public String Resource
        {
            get { return _resource; }
            set
            {
                _resource = value;
                _resourceLoader = null;
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
            if (_resourceLoader != null) return;

            if (string.IsNullOrWhiteSpace(Resource))
            {
                _resourceLoader = ResourceLoader.GetForCurrentView();
            }
            else
            {
                _resourceLoader = ResourceLoader.GetForCurrentView(Resource);
            }
        }

        private void Update()
        {
            UpdateResourceLoader();

            if (TrueKey != null && TrueValue == null)
            {
                TrueValue = _resourceLoader.GetString(TrueKey);
            }

            if (FalseKey != null && FalseValue == null)
            {
                FalseValue = _resourceLoader.GetString(FalseKey);
            }
        }
    }

    public class BoolToSelectionModeConverter : BoolToValueConverter<ListViewSelectionMode> { }
}
