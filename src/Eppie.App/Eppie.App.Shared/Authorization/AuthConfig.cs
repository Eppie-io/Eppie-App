using System.Collections.Generic;

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
    }
}
