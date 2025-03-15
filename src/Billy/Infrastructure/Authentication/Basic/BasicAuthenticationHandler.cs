using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Billy.Infrastructure.Authentication.Basic
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            var endpoint = Context.GetEndpoint();
            
            if (DoesEndpointSupportAnonymousCalls(endpoint))
                return Task.FromResult(AuthenticateResult.NoResult());

            if (!DoesRequestContainAuthorizationHeader())
                return Task.FromResult(AuthenticateResult.NoResult());

            if (!DoesRequestContainBasicAuthenticationRequestHeader())
                return Task.FromResult(AuthenticateResult.NoResult());
            
            try
            {
                var (userName, password) = ExtractUserNameAndPasswordFromRequestAuthorizationHeader();

                if (!IsUserValid(userName, password))
                    return Task.FromResult(AuthenticateResult.Fail("Invalid UserName or Password"));
                    
                var claims = new[] {
                    new Claim(ClaimTypes.Name, userName),
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
            
            

            bool DoesEndpointSupportAnonymousCalls(Endpoint endpoint) =>
                endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

            bool DoesRequestContainAuthorizationHeader() =>
                Request.Headers.ContainsKey("Authorization");

            bool DoesRequestContainBasicAuthenticationRequestHeader()
            {
                var header = Request.Headers[HeaderNames.Authorization];
                return header.ToString().StartsWith($"{AuthenticationScheme.BasicScheme} ", StringComparison.OrdinalIgnoreCase);
            }

            bool IsUserValid(string userName, string password) =>
                string.Equals(userName, Options.UserName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(password, Options.Password, StringComparison.OrdinalIgnoreCase);

            (string userName, string password) ExtractUserNameAndPasswordFromRequestAuthorizationHeader()
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers[HeaderNames.Authorization]);
                
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                
                var username = credentials[0];
                var password = credentials[1];

                return (username, password);
            }
        }

     
    }
}