using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.App.ViewModels.Validation;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class BaseAccountSettingsModel : ObservableObject
    {
        /// <summary>
        /// Default interval value (in minutes) for checking new messages
        /// </summary>
        protected const int DefaultSynchronizationInterval = 10;

        protected Account CurrentAccount { get; set; }
        public ValidatableProperty<string> Email { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> SynchronizationInterval { get; } = new ValidatableProperty<string>();

        private string _senderName;
        public string SenderName
        {
            get => _senderName;
            set => SetProperty(ref _senderName, value);
        }

        private bool _isBackupAccountSettingsEnabled = true;
        public bool IsBackupAccountSettingsEnabled
        {
            get { return _isBackupAccountSettingsEnabled; }
            set
            {
                SetProperty(ref _isBackupAccountSettingsEnabled, value);

                if (!IsBackupAccountSettingsEnabled)
                {
                    IsBackupAccountMessagesEnabled = false;
                }
            }
        }

        private bool _isBackupAccountMessagesEnabled = true;
        public bool IsBackupAccountMessagesEnabled
        {
            get { return _isBackupAccountMessagesEnabled; }
            set { SetProperty(ref _isBackupAccountMessagesEnabled, value); }
        }

        public BaseAccountSettingsModel()
        {
            Email.SetInitialValue(string.Empty);
            SynchronizationInterval.SetInitialValue(DefaultSynchronizationInterval.ToString());
        }
        protected BaseAccountSettingsModel(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            CurrentAccount = account;

            if (account.Email != null)
            {
                Email.SetInitialValue(account.Email.Address);
                SenderName = account.Email.Name;
            }

            IsBackupAccountSettingsEnabled = account.IsBackupAccountSettingsEnabled;
            IsBackupAccountMessagesEnabled = account.IsBackupAccountMessagesEnabled;
            SynchronizationInterval.SetInitialValue(account.SynchronizationInterval.ToString());

            Email.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(Email), args.PropertyName);
            };
            SynchronizationInterval.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged<string>(nameof(SynchronizationInterval), args.PropertyName);
            };
        }

        /// <summary>
        /// Handles ValidatableProperty changes
        /// </summary>
        /// <typeparam name="T">The type of ValidatableProperty</typeparam>
        /// <param name="validatablePropertyName">The name of the ValidatableProperty</param>
        /// <param name="propertyName">The name of the property that was changed inside the ValidatableProperty</param>
        protected void OnValidatablePropertyChanged<T>(string validatablePropertyName, string propertyName)
        {
            if (propertyName == nameof(ValidatableProperty<T>.Value) ||
                propertyName == nameof(ValidatableProperty<T>.NeedsValidation))
            {
                OnPropertyChanged(validatablePropertyName);
            }
        }
    }
}
