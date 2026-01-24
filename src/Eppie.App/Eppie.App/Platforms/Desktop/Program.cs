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
using System.Linq;
using Uno.UI.Hosting;

namespace Eppie.App
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Check for mailto: protocol in arguments
            string mailtoUri = null;
            if (args != null && args.Length > 0)
            {
                // Look for mailto: URI in arguments
                mailtoUri = args.FirstOrDefault(arg =>
                    arg != null && arg.StartsWith(Tuvi.App.ViewModels.Helpers.MailtoUriParser.MailtoSchemePrefix, StringComparison.OrdinalIgnoreCase));
            }

            var host = UnoPlatformHostBuilder.Create()
                .App(() =>
                {
                    var app = new Eppie.App.App();

                    // If we have a mailto URI, set it as pending
                    if (!string.IsNullOrEmpty(mailtoUri))
                    {
                        app.SetPendingMailtoUri(mailtoUri);
                    }

                    return app;
                })
                .UseX11()
                .UseLinuxFrameBuffer()
                .UseMacOS()
                .UseWin32()
                .Build();

            host.Run();
        }
    }
}
