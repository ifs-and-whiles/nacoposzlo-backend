using System;
using Microsoft.AspNetCore.Mvc;

namespace Billy.MobileApi
{
    public abstract class MobileApiControllerBase : ControllerBase
    {
        protected Guid GetRequestCorrelationId() =>
            Guid.Parse(Request.HttpContext.TraceIdentifier);

        protected string GetUserId()
        {
            var claims = Request.HttpContext.User.FindFirst(p => p.Type == "username");
            return claims.Value;
        }
    }
}