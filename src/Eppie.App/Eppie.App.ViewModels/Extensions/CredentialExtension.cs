using System.IdentityModel.Tokens.Jwt;
using Finebits.Authorization.OAuth2.Types;
using Microsoft.IdentityModel.Tokens;

namespace Tuvi.App.ViewModels.Extensions
{
    public static class CredentialExtension
    {
        const string EmailPayloadProperty = "email";
        const string NamePayloadProperty = "name";

        public static object ReadPayloadProperty(this AuthCredential credential, string property)
        {
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = handler.ReadJwtToken(credential.IdToken);

                if (jwtToken.Payload.TryGetValue(property, out object value))
                {
                    return value;
                }
            }
            catch (SecurityTokenArgumentException)
            { }

            return null;
        }

        public static string ReadEmailAddress(this AuthCredential credential)
        {
            return credential.ReadPayloadProperty(EmailPayloadProperty) as string;
        }

        public static string ReadName(this AuthCredential credential)
        {
            return credential.ReadPayloadProperty(NamePayloadProperty) as string;
        }
    }
}
