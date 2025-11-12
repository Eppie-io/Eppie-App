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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Eppie.App.Shared.Logging;
using Microsoft.Extensions.Logging;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Services;
using Windows.Storage;

namespace Tuvi.App.Shared.Services
{
    /// <summary>
    /// Service to store and retrieve local settings
    /// WARNING: Settings are not encrypted!
    /// </summary>
    public class LocalSettingsService : ILocalSettingsService
    {
        /// <summary>
        /// The event arises when any setting changed.
        /// </summary>
        public event EventHandler<SettingChangedEventArgs> SettingChanged;

        /// <summary>
        /// Property to store the UI language
        /// </summary>
        public string Language
        {
            get
            {
                return GetOption("");
            }
            set
            {
                SetOption(value);
            }
        }

        /// <summary>
        /// Property to store the UI theme
        /// </summary>
        public Tuvi.App.ViewModels.Services.AppTheme Theme
        {
            get
            {
                return GetEnumOption(Tuvi.App.ViewModels.Services.AppTheme.Default);
            }
            set
            {
                SetEnumOption(value);
            }
        }

        /// <summary>
        /// UI scale setting
        /// </summary>
        public AppScale UIScale
        {
            get
            {
                return GetEnumOption(AppScale.SystemDefault);
            }
            set
            {
                SetEnumOption(value);
            }
        }

        /// <summary>
        /// Property to store the selected mail filter on all messages page
        /// </summary>
        public string SelectedMailFilterForAllMessagesPage
        {
            get
            {
                return GetOption(nameof(AllMessagesFilter));
            }
            set
            {
                SetOption(value);
            }
        }

        /// <summary>
        /// Property to store the selected mail filter on folder messages page
        /// </summary>
        public string SelectedMailFilterForFolderMessagesPage
        {
            get
            {
                return GetOption(nameof(AllMessagesFilter));
            }
            set
            {
                SetOption(value);
            }
        }

        /// <summary>
        /// Property to store the selected mail filter on contact messages page
        /// </summary>
        public string SelectedMailFilterForContactMessagesPage
        {
            get
            {
                return GetOption(nameof(AllMessagesFilter));
            }
            set
            {
                SetOption(value);
            }
        }

        /// <summary>
        /// Stores the number of times the main page has been opened before showing the request to rate the app.
        /// </summary>
        public int RequestReviewCount
        {
            get
            {
                return GetOption(0);
            }
            set
            {
                SetOption(value);
            }
        }

        /// <summary>
        /// Property to store logging severity levels.
        /// </summary>
        public LogLevel LogLevel
        {
            get
            {
                return GetEnumOption(LogLevel.None);
            }
            set
            {
                SetEnumOption(value);
            }
        }

        /// <summary>
        /// Property to get the path to the log folder.
        /// </summary>
        public string LogFolderPath => LogConfiguration.LogFolderPath;

        /// <summary>
        /// Persists which pane was opened last (None, IdentityManager, ContactsPanel, MailboxesPanel)
        /// </summary>
        public SidePaneKind LastSidePane
        {
            get => GetEnumOption(SidePaneKind.None);
            set => SetEnumOption(value);
        }

        /// <summary>
        /// Persists NavigationView (hamburger) pane open/closed state
        /// </summary>
        public bool IsNavigationPaneOpen
        {
            get => GetOption(false);
            set => SetOption(value);
        }

        /// <summary>
        /// Identifier of the last displayed WhatsNew popup. Empty means never shown.
        /// </summary>
        public string LastShownWhatsNewId
        {
            get => GetOption(string.Empty);
            set => SetOption(value);
        }

        /// <summary>
        /// Stores selected contacts sorter type.
        /// </summary>
        public string SelectedContactsSortingComparer
        {
            get => GetOption(string.Empty);
            set => SetOption(value);
        }

        #region Set/Get option

        private ApplicationDataContainer AppLocalSettings { get { return ApplicationData.Current.LocalSettings; } }

        private void SetOption<T>(T value, [CallerMemberName] string key = null)
        {
            try
            {
                AppLocalSettings.Values[key] = value;
                SettingChanged?.Invoke(this, new SettingChangedEventArgs(key));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private T GetOption<T>(T defaultValue, [CallerMemberName] string key = null)
        {
            try
            {
                object value;
                if (AppLocalSettings.Values.TryGetValue(key, out value))
                {
                    return (T)value;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return defaultValue;
        }

        private void SetEnumOption<TEnum>(TEnum value, [CallerMemberName] string key = null)
            where TEnum : struct, Enum
        {
            SetOption<string>(value.ToString(), key);
        }

        private TEnum GetEnumOption<TEnum>(TEnum defaultValue, [CallerMemberName] string key = null)
            where TEnum : struct, Enum
        {
            string value = GetOption<string>(defaultValue.ToString(), key);

            if (Enum.TryParse<TEnum>(value, true, out TEnum result))
            {
                return result;
            }

            return defaultValue;
        }

        #endregion Set/Get option
    }
}
