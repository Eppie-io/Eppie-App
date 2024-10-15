using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public class LanguageChangedEventArgs : EventArgs
    {
        public string Language { get; set; }
    }

    public class ComboBoxValue
    {
        public string DisplayName { get; set; }
        public string LanguageTag { get; set; }
    }

    public sealed partial class LanguageSelectionControl : UserControl
    {
        public event EventHandler<string> LanguageChangedHandler;
        private static readonly ResourceLoader Loader = ResourceLoader.GetForCurrentView();

        private List<ComboBoxValue> _values  = new List<ComboBoxValue>();
        private bool _selectionInited;
        private string _selectedValue;

        public LanguageSelectionControl()
        {
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            _values = new List<ComboBoxValue>
            {
                new ComboBoxValue {DisplayName = Loader.GetString("SystemDefaultLanguage"), LanguageTag = ""}
            };

            IEnumerable<Language> orderedSupportedLanguages =
                ApplicationLanguages.ManifestLanguages.Select(lang => new Language(lang)).OrderBy(lang => lang.DisplayName);

            foreach (var lang in orderedSupportedLanguages)
            {
                AddLanguage(lang);
            }

            LanguageComboBox.ItemsSource = _values;
            InitSelectionImpl(_selectedValue);
        }

        public void InitSelection(string selectedLanguage)
        {
            _selectedValue = selectedLanguage;
            InitSelectionImpl(selectedLanguage);
        }

        private void InitSelectionImpl(string selectedLanguage)
        {
            if (_values.Count > 0)
            {
                _selectionInited = false;
                LanguageComboBox.SelectedIndex = _values.FindIndex(value => (value.LanguageTag == selectedLanguage));
                _selectionInited = true;
            }
        }

        private void AddLanguage(Language lang)
        {
            _values.Add(new ComboBoxValue { DisplayName = lang.DisplayName, LanguageTag = lang.LanguageTag });
        }
        
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (_selectionInited && combo != null)
            {
                var val = combo.SelectedValue;
                if (val != null)
                {
                    LanguageChangedHandler?.Invoke(this, val.ToString());
                }
            }
        }
    }
}
