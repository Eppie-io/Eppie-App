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

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class TextBlockTooltipSource : ITooltipSource<TextBlock>
    {
        public bool IsActive => Source?.IsTextTrimmed ?? false;
        public string Text => Source?.Text;

        public event EventHandler TooltipChanged;

        private TextBlock _source;
        public TextBlock Source
        {
            get { return _source; }
            set
            {
                if (_source != null)
                {
                    _source.IsTextTrimmedChanged -= OnIsTextTrimmedChanged;
                }

                _source = value;

                if (_source != null)
                {
                    _source.IsTextTrimmedChanged += OnIsTextTrimmedChanged;
                }
            }
        }

        private void OnIsTextTrimmedChanged(TextBlock sender, IsTextTrimmedChangedEventArgs args)
        {
            TooltipChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class TrimmedTextBlockTooltipBehavior : TooltipBehavior<TextBlock, TextBlockTooltipSource>
    { }
}
