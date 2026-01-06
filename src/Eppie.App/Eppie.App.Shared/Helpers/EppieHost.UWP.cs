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

#if WINDOWS_UWP

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eppie.App.Helpers
{
    internal static partial class EppieHost
    {
        public static IHostBuilder CreateBuilder(ILoggerFactory loggerFactory)
        {
            return Host.CreateDefaultBuilder(Environment.GetCommandLineArgs()).ConfugureDefault().ConfugureUwp().AddLoggerFactory(loggerFactory);
        }

        public static IHostBuilder CreateBuilder()
        {
            return Host.CreateDefaultBuilder(Environment.GetCommandLineArgs()).ConfugureDefault().ConfugureUwp();
        }

        private static IHostBuilder ConfugureUwp(this IHostBuilder builder)
        {
            return builder;
        }
    }
}

#endif
