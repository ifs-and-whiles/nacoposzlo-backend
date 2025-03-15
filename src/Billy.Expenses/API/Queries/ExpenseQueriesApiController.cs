using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Expenses.Projections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueriesContacts = Billy.Expenses.Contracts.Queries.Expenses.V1;
namespace Billy.Expenses.API.Queries
{
    [Authorize(Policy = "ServiceUsers")]
    [ApiController, Route(ExpensesApiRouteConsts.Prefix + "/v1/expenses/queries")]
    public class ExpenseQueriesCommandApiController: ExpenseApiControllerBase
    {
        private readonly IHandler<QueriesContacts.GetExpense, QueriesContacts.ReadModels.Expense> _getExpenseQueryHandler;
        private readonly IHandler<QueriesContacts.GetExpenses, QueriesContacts.PagedResult<QueriesContacts.ReadModels.Expense>> 
            _getManyExpensesQueryHandler;

        public ExpenseQueriesCommandApiController(
            IHandler<QueriesContacts.GetExpense, QueriesContacts.ReadModels.Expense> getExpenseQueryHandler,
            IHandler<QueriesContacts.GetExpenses, QueriesContacts.PagedResult<QueriesContacts.ReadModels.Expense>> getManyExpensesQueryHandler)
        {
            _getExpenseQueryHandler = getExpenseQueryHandler;
            _getManyExpensesQueryHandler = getManyExpensesQueryHandler;
        }

        [HttpGet]
        [Route("get-single")]
        public async Task<QueriesContacts.ReadModels.Expense> GetExpense(
            [FromQuery] QueriesContacts.GetExpense query)
        {
            return await _getExpenseQueryHandler.Handle(new MessageContext<QueriesContacts.GetExpense>
            {
                Message = query,
                CorrelationId = GetRequestCorrelationId()
            });
        }

        [HttpGet]
        [Route("get-all-for-user")]
        public async Task<QueriesContacts.PagedResult<QueriesContacts.ReadModels.Expense>> GetExpenses(
            [FromQuery] QueriesContacts.GetExpenses query)
        {
            return await _getManyExpensesQueryHandler.Handle(new MessageContext<QueriesContacts.GetExpenses>
            {
                Message = query,
                CorrelationId = GetRequestCorrelationId()
            });
        }
    }
}
