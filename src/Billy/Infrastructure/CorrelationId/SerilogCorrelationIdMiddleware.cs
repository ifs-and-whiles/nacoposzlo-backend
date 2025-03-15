using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Billy.Infrastructure.CorrelationId
{
    public class SerilogCorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string BearerUserNameClaimType = "username";
        private const string BasicAuthUserNameClaimType = ClaimTypes.Name;
        public SerilogCorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public Task Invoke(HttpContext context)
        {
            //Depending on the auth - claim types are different. Supported auth: bearer, basic
            var user = context
                .User?
                .Claims?
                .FirstOrDefault(p => p.Type == BearerUserNameClaimType || 
                    p.Type == BasicAuthUserNameClaimType);
            
            using (LogContext.PushProperty("GlobalUserIdentifier", user?.Value))
            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
            {
                return _next(context);
            }
        }
    }
}
