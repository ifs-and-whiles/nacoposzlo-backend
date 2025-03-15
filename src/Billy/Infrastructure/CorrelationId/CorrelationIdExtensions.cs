using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Billy.Infrastructure.CorrelationId
{
    public static class CorrelationIdExtensions
    {
        public static IApplicationBuilder UseHttpCorrelationIdMiddleware(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<HttpCorrelationIdMiddleware>();
        }

        public static IApplicationBuilder UseHttpCorrelationIdMiddleware(this IApplicationBuilder app, string header)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseHttpCorrelationIdMiddleware(new CorrelationIdOptions
            {
                Header = header
            });
        }

        public static IApplicationBuilder UseHttpCorrelationIdMiddleware(this IApplicationBuilder app, CorrelationIdOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<HttpCorrelationIdMiddleware>(Options.Create(options));
        }
    }
}
