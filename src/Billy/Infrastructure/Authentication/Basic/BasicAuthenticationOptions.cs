using Microsoft.AspNetCore.Authentication;

namespace Billy.Infrastructure.Authentication.Basic
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}