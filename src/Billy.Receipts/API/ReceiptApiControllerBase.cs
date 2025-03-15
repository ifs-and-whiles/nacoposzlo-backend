using System;
using Microsoft.AspNetCore.Mvc;

namespace Billy.Receipts.API
{
    public abstract class ReceiptApiControllerBase : ControllerBase
    {
        protected Guid GetRequestCorrelationId() =>
            Guid.Parse(Request.HttpContext.TraceIdentifier);
    }
}