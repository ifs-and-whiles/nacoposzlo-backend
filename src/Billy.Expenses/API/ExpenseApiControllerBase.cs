using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Billy.Expenses.API
{
    public abstract class ExpenseApiControllerBase: ControllerBase
    {
        protected Guid GetRequestCorrelationId() =>
            Guid.Parse(Request.HttpContext.TraceIdentifier);
    }
}
