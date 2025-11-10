using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace MealieMCP.Server.Authentication
{
    public class ApiTokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public ICollection<string> Tokens { get; set; } = new List<string>();

        public bool AllowAnonymousWhenNoTokensConfigured { get; set; } = true;
    }
}
