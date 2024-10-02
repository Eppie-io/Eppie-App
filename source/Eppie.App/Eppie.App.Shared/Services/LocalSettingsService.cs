using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;
using Tuvi.App.ViewModels.Services;
using Windows.Storage;

namespace Tuvi.App.Shared.Services
{
    public class LocalSettingsService : ILocalSettingsService
    {
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
