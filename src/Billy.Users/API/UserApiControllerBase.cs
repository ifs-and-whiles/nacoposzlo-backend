using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Billy.Users.API
{
    public abstract class UserApiControllerBase : ControllerBase
    {
        protected Guid GetRequestCorrelationId() =>
            Guid.Parse(Request.HttpContext.TraceIdentifier);
    }
}
