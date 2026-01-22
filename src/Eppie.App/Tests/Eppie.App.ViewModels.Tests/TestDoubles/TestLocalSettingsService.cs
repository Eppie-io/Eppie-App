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

using Microsoft.Extensions.Logging;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Tests.TestDoubles
{
    internal sealed class TestLocalSettingsService : ILocalSettingsService
    {
        public event EventHandler<SettingChangedEventArgs>? SettingChanged;

        private string _language = string.Empty;
        public string Language
        {
            get => _language;
            set
            {
                if (!string.Equals(_language, value, StringComparison.Ordinal))
                {
                    _language = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(Language)));
                }
            }
        }

        private AppTheme _theme;
        public AppTheme Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(Theme)));
                }
            }
        }

        private AppScale _uiScale;
        public AppScale UIScale
        {
            get => _uiScale;
            set
            {
                if (_uiScale != value)
                {
                    _uiScale = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(UIScale)));
                }
            }
        }

        private string _selectedMailFilterForAllMessagesPage = string.Empty;
        public string SelectedMailFilterForAllMessagesPage
        {
            get => _selectedMailFilterForAllMessagesPage;
            set
            {
                if (!string.Equals(_selectedMailFilterForAllMessagesPage, value, StringComparison.Ordinal))
                {
                    _selectedMailFilterForAllMessagesPage = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(SelectedMailFilterForAllMessagesPage)));
                }
            }
        }

        private string _selectedMailFilterForFolderMessagesPage = string.Empty;
        public string SelectedMailFilterForFolderMessagesPage
        {
            get => _selectedMailFilterForFolderMessagesPage;
            set
            {
                if (!string.Equals(_selectedMailFilterForFolderMessagesPage, value, StringComparison.Ordinal))
                {
                    _selectedMailFilterForFolderMessagesPage = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(SelectedMailFilterForFolderMessagesPage)));
                }
            }
        }

        private string _selectedMailFilterForContactMessagesPage = string.Empty;
        public string SelectedMailFilterForContactMessagesPage
        {
            get => _selectedMailFilterForContactMessagesPage;
            set
            {
                if (!string.Equals(_selectedMailFilterForContactMessagesPage, value, StringComparison.Ordinal))
                {
                    _selectedMailFilterForContactMessagesPage = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(SelectedMailFilterForContactMessagesPage)));
                }
            }
        }

        private int _requestReviewCount;
        public int RequestReviewCount
        {
            get => _requestReviewCount;
            set
            {
                if (_requestReviewCount != value)
                {
                    _requestReviewCount = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(RequestReviewCount)));
                }
            }
        }

        private int _developmentSupportRequestCount;
        public int DevelopmentSupportRequestCount
        {
            get => _developmentSupportRequestCount;
            set
            {
                if (_developmentSupportRequestCount != value)
                {
                    _developmentSupportRequestCount = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(DevelopmentSupportRequestCount)));
                }
            }
        }

        private LogLevel _logLevel;
        public LogLevel LogLevel
        {
            get => _logLevel;
            set
            {
                if (_logLevel != value)
                {
                    _logLevel = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(LogLevel)));
                }
            }
        }

        public string LogFolderPath { get; } = string.Empty;

        private SidePaneKind _lastSidePane;
        public SidePaneKind LastSidePane
        {
            get => _lastSidePane;
            set
            {
                if (_lastSidePane != value)
                {
                    _lastSidePane = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(LastSidePane)));
                }
            }
        }

        private bool _isNavigationPaneOpen;
        public bool IsNavigationPaneOpen
        {
            get => _isNavigationPaneOpen;
            set
            {
                if (_isNavigationPaneOpen != value)
                {
                    _isNavigationPaneOpen = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(IsNavigationPaneOpen)));
                }
            }
        }

        private string _lastShownWhatsNewId = string.Empty;
        public string LastShownWhatsNewId
        {
            get => _lastShownWhatsNewId;
            set
            {
                if (!string.Equals(_lastShownWhatsNewId, value, StringComparison.Ordinal))
                {
                    _lastShownWhatsNewId = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(LastShownWhatsNewId)));
                }
            }
        }

        private ContactsSortOrder _contactsSortOrder = ContactsSortOrder.ByTime;
        public ContactsSortOrder ContactsSortOrder
        {
            get => _contactsSortOrder;
            set
            {
                if (_contactsSortOrder != value)
                {
                    _contactsSortOrder = value;
                    SettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(ContactsSortOrder)));
                }
            }
        }
    }
}
