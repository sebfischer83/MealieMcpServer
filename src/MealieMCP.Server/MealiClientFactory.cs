using MealieMCP.Server.Config;
using MealiMCP.Server.Api;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace MealieMCP.Server
{
    public class MealiClientFactory
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly HttpClient _httpClient;
        private readonly MealieSettings _mealieSettings;

        public MealiClientFactory(HttpClient httpClient, MealieSettings mealieSettings)
        {
            _authenticationProvider = new BaseBearerTokenAuthenticationProvider(new MealieAccessTokenFactory(mealieSettings));
            _httpClient = httpClient;
            _mealieSettings = mealieSettings;
        }

        public ApiClient GetClient()
        {
            var adapter = new HttpClientRequestAdapter(_authenticationProvider, httpClient: _httpClient)
            {
                BaseUrl = _mealieSettings.Url
            };
            return new ApiClient(adapter);
        }
    }

    public class MealieAccessTokenFactory : IAccessTokenProvider
    {
        private readonly MealieSettings _mealieSettings;

        public MealieAccessTokenFactory(MealieSettings mealieSettings)
        {
            _mealieSettings = mealieSettings;
        }

        public AllowedHostsValidator AllowedHostsValidator => throw new NotImplementedException();

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_mealieSettings.ApiKey);
        }
    }
}
