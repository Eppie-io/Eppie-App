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
using Microsoft.Extensions.Logging;

namespace Tuvi.App.ViewModels.Services
{
    /// <summary>
    /// Application theme options
    /// </summary>
    public enum AppTheme
    {
        /// <summary>
        /// Use system default theme
        /// </summary>
        Default = 0,

        /// <summary>
        /// Use light theme
        /// </summary>
        Light = 1,

        /// <summary>
        /// Use dark theme
        /// </summary>
        Dark = 2
    }

    /// <summary>
    /// UI scale options (percentage) or system default.
    /// SystemDefault keeps OS provided scaling (DPI). Other values apply manual scaling.
    /// </summary>
    public enum AppScale
    {
        /// <summary>
        /// Use system default UI scale
        /// </summary>
        SystemDefault = 0,

        /// <summary>
        /// 100% scale
        /// </summary>
        Scale100 = 100,

        /// <summary>
        /// 150% scale
        /// </summary>
        Scale150 = 150,

        /// <summary>
        /// 200% scale
        /// </summary>
        Scale200 = 200,

        /// <summary>
        /// 250% scale
        /// </summary>
        Scale250 = 250,

        /// <summary>
        /// 300% scale
        /// </summary>
        Scale300 = 300
    }

    /// <summary>
    /// Identifies which side pane content is/was opened in the main page SplitView.
    /// Used to persist and restore the user's last opened pane between app runs.
    /// </summary>
    public enum SidePaneKind
    {
        /// <summary>
        /// No pane is selected. The app must not restore any pane explicitly.
        /// </summary>
        None,
        /// <summary>
        /// Identity Manager pane (accounts onboarding/settings).
        /// </summary>
        IdentityManager,
        /// <summary>
        /// Contacts panel (contacts list).
        /// </summary>
        ContactsPanel,
        /// <summary>
        /// Mailboxes panel (accounts and folders tree).
        /// </summary>
        MailboxesPanel
    }

    /// <summary>
    /// Service interface to store and retrieve local settings
    /// WARNING: Settings are not encrypted!
    /// </summary>
    public interface ILocalSettingsService
    {
        /// <summary>
        /// Occurs when any setting value changes.
        /// </summary>
        event EventHandler<SettingChangedEventArgs> SettingChanged;

        /// <summary>
        /// Current application UI language.
        /// </summary>
        string Language { get; set; }

        /// <summary>
        /// Current application theme.
        /// </summary>
        AppTheme Theme { get; set; }

        /// <summary>
        /// Current UI scale.
        /// </summary>
        AppScale UIScale { get; set; }

        /// <summary>
        /// Selected mail filter on the All Messages page.
        /// </summary>
        string SelectedMailFilterForAllMessagesPage { get; set; }

        /// <summary>
        /// Selected mail filter on the Folder Messages page.
        /// </summary>
        string SelectedMailFilterForFolderMessagesPage { get; set; }

        /// <summary>
        /// Selected mail filter on the Contact Messages page.
        /// </summary>
        string SelectedMailFilterForContactMessagesPage { get; set; }

        /// <summary>
        /// Counter used to decide when to ask the user to review the app.
        /// </summary>
        int RequestReviewCount { get; set; }

        /// <summary>
        /// Current logging level used by the app.
        /// </summary>
        LogLevel LogLevel { get; set; }

        /// <summary>
        /// Absolute path to the folder where logs are written.
        /// </summary>
        string LogFolderPath { get; }

        /// <summary>
        /// The last opened side pane in the main page SplitView.
        /// Set to <see cref="SidePaneKind.None"/> to indicate that no pane should be restored.
        /// </summary>
        SidePaneKind LastSidePane { get; set; }

        /// <summary>
        /// Whether the left NavigationView (hamburger) pane is open.
        /// Used to persist and restore the user's preference across app runs.
        /// </summary>
        bool IsNavigationPaneOpen { get; set; }

        /// <summary>
        /// Identifier of the last displayed WhatsNew popup. Empty means never shown.
        /// Used to ensure the popup appears only once per new content.
        /// </summary>
        string LastShownWhatsNewId { get; set; }
    }

    /// <summary>
    /// Event args that carries the name of the changed setting.
    /// </summary>
    public class SettingChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the setting that changed.
        /// </summary>
        public string Name { get; }

        public SettingChangedEventArgs(string name)
        {
            Name = name;
        }
    }
}
