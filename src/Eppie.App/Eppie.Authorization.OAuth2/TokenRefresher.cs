using System;
using System.Threading;
using System.Threading.Tasks;
using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;
using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Tuvi.OAuth2
{
    public class TokenRefresher : ITokenRefresher
    {
        public static ITokenRefresher CreateTokenRefresher(AuthorizationProvider authorizationProvider)
        {
            return new TokenRefresher(authorizationProvider);
        }

        private static string EmptyOrWhiteSpaceExceptionString { get => "The value cannot be an empty string or composed entirely of whitespace."; }
        private static readonly Func<string, string> EmailServiceNotSupportedExceptionString = (email) => $"'{email}' email service is not supported.";

        private readonly AuthorizationProvider _authorizationProvider;

        private TokenRefresher(AuthorizationProvider authorizationProvider)
        {
            _authorizationProvider = authorizationProvider;
        }

        public async Task<AuthToken> RefreshTokenAsync(string mailServiceName, string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(mailServiceName))
            {
                throw new ArgumentException(EmptyOrWhiteSpaceExceptionString, nameof(mailServiceName));
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException(EmptyOrWhiteSpaceExceptionString, nameof(refreshToken));
            }

            try
            {
                MailService mailService = GetMailService(mailServiceName);
                IRefreshable refresher = _authorizationProvider.CreateRefreshTokenClient(mailService);
                AuthCredential freshToken = await refresher.RefreshAsync(CreateToken(refreshToken), cancellationToken).ConfigureAwait(false);

                return new AuthToken()
                {
                    AccessToken = freshToken.AccessToken,
                    RefreshToken = freshToken.RefreshToken,
                    ExpiresIn = freshToken.ExpiresIn,
                };
            }
            catch (Finebits.Authorization.OAuth2.Exceptions.AuthorizationInvalidResponseException e)
            {
                throw new AuthorizationException(e.Message, e);
            }
        }

        private static MailService GetMailService(string mailServiceName)
        {
            return Enum.TryParse(mailServiceName, out MailService mailService) && mailService != MailService.Unknown
                ? mailService
                : throw new AuthenticationException(EmailServiceNotSupportedExceptionString(mailServiceName));
        }

        private static AuthCredential CreateToken(string refreshToken)
        {
            return new AuthCredential(tokenType: Credential.BearerType,
                                      accessToken: string.Empty,
                                      refreshToken: refreshToken,
                                      idToken: string.Empty,
                                      expiresIn: TimeSpan.Zero,
                                      scope: string.Empty);
        }
    }
}
