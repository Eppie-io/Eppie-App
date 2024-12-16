using System.Collections.Generic;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.AuthenticationBroker;

namespace Tuvi.App.Shared.Authorization
{
    internal static partial class AuthConfig
    {
        public static IReadOnlyCollection<string> GmailScope = new[]
        {
            "https://mail.google.com/",
            "openid",
            "profile",
            "email"
        };

        public static IReadOnlyCollection<string> OutlookScope = new[]
        {
            "offline_access",
            "openid",
            "profile",
            "email",
            "https://outlook.office.com/IMAP.AccessAsUser.All",
            "https://outlook.office.com/SMTP.Send"
        };

        private static IAuthenticationBroker GetAuthenticationBroker()
        {
#if (__ANDROID__ || __IOS__)
            throw new NotImplementedException();
#elif WINDOWS_UWP
            return new WindowsAuthenticationBroker();
#elif (HAS_UNO) // macOS; Skia.Gtk; Wasm;
            if (!DesktopAuthenticationBroker.IsSupported)
            {
                throw new InvalidOperationException("DesktopAuthenticationBroker is not supported");
            }

            return new DesktopAuthenticationBroker(new WebBrowserLauncher());
#else   // Unknown
            throw new NotImplementedException();
#endif
        }
    }
}
