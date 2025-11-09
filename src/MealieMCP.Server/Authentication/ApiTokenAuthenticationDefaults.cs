namespace MealieMCP.Server.Authentication
{
    public static class ApiTokenAuthenticationDefaults
    {
        public const string AuthenticationScheme = "ApiToken";
        public const string PolicyName = "ApiToken";
        public const string AuthorizationHeaderName = "Authorization";
        public const string BearerPrefix = "Bearer ";
        public const string AlternativeHeaderName = "X-Api-Token";
        public const string QueryParameterName = "access_token";
    }
}
