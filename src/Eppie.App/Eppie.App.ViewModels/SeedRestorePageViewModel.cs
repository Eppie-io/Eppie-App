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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core;

namespace Tuvi.App.ViewModels
{
    public enum SeedRestoreActions
    {
        Restore,
        ResetPassword
    }

    public class SeedRestorePageViewModel : BaseViewModel
    {
        private SeedRestoreActions _action = SeedRestoreActions.Restore;

        private SeedPhraseModel seedPhrase;
        [CustomValidation(typeof(SeedRestorePageViewModel), nameof(ClearValidationErrors))]
        [CustomValidation(typeof(SeedRestorePageViewModel), nameof(ValidateSeedWordsAreNotEmpty))]
        [CustomValidation(typeof(SeedRestorePageViewModel), nameof(ValidateSeedWordsAreInDictionary))]
        public SeedPhraseModel SeedPhrase
        {
            get { return seedPhrase; }
            set
            {
                if (seedPhrase != null)
                {
                    seedPhrase.PropertyChanged -= OnSeedPhraseChanged;
                }

                SetProperty(ref seedPhrase, value, true);

                if (seedPhrase != null)
                {
                    seedPhrase.PropertyChanged += OnSeedPhraseChanged;
                }
            }
        }

        private void OnSeedPhraseChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateProperty(SeedPhrase, nameof(SeedPhrase));
        }

        public void OnSeedPhraseChanging(string seed)
        {
            var seedPhrase = new SeedPhraseModel(SecurityManager.GetRequiredSeedPhraseLength());
            seedPhrase.Import(seed);

            var validationContext = new ValidationContext(this);

            isAcceptEnabledForce = ValidateSeedWordsAreNotEmpty(seedPhrase, validationContext) == ValidationResult.Success
                                   && ValidateSeedWordsAreInDictionary(seedPhrase, validationContext) == ValidationResult.Success;

            if (AcceptSeedCommand.CanExecute(null))
            {
                SeedPhrase = seedPhrase;
            }

            AcceptSeedCommand.NotifyCanExecuteChanged();
        }

        private ISecurityManager SecurityManager;

        public override void OnNavigatedTo(object data)
        {
            if (data is SeedRestoreActions action)
            {
                _action = action;
            }

            BackCommand.NotifyCanExecuteChanged();

            SecurityManager = Core.GetSecurityManager();
            CreateEmptySeed(SecurityManager.GetRequiredSeedPhraseLength());

            base.OnNavigatedTo(data);
        }

        public IRelayCommand BackCommand { get; }

        public IRelayCommand AcceptSeedCommand { get; }

        public ICommand PasteSeedCommand => new AsyncRelayCommand<IClipboardProvider>(PasteSeedAsync);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public SeedRestorePageViewModel()
        {
            BackCommand = new RelayCommand(() => NavigationService?.GoBack(), () => NavigationService.CanGoBack());
            AcceptSeedCommand = new RelayCommand(AcceptSeed, () => IsAcceptEnabled());

            ErrorsChanged += (sender, e) => AcceptSeedCommand.NotifyCanExecuteChanged();
        }

        private bool isAcceptEnabledForce;
        private bool IsAcceptEnabled()
        {
            return (!HasErrors && isAcceptEnabledForce) || isAcceptEnabledForce;
        }

        private void CreateEmptySeed(int seedPhraseWordsCount)
        {
            SeedPhrase = new SeedPhraseModel(seedPhraseWordsCount);
        }

        private void AcceptSeed()
        {
            try
            {
                var context = new PasswordStartContext()
                {
                    PasswordActions = PasswordActions.SetPassword,
                    SeedPhrase = SeedPhrase.GetPhrase()
                };

                NavigationService?.Navigate(nameof(PasswordPageViewModel), context);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task PasteSeedAsync(IClipboardProvider clipboard)
        {
            try
            {
                var clipboardContent = await clipboard.GetClipboardContentAsync().ConfigureAwait(true);

                CreateEmptySeed(SecurityManager.GetRequiredSeedPhraseLength());
                SeedPhrase.Import(clipboardContent);
                OnPropertyChanged(nameof(SeedPhrase));

                ValidateProperty(SeedPhrase, nameof(SeedPhrase));
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public static ValidationResult ClearValidationErrors(SeedPhraseModel seedPhraseModel, ValidationContext context)
        {
            if (seedPhraseModel != null)
            {
                foreach (var wordModel in seedPhraseModel)
                {
                    wordModel.Word.Errors.Clear();
                }
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateSeedWordsAreNotEmpty(SeedPhraseModel seedPhraseModel, ValidationContext context)
        {
            var result = ValidationResult.Success;

            if (seedPhraseModel != null &&
                context?.ObjectInstance is SeedRestorePageViewModel viewModel)
            {
                foreach (var wordModel in seedPhraseModel.Where(wModel => wModel.Word.NeedsValidation))
                {
                    if (string.IsNullOrEmpty(wordModel.Word.Value))
                    {
                        var error = viewModel.GetLocalizedString("SeedWordIsEmpty");
                        wordModel.Word.Errors.Add(error);
                        if (result == ValidationResult.Success)
                        {
                            result = new ValidationResult(error);
                        }
                    }
                }
            }

            return result;
        }

        public static ValidationResult ValidateSeedWordsAreInDictionary(SeedPhraseModel seedPhraseModel, ValidationContext context)
        {
            var result = ValidationResult.Success;

            if (seedPhraseModel != null &&
                context?.ObjectInstance is SeedRestorePageViewModel viewModel)
            {
                foreach (var wordModel in seedPhraseModel.Where(wModel => wModel.Word.NeedsValidation))
                {
                    if (!string.IsNullOrEmpty(wordModel.Word.Value) &&
                        !viewModel.SecurityManager.GetSeedValidator().IsWordExistInDictionary(wordModel.Word.Value))
                    {
                        var error = viewModel.GetLocalizedString("SeedWordNoInDictionary");
                        wordModel.Word.Errors.Add(error);
                        if (result == ValidationResult.Success)
                        {
                            result = new ValidationResult(error);
                        }
                    }
                }
            }

            return result;
        }
    }
}
