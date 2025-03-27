using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        #region Set/Get option

        private ApplicationDataContainer AppLocalSettings { get { return ApplicationData.Current.LocalSettings; } }

        private void SetOption<T>(T value, [CallerMemberName] string key = null)
        {
            try
            {
                AppLocalSettings.Values[key] = value;
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

        #endregion Set/Get option
    }
}
