using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Validation;

namespace Tuvi.App.ViewModels
{
    [Flags]
    public enum PasswordActions
    {
        None = 0,
        EnterPassword = (1 << 0),
        Confirm = (1 << 1),
        CheckCurrent = (1 << 2),
        SetPassword = EnterPassword | Confirm,
        ChangePassword = SetPassword | CheckCurrent
    }

    public class PasswordStartContext
    {
        public PasswordActions PasswordActions { get; set; }

        // TODO: seed phrase should be zeroed
        public string[] SeedPhrase { get; set; }
    }

    public class PasswordControlModel : ObservableObject
    {
        public PasswordControlModel() : this(PasswordActions.None)
        {
        }

        public PasswordControlModel(PasswordActions passwordAction)
        {
            CurrentPassword.SetInitialValue(string.Empty);
            Password.SetInitialValue(string.Empty);
            ConfirmPassword.SetInitialValue(string.Empty);
            PasswordAction = passwordAction;
            RememberPassword = false;

            CurrentPassword.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged(nameof(CurrentPassword), args.PropertyName);
            };
            Password.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged(nameof(Password), args.PropertyName);
            };
            ConfirmPassword.PropertyChanged += (sender, args) =>
            {
                OnValidatablePropertyChanged(nameof(ConfirmPassword), args.PropertyName);
            };
        }

        /// <summary>
        /// Handles ValidatableProperty changes
        /// </summary>
        /// <param name="validatablePropertyName">The name of the ValidatableProperty</param>
        /// <param name="propertyName">The name of the property that was changed inside the ValidatableProperty</param>
        private void OnValidatablePropertyChanged(string validatablePropertyName, string propertyName)
        {
            if (propertyName == nameof(ValidatableProperty<string>.Value) ||
                propertyName == nameof(ValidatableProperty<string>.NeedsValidation))
            {
                OnPropertyChanged(validatablePropertyName);
            }
        }

        // TODO: passwords should be zeroed
        public ValidatableProperty<string> CurrentPassword { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> Password { get; } = new ValidatableProperty<string>();
        public ValidatableProperty<string> ConfirmPassword { get; } = new ValidatableProperty<string>();

        public PasswordActions PasswordAction { get; }

        private bool _rememberPassword;
        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set { SetProperty(ref _rememberPassword, value); }
        }
    }

    public class PasswordPageViewModel : BaseViewModel
    {
        private PasswordControlModel _passwordModel;
        [CustomValidation(typeof(PasswordPageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(PasswordPageViewModel), nameof(ValidatePasswordIsNotEmpty))]
        [CustomValidation(typeof(PasswordPageViewModel), nameof(ValidateConfirmPasswordMatch))]
        [CustomValidation(typeof(PasswordPageViewModel), nameof(ValidatePasswordDiffersCurrent))]
        public PasswordControlModel PasswordModel
        {
            get { return _passwordModel; }
            set
            {
                if (_passwordModel != null)
                {
                    _passwordModel.PropertyChanged -= OnPasswordModelChanged;
                }

                SetProperty(ref _passwordModel, value, true);

                if (_passwordModel != null)
                {
                    _passwordModel.PropertyChanged += OnPasswordModelChanged;
                }
            }
        }

        private void OnPasswordModelChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateProperty(PasswordModel, nameof(PasswordModel));
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                SetProperty(ref _text, value);
                OnPropertyChanged(nameof(IsTextVisible));
            }
        }

        private string[] _seedPhrase;

        public bool IsTextVisible => !string.IsNullOrWhiteSpace(Text);

        private bool _isForgotButtonVisible;
        public bool IsForgotButtonVisible
        {
            get { return _isForgotButtonVisible; }
            private set
            {
                SetProperty(ref _isForgotButtonVisible, value);
            }
        }

        private bool _isCancelButtonVisible;
        public bool IsCancelButtonVisible
        {
            get { return _isCancelButtonVisible; }
            private set
            {
                SetProperty(ref _isCancelButtonVisible, value);
            }
        }

        public IRelayCommand ApplyCommand { get; }

        public ICommand ForgotPasswordCommand => new RelayCommand(DoForgotPassword);

        public ICommand CancelCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public PasswordPageViewModel()
        {
            ApplyCommand = new AsyncRelayCommand(DoApplyAsync, () => !HasErrors);
            ErrorsChanged += (sender, e) => ApplyCommand.NotifyCanExecuteChanged();
        }

        public override void OnNavigatedTo(object data)
        {
            PasswordActions passwordAction = PasswordActions.None;

            if (data is PasswordActions action)
            {
                passwordAction = action;
            }
            else if (data is PasswordStartContext context)
            {
                passwordAction = context.PasswordActions;
                _seedPhrase = context.SeedPhrase;
            }
            SetPageHeader(passwordAction);

            IsForgotButtonVisible = passwordAction == PasswordActions.EnterPassword;
            IsCancelButtonVisible = passwordAction != PasswordActions.EnterPassword;

            PasswordModel = new PasswordControlModel(passwordAction);

            base.OnNavigatedTo(data);
        }

        private void SetPageHeader(PasswordActions action)
        {
            switch (action)
            {
                case PasswordActions.EnterPassword:
                    Text = GetLocalizedString("EnterPasswordPageText");
                    break;
                case PasswordActions.SetPassword:
                    Text = GetLocalizedString("SetPasswordPageText");
                    break;
                case PasswordActions.ChangePassword:
                    Text = GetLocalizedString("ChangePasswordPageText");
                    break;
                default:
                    break;
            }
        }

        private async Task DoApplyAsync()
        {
            try
            {
                if (await ProcessActionAsync(PasswordModel.PasswordAction).ConfigureAwait(true))
                {
                    if (PasswordModel.PasswordAction == PasswordActions.ChangePassword)
                    {
                        NavigationService?.GoBack();
                    }
                    else
                    {
                        NavigationService?.Navigate(nameof(MainPageViewModel));
                    }
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task<bool> ProcessActionAsync(PasswordActions action)
        {
            switch (action)
            {
                case PasswordActions.EnterPassword:
                    {
                        return await ProcessEnterPasswordActionAsync().ConfigureAwait(true);
                    }
                case PasswordActions.SetPassword:
                    {
                        return await ProcessSetPasswordActionAsync().ConfigureAwait(true);
                    }
                case PasswordActions.ChangePassword:
                    {
                        return await ProcessChangePasswordActionAsync().ConfigureAwait(true);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private async Task<bool> ProcessEnterPasswordActionAsync()
        {
            PasswordModel.Password.Errors.Clear();

            if (string.IsNullOrEmpty(PasswordModel.Password.Value))
            {
                //Validator will block Apply button and show notification
                //if password field is empty even if value was not entered
                PasswordModel.Password.NeedsValidation = true;
                ValidateProperty(PasswordModel, nameof(PasswordModel));
                return false;
            }

            if (!await VerifyApplicationPasswordAsync(PasswordModel.Password.Value).ConfigureAwait(true))
            {
                PasswordModel.Password.Errors.Add(GetLocalizedString("IncorrectPassword"));
                return false;
            }

            return true;
        }

        private async Task<bool> ProcessSetPasswordActionAsync()
        {
            if (!TestPasswordFieldsForNull())
            {
                return false;
            }
            await Core.ResetApplicationAsync().ConfigureAwait(true);
            if (_seedPhrase != null)
            {
                await Core.GetSecurityManager().RestoreSeedPhraseAsync(_seedPhrase).ConfigureAwait(true);
                Array.Clear(_seedPhrase, 0, _seedPhrase.Length); // TODO: make this correctly
            }
            if (await VerifyApplicationPasswordAsync(PasswordModel.Password.Value).ConfigureAwait(true))
            {
                return true;
            }
            return false;
        }

        private async Task<bool> ProcessChangePasswordActionAsync()
        {
            PasswordModel.CurrentPassword.Errors.Clear();

            if (!TestPasswordFieldsForNull())
            {
                return false;
            }

            if (!await Core.ChangeApplicationPasswordAsync(PasswordModel.CurrentPassword.Value, PasswordModel.Password.Value).ConfigureAwait(true))
            {
                PasswordModel.CurrentPassword.Errors.Add(GetLocalizedString("IncorrectPassword"));
                return false;
            }

            return true;
        }

        private bool TestPasswordFieldsForNull()
        {
            if (string.IsNullOrEmpty(PasswordModel.Password.Value))
            {
                //Validator will block Apply button and show notification
                //if Password field is empty even if value was not entered
                PasswordModel.Password.NeedsValidation = true;
                ValidateProperty(PasswordModel, nameof(PasswordModel));
                return false;
            }
            else if (string.IsNullOrEmpty(PasswordModel.ConfirmPassword.Value))
            {
                //Validator will block Apply button and show notification
                //if ConfirmPassword field is empty even if value was not entered
                PasswordModel.ConfirmPassword.NeedsValidation = true;
                ValidateProperty(PasswordModel, nameof(PasswordModel));
                return false;
            }
            return true;
        }

        private Task<bool> VerifyApplicationPasswordAsync(string password)
        {
            return Core.InitializeApplicationAsync(password);
        }

        private void DoForgotPassword()
        {
            NavigationService?.Navigate(nameof(SeedRestorePageViewModel), SeedRestoreActions.ResetPassword);
        }

        private void DoCancel()
        {
            if (NavigationService != null)
            {
                if (NavigationService.CanGoBack())
                {
                    NavigationService.GoBack();
                }
                else
                {
                    NavigationService.ExitApplication();
                }
            }
        }

        public static ValidationResult ClearValidationErrors(PasswordControlModel passwordModel, ValidationContext context)
        {
            if (passwordModel != null)
            {
                passwordModel.Password.Errors.Clear();
                passwordModel.CurrentPassword.Errors.Clear();
                passwordModel.ConfirmPassword.Errors.Clear();
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidatePasswordIsNotEmpty(PasswordControlModel passwordModel, ValidationContext context)
        {
            if (context?.ObjectInstance is PasswordPageViewModel viewModel)
            {
                if (passwordModel != null &&
                    (passwordModel.PasswordAction & PasswordActions.EnterPassword) == PasswordActions.EnterPassword &&
                    passwordModel.Password.NeedsValidation &&
                    string.IsNullOrEmpty(passwordModel.Password.Value))
                {
                    var error = viewModel.GetLocalizedString("FieldIsEmptyNotification");
                    passwordModel.Password.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateConfirmPasswordMatch(PasswordControlModel passwordModel, ValidationContext context)
        {
            if (context?.ObjectInstance is PasswordPageViewModel viewModel)
            {
                if (passwordModel != null &&
                    (passwordModel.PasswordAction & PasswordActions.Confirm) == PasswordActions.Confirm &&
                    passwordModel.ConfirmPassword.NeedsValidation &&
                    passwordModel.ConfirmPassword.Value != passwordModel.Password.Value)
                {
                    var error = viewModel.GetLocalizedString("PasswordsDontMatchNotification");
                    passwordModel.ConfirmPassword.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidatePasswordDiffersCurrent(PasswordControlModel passwordModel, ValidationContext context)
        {
            if (context?.ObjectInstance is PasswordPageViewModel viewModel)
            {
                if (passwordModel != null &&
                    (passwordModel.PasswordAction & PasswordActions.CheckCurrent) == PasswordActions.CheckCurrent &&
                    passwordModel.Password.NeedsValidation &&
                    passwordModel.Password.Value == passwordModel.CurrentPassword.Value)
                {
                    var error = viewModel.GetLocalizedString("PasswordIsTheSameNotification");
                    passwordModel.Password.Errors.Add(error);
                    return new ValidationResult(error);
                }
            }

            return ValidationResult.Success;
        }
    }
}
