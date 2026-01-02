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
using System.Globalization;

namespace Tuvi.App.ViewModels
{
    public class AboutPageViewModel : BaseViewModel
    {
        public string TwitterPostLink
        {
            get
            {
                var twitterHandle = BrandService.GetTwitterHandle();
                var githubUrl = BrandService.GetGitHub();
                var text = GetLocalizedString("WhatsNewTwitPostText");
                text = string.Format(CultureInfo.InvariantCulture, text, twitterHandle);

                return $"https://twitter.com/intent/tweet?text={text}&url={githubUrl}";
            }
        }

        public async void RequestReview()
        {
            try
            {
                await AppStoreService.RequestReviewAsync().ConfigureAwait(true);
            }
            catch (NotImplementedException)
            {
                await LauncherService.LaunchAsync(new Uri(BrandService.GetGitHub())).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
