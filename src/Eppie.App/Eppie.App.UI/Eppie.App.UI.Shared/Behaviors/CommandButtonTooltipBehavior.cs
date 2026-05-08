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
using Eppie.App.UI.Controls;
using Eppie.App.UI.Extensions;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class CommandButtonTooltipSource : ITooltipSource<CommandButton>
    {
        // The tooltip should be active if the button is in compact mode or if the label text is trimmed
        public bool IsActive => Source?.IsCompact == true || _label?.IsTextTrimmed == true;

        public string Text => _label?.Text;

        public event EventHandler TooltipChanged;

        private CommandButton _source;
        public CommandButton Source
        {
            get { return _source; }
            set
            {
                if (_source != null)
                {
                    if (_compactToken.HasValue)
                    {
                        _source.UnregisterPropertyChangedCallback(CommandButton.IsCompactProperty, _compactToken.Value);
                        _compactToken = null;
                    }

                    if (_label != null)
                    {
                        if (_labelToken.HasValue)
                        {
                            _label?.UnregisterPropertyChangedCallback(TextBlock.TextProperty, _labelToken.Value);
                            _labelToken = null;
                        }

                        _label.IsTextTrimmedChanged -= OnIsTextTrimmedChanged;
                        _label = null;
                    }
                }

                _source = value;

                if (_source != null)
                {
                    _compactToken = _source.RegisterPropertyChangedCallback(CommandButton.IsCompactProperty, OnPropertyChanged);

                    _label = GetLabelTextBlock();
                    if (_label != null)
                    {
                        _labelToken = _label.RegisterPropertyChangedCallback(TextBlock.TextProperty, OnPropertyChanged);
                        _label.IsTextTrimmedChanged += OnIsTextTrimmedChanged;
                    }

                    UpdateTooltip();
                }
            }
        }

        private long? _compactToken;
        private long? _labelToken;

        private TextBlock _label;

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

        private TextBlock GetLabelTextBlock()
        {
            Button button = _source?.Content as Button;
            Grid grid = button?.Content as Grid;

            return grid?.FindChildByName("LabelTextBlock") as TextBlock;
        }
    }

    public class CommandButtonTooltipBehavior : TooltipBehavior<CommandButton, CommandButtonTooltipSource>
    { }
}
