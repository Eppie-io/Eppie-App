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

using Tuvi.App.ViewModels.Services;

namespace Eppie.App.Models
{
    public sealed class BrandResource
    {
        private readonly IBrandService _brand;

        public BrandResource()
        {
            _brand = new BrandLoader();
        }

        public string AppName => _brand.GetName();
        public string Homepage => _brand.GetHomepage();
        public string Support => _brand.GetSupport();
        public string GitHub => _brand.GetGitHub();
        public string DevelopmentSupport => _brand.GetDevelopmentSupport();
        public string TwitterHandle => _brand.GetTwitterHandle();
        public string Translation => _brand.GetTranslation();
        public string License => _brand.GetLicense();
        public string AppVersion => _brand.GetAppVersion();
        public string PublisherDisplayName => _brand.GetPublisherDisplayName();

        public string TwitterUrl => $"https://twitter.com/{TwitterHandle}";
        public string SupportMailto => $"mailto:{Support}";
    }
}
