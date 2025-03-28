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
