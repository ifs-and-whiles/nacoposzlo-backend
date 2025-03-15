using System;
using Microsoft.AspNetCore.Authentication;

namespace Billy.Infrastructure.Authentication.Basic
{
    public static class BasicAuthenticationExtensions
    {
        public static AuthenticationBuilder AddBasicAuthentication(
            this AuthenticationBuilder builder)
        {
            return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>
                (AuthenticationScheme.BasicScheme, (_ => {}));
        }

        public static AuthenticationBuilder AddBasicAuthentication(
            this AuthenticationBuilder builder,
            Action<BasicAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>
                (AuthenticationScheme.BasicScheme, configureOptions);
        }
    }
}