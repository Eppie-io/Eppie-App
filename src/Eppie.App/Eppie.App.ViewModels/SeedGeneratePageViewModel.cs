using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;

namespace Tuvi.App.ViewModels
{
    public class SeedGeneratePageViewModel : BaseViewModel
    {
        private SeedPhraseModel seedPhrase;
        public SeedPhraseModel SeedPhrase
        {
            get { return seedPhrase; }
            private set
            {
                SetProperty(ref seedPhrase, value);
            }
        }

        private bool _isBusyShown;
        public bool IsBusyShown
        {
            get { return _isBusyShown; }
            private set { SetProperty(ref _isBusyShown, value); }
        }

        public override async void OnNavigatedTo(object data)
        {
            IsBusyShown = true;
            try
            {
                await GenerateSeedPhraseAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                IsBusyShown = false;
            }
        }

        public ICommand GenerateSeedCommand => new AsyncRelayCommand(GenerateSeedPhraseAsync);

        public ICommand CopySeedCommand => new RelayCommand<IClipboardProvider>(CopySeed);

        public ICommand AcceptSeedCommand => new RelayCommand(() => NavigationService?.Navigate(nameof(PasswordPageViewModel), PasswordActions.SetPassword));

        public ICommand BackCommand => new RelayCommand(() => NavigationService?.GoBack());

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        private async Task GenerateSeedPhraseAsync()
        {
            try
            {
                string[] seed = await Core.GetSecurityManager().CreateSeedPhraseAsync().ConfigureAwait(true);
                SeedPhrase = new SeedPhraseModel(seed, true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void CopySeed(IClipboardProvider clipboard)
        {
            try
            {
                clipboard.SetClipboardContent(SeedPhrase.CleanText);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
