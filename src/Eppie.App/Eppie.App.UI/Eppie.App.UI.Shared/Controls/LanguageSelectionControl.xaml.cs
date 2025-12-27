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
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Eppie.App.UI.Resources;
using System.Diagnostics.CodeAnalysis;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
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

    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class LanguageSelectionControl : UserControl
    {
        private static readonly StringProvider StringProvider = StringProvider.GetInstance();

        public IReadOnlyList<string> ManifestLanguages
        {
            get { return (IReadOnlyList<string>)GetValue(ManifestLanguagesProperty); }
            set { SetValue(ManifestLanguagesProperty, value); }
        }

        public static readonly DependencyProperty ManifestLanguagesProperty =
            DependencyProperty.Register(nameof(ManifestLanguages), typeof(IReadOnlyList<string>), typeof(LanguageSelectionControl), new PropertyMetadata(null));

        public event EventHandler<string> LanguageChangedHandler;

        private List<ComboBoxValue> _values = new List<ComboBoxValue>();
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
                new ComboBoxValue {DisplayName = StringProvider.GetString("SystemDefaultLanguage"), LanguageTag = string.Empty}
            };

            IEnumerable<CultureInfo> orderedSupportedCultures = ManifestLanguages.Select(lang => new CultureInfo(lang, false)).OrderBy(culture => culture.Name);

            foreach (var culture in orderedSupportedCultures)
            {
                AddCulture(culture);
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

        private void AddCulture(CultureInfo culture)
        {
            _values.Add(new ComboBoxValue { DisplayName = GetDisplayName(culture), LanguageTag = culture.Name });
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

        public static string GetDisplayName(CultureInfo culture)
        {
            return culture.TextInfo.ToTitleCase(culture.NativeName);
        }

    }
}
