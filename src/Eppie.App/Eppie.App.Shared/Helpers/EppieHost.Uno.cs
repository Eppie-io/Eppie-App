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


#if !WINDOWS_UWP

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eppie.App.Helpers
{
    internal static partial class EppieHost
    {
        public static IHostBuilder CreateBuilder(ILoggerFactory loggerFactory)
        {
            return UnoHost.CreateDefaultBuilder(Environment.GetCommandLineArgs()).ConfugureDefault().ConfugureUno().AddLoggerFactory(loggerFactory);
        }

        public static IHostBuilder CreateBuilder()
        {
            return UnoHost.CreateDefaultBuilder(Environment.GetCommandLineArgs()).ConfugureDefault().ConfugureUno();
        }

        private static IHostBuilder ConfugureUno(this IHostBuilder hostBuilder)
        {
            hostBuilder//.ConfigureUnoLogging()
                       .UseConfiguration(configure: ConfigureConfig)
                       .UseLogging(configure: (context, logBuilder) => { })
                       .UseLocalization(); // Enable localization (see appsettings.json for supported languages)

            return hostBuilder;
        }

        private static IHostBuilder ConfigureConfig(IConfigBuilder builder)
        {
            return builder.EmbeddedSource<App>()
                          .Section<AppConfig>();
        }

        private static IHostBuilder ConfigureUnoLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseLogging(configure: (context, logBuilder) =>
            {
                // Configure log levels for different categories of logging
                logBuilder
                    .SetMinimumLevel(
                        context.HostingEnvironment.IsDevelopment() ?
                            LogLevel.Information :
                            LogLevel.Warning)

                    // Default filters for core Uno Platform namespaces
                    .CoreLogLevel(LogLevel.Warning);

                // Uno Platform namespace filter groups
                // Uncomment individual methods to see more detailed logging
                //// Generic Xaml events
                //logBuilder.XamlLogLevel(LogLevel.Debug);
                //// Layout specific messages
                //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                //// Storage messages
                //logBuilder.StorageLogLevel(LogLevel.Debug);
                //// Binding related messages
                //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                //// Binder memory references tracking
                //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                //// DevServer and HotReload related
                //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                //// Debug JS interop
                //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

            }, enableUnoLogging: true);
        }
    }
}

#endif
