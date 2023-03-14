﻿///////////////////////////////////////////////////////////////////////////////
//
//   Copyright 2023 Eppie(https://eppie.io)
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
///////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace Eppie.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    ///
    /// Your own code may be placed in the Eppie.App/AppBase.cs class.
    /// </summary>
    public sealed partial class AppHead : App
    {
        static AppHead()
            => InitializeLogging();

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public AppHead()
            => this.InitializeComponent();

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
#if DEBUG
		// Logging is disabled by default for release builds, as it incurs a significant
		// initialization cost from Microsoft.Extensions.Logging setup. If startup performance
		// is a concern for your application, keep this disabled. If you're running on the web or
		// desktop targets, you can use URL or command line parameters to enable it.
		//
		// For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

		var factory = LoggerFactory.Create(builder =>
		{
#if __WASM__
			builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__ && !__MACCATALYST__
			builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
			builder.AddDebug();
#else
			builder.AddConsole();
#endif

			// Exclude logs below this level
			builder.SetMinimumLevel(LogLevel.Information);

			// Default filters for Uno Platform namespaces
			builder.AddFilter("Uno", LogLevel.Warning);
			builder.AddFilter("Windows", LogLevel.Warning);
			builder.AddFilter("Microsoft", LogLevel.Warning);

			// Generic Xaml events
			// builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
			// builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
			// builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
			// builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
			// builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

			// Layouter specific messages
			// builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
			// builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
			// builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

			// builder.AddFilter("Windows.Storage", LogLevel.Debug );

			// Binding related messages
			// builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
			// builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

			// Binder memory references tracking
			// builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

			// RemoteControl and HotReload related
			// builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

			// Debug JS interop
			// builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
		});

		global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
		global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
        }
    }
}