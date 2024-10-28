#if !WINDOWS_UWP

using System;
using Finebits.Authorization.OAuth2.Google;
using Finebits.Authorization.OAuth2.Outlook;
using Tuvi.OAuth2;

namespace Tuvi.App.Shared.Authorization
{
    internal static partial class AuthConfig
    {
        public static AuthorizationConfiguration GetAuthorizationConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}

#endif
