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

using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using Windows.Storage;

namespace Eppie.App.Shared.Logging
{
    internal static class LogConfiguration
    {
        private static readonly string AppLogFileName = "EppieLog.json";
        private static readonly string FolderName = "Logs";
        public static readonly string LogFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, FolderName);
        private static readonly string AppLogFilePath = Path.Combine(LogFolderPath, AppLogFileName);

        public static LoggerConfiguration AddLogging(this LoggerConfiguration configuration, LogLevel logLevel)
        {
            configuration.MinimumLevel.Is(logLevel.ToLogEventLevel())
                         .Enrich.WithProperty(nameof(Helpers.Platform), Helpers.PlatformTools.CurrentPlatform)
                         .Enrich.WithEnvironmentName()
                         .Enrich.WithThreadId()
                         .Enrich.WithThreadName()
#if DEBUG
                         .Enrich.WithExceptionDetails()
#endif
                         .Enrich.WithSensitiveDataMasking(options =>
                         {
                             options.Mode = MaskingMode.Globally;
#if DEBUG
                             options.MaskingOperators = new List<IMaskingOperator>
                             {
                                 new HashTransformOperator<EmailAddressMaskingOperator>(),
                                 new HashTransformOperator<IbanMaskingOperator>(),
                                 new HashTransformOperator<CreditCardMaskingOperator>()
                             };
#endif
                         });

#if !__WASM__
            configuration.AddFileLogging(AppLogFilePath);
#endif

#if DEBUG
            configuration.AddConsoleLogging();
#endif

            return configuration;
        }

        public static ILoggerFactory CreateLoggerFactory(this LoggerConfiguration configuration)
        {
            return new SerilogLoggerFactory(configuration.CreateLogger());
        }

        private static LoggerConfiguration AddConsoleLogging(this LoggerConfiguration configuration)
        {
#if __ANDROID__
            configuration.WriteTo.AndroidLog();
#elif __IOS__
            configuration.WriteTo.NSLog();
#else
            configuration.WriteTo.Console();
#endif
            configuration.WriteTo.Debug();

            return configuration;
        }

        private static LoggerConfiguration AddFileLogging(this LoggerConfiguration configuration, string filePath)
        {
            return configuration.WriteTo.Async(config =>
            {
                config.File(formatter: new CompactJsonFormatter(),
                            path: filePath,
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 10485760); // 10mb
            });
        }
    }
}
