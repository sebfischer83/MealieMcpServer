using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MealieMCP.Server.Authentication
{
    public class ApiTokenAuthenticationHandler : AuthenticationHandler<ApiTokenAuthenticationOptions>
    {
        public ApiTokenAuthenticationHandler(
            IOptionsMonitor<ApiTokenAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var configuredTokens = Options.Tokens
                ?.Where(token => !string.IsNullOrWhiteSpace(token))
                .Select(token => token.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();

            if (configuredTokens.Length == 0 && Options.AllowAnonymousWhenNoTokensConfigured)
            {
                var identity = new ClaimsIdentity(Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            var providedToken = ExtractToken();
            if (string.IsNullOrEmpty(providedToken))
            {
                return Task.FromResult(AuthenticateResult.Fail("API token missing."));
            }

            if (!configuredTokens.Contains(providedToken, StringComparer.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API token."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, providedToken),
                new Claim(ClaimTypes.Name, "ApiTokenUser"),
            };

            var authenticatedIdentity = new ClaimsIdentity(claims, Scheme.Name);
            var authenticatedPrincipal = new ClaimsPrincipal(authenticatedIdentity);
            var authenticatedTicket = new AuthenticationTicket(authenticatedPrincipal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(authenticatedTicket));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return base.HandleChallengeAsync(properties);
        }

        private string? ExtractToken()
        {
            if (Request.Headers.TryGetValue(ApiTokenAuthenticationDefaults.AuthorizationHeaderName, out var authorizationHeader))
            {
                var headerValue = authorizationHeader.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(headerValue) && headerValue.StartsWith(ApiTokenAuthenticationDefaults.BearerPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var bearerValue = headerValue.Substring(ApiTokenAuthenticationDefaults.BearerPrefix.Length).Trim();
                    if (!string.IsNullOrEmpty(bearerValue))
                    {
                        return bearerValue;
                    }
                }
            }

            if (Request.Headers.TryGetValue(ApiTokenAuthenticationDefaults.AlternativeHeaderName, out var alternativeHeader))
            {
                var headerValue = alternativeHeader.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    return headerValue.Trim();
                }
            }

            if (Request.Query.TryGetValue(ApiTokenAuthenticationDefaults.QueryParameterName, out var queryValue))
            {
                var value = queryValue.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }
    }
}
