// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.WinUI;
using Eppie.App.UI.Controls;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class AddressPresenterTooltipSource : ITooltipSource<AddressPresenter>, IFormattedTooltipSource
    {
        // {0} = DisplayName, {1} = Address
        private static readonly string DefaultTooltipFormat = "{0} <{1}>";

        // The tooltip should be active if the presenter is in compact mode or if the text is trimmed
        public bool IsActive => Source?.Mode == AddressPresenterMode.Compact
                                             || _addressBlock?.IsTextTrimmed == true
                                             || _nameBlock?.IsTextTrimmed == true;
        public string Text => null;

        public event EventHandler TooltipChanged;

        private AddressPresenter _source;
        public AddressPresenter Source
        {
            get { return _source; }
            set
            {
                if (_source != null)
                {
                    Unregister();
                }

                _source = value;

                if (_source != null)
                {
                    Register();
                    UpdateTooltip();
                }
            }
        }

        private long? _modeToken;
        private long? _nameToken;
        private long? _addressToken;

        private TextBlock _nameBlock;
        private TextBlock _addressBlock;

        public string GetFormattedTooltipText(string format)
        {
            try
            {
                return string.Format(CultureInfo.CurrentCulture, format ?? DefaultTooltipFormat, Source?.DisplayName, Source?.Address);
            }
            catch (FormatException ex)
            {
                if (Debugger.IsAttached)
                {
                    Debug.Assert(false, ex.Message);
                }
            }
            return null;
        }

        private void Register()
        {
            _modeToken = _source?.RegisterPropertyChangedCallback(AddressPresenter.ModeProperty, OnPropertyChanged);

            _nameBlock = GetTextBlock(AddressPresenter.DisplayNameElementName);
            if (_nameBlock != null)
            {
                _nameToken = _nameBlock.RegisterPropertyChangedCallback(TextBlock.TextProperty, OnPropertyChanged);
                _nameBlock.IsTextTrimmedChanged += OnIsTextTrimmedChanged;
            }

            _addressBlock = GetTextBlock(AddressPresenter.AddressElementName);
            if (_addressBlock != null)
            {
                _addressToken = _addressBlock.RegisterPropertyChangedCallback(TextBlock.TextProperty, OnPropertyChanged);
                _addressBlock.IsTextTrimmedChanged += OnIsTextTrimmedChanged;
            }
        }

        private void Unregister()
        {
            if (_modeToken.HasValue)
            {
                _source?.UnregisterPropertyChangedCallback(AddressPresenter.ModeProperty, _modeToken.Value);
                _modeToken = null;
            }

            if (_nameToken.HasValue)
            {
                _nameBlock?.UnregisterPropertyChangedCallback(TextBlock.TextProperty, _nameToken.Value);
                _nameToken = null;
            }

            if (_addressToken.HasValue)
            {
                _addressBlock?.UnregisterPropertyChangedCallback(TextBlock.TextProperty, _addressToken.Value);
                _addressToken = null;
            }

            if (_nameBlock != null)
            {
                _nameBlock.IsTextTrimmedChanged -= OnIsTextTrimmedChanged;
                _nameBlock = null;
            }

            if (_addressBlock != null)
            {
                _addressBlock.IsTextTrimmedChanged -= OnIsTextTrimmedChanged;
                _addressBlock = null;
            }
        }

        private void OnIsTextTrimmedChanged(TextBlock sender, IsTextTrimmedChangedEventArgs args)
        {
            UpdateTooltip();
        }

        private void OnPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateTooltip();
        }

        private void UpdateTooltip()
        {
            TooltipChanged?.Invoke(this, EventArgs.Empty);
        }

        private TextBlock GetTextBlock(string elementName)
        {
            // FindChild method can be used on controls that aren't yet connected or rendered in the visual tree.
            return _source?.FindChild<TextBlock>((text) => text.Name?.Equals(elementName, StringComparison.Ordinal) ?? false);
        }
    }

    public class AddressPresenterTooltipBehavior : TooltipBehavior<AddressPresenter, AddressPresenterTooltipSource>
    { }
}
