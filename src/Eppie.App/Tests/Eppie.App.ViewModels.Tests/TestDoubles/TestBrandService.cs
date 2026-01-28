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

using Tuvi.App.ViewModels.Services;

namespace Eppie.App.ViewModels.Tests.TestDoubles
{
    internal sealed class TestBrandService : IBrandService
    {
        public string GetName() => "Eppie";
        public string GetSupport() => "support";
        public string GetHomepage() => "https://eppie.test/download";
        public string GetPublisherDisplayName() => "Eppie";
        public string GetAppVersion() => "1";
        public string GetVersion() => "1";
        public string GetPackageVersion() => "1";
        public string GetFileVersion() => "1";
        public string GetInformationalVersion() => "1";
        public string GetDevelopmentSupport() => "dev";
        public string GetTwitterHandle() => "@eppie";
        public string GetGitHub() => "gh";
        public string GetTranslation() => "tr";
        public string GetLicense() => "lic";
    }
}
