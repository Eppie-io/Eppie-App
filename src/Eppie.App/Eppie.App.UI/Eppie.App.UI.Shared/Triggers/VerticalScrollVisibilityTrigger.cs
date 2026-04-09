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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Triggers
{
    public class VerticalScrollVisibilityTrigger : StateTriggerBase
    {
        private ScrollViewer _targetScrollViewer;
        private long? _token;

        public ScrollViewer TargetScrollViewer
        {
            get => _targetScrollViewer;
            set
            {
                if (_targetScrollViewer != null && _token.HasValue)
                {
                    _targetScrollViewer.UnregisterPropertyChangedCallback(ScrollViewer.ScrollableHeightProperty, _token.Value);
                    _token = null;
                }

                _targetScrollViewer = value;

                if (_targetScrollViewer != null)
                {
                    _token = _targetScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ScrollableHeightProperty, OnScrollableHeightChanged);
                }
            }
        }

        private void OnScrollableHeightChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateTrigger();
        }

        private void UpdateTrigger()
        {
            if (_targetScrollViewer != null)
            {
                SetActive(_targetScrollViewer.ScrollableHeight > 0);
            }
        }
    }
}
