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

using Windows.ApplicationModel.Resources;

namespace Eppie.App.UI.Resources
{
    public class StringProvider
    {
        private readonly ResourceLoader _resourceLoader;

        public static StringProvider GetInstance()
        {
            return new StringProvider();
        }

        public static StringProvider GetInstance(string name)
        {
            return new StringProvider(name);
        }

        public string GetString(string resource)
        {
            return _resourceLoader.GetString(resource);
        }

        private StringProvider()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
        }

        private StringProvider(string name)
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse(name);
        }
    }
}
