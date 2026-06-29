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
    public enum AddressPresenterTooltipMode
    {
        Formatted,
        Name,
        Address,
    }

    public class AddressPresenterTooltipSource : ITooltipSource<AddressPresenter>, IFormattedTooltipSource
    {
        // {0} = DisplayName, {1} = Address
        private static readonly string DefaultTooltipFormat = "{0} <{1}>";

        // The tooltip is active if the text is trimmed or
        // if the tooltip displays more than just the name when the presenter is in compact mode.
        public bool IsActive => _addressBlock?.IsTextTrimmed == true ||
                                _nameBlock?.IsTextTrimmed == true ||
                                (Source?.Mode == AddressPresenterMode.Compact && Mode != AddressPresenterTooltipMode.Name);
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

        internal AddressPresenterTooltipMode Mode { get; set; } = AddressPresenterTooltipMode.Formatted;
        internal string Format { get; set; }

        private long? _modeToken;
        private long? _nameToken;
        private long? _addressToken;

        private TextBlock _nameBlock;
        private TextBlock _addressBlock;

        public string GetFormattedTooltipText(string format)
        {
            switch (Mode)
            {
                case AddressPresenterTooltipMode.Name:
                    return Source?.DisplayName;
                case AddressPresenterTooltipMode.Address:
                    return Source?.Address;
                default:
                    return string.Format(CultureInfo.CurrentCulture, format ?? Format ?? DefaultTooltipFormat, Source?.DisplayName, Source?.Address);
            }
        }

        private void Register()
        {
            _modeToken = _source?.RegisterPropertyChangedCallback(AddressPresenter.ModeProperty, OnPropertyChanged);
            _nameToken = _source?.RegisterPropertyChangedCallback(AddressPresenter.DisplayNameProperty, OnPropertyChanged);
            _addressToken = _source?.RegisterPropertyChangedCallback(AddressPresenter.AddressProperty, OnPropertyChanged);

            _nameBlock = GetTextBlock(AddressPresenter.DisplayNameElementName);
            if (_nameBlock != null)
            {
                _nameBlock.IsTextTrimmedChanged += OnIsTextTrimmedChanged;
            }

            _addressBlock = GetTextBlock(AddressPresenter.AddressElementName);
            if (_addressBlock != null)
            {
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
                _source?.UnregisterPropertyChangedCallback(AddressPresenter.DisplayNameProperty, _nameToken.Value);
                _nameToken = null;
            }

            if (_addressToken.HasValue)
            {
                _source?.UnregisterPropertyChangedCallback(AddressPresenter.AddressProperty, _addressToken.Value);
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
    {
        public AddressPresenterTooltipMode Mode
        {
            get { return (AddressPresenterTooltipMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(AddressPresenterTooltipMode), typeof(AddressPresenterTooltipBehavior), new PropertyMetadata(AddressPresenterTooltipMode.Formatted, OnPropertyChanged));


        // {0} = DisplayName, {1} = Address
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register(nameof(Format), typeof(string), typeof(AddressPresenterTooltipBehavior), new PropertyMetadata(null, OnPropertyChanged));


        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is AddressPresenterTooltipBehavior behavior)
            {
                behavior.OnChanged();
            }
        }

        protected override void InitializeTooltipSource(AddressPresenterTooltipSource tooltipSource)
        {
            base.InitializeTooltipSource(tooltipSource);

            tooltipSource.Mode = Mode;
            tooltipSource.Format = Format;
        }

        protected override void UpdateTooltipSource(AddressPresenterTooltipSource tooltipSource)
        {
            base.UpdateTooltipSource(tooltipSource);

            tooltipSource.Mode = Mode;
            tooltipSource.Format = Format;
        }
    }
}
