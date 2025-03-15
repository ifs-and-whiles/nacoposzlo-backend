using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Billy.Api.Mobile.Utils
{
    public abstract class MobileApiControllerBase : ControllerBase
    {
        protected Guid GetRequestCorrelationId() =>
            Guid.Parse(Request.HttpContext.TraceIdentifier);
    }
}
