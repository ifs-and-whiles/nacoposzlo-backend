using System;
using System.Threading.Tasks;
using Billy.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Billy.Expenses.Contracts.Commands.Expenses;

namespace Billy.Expenses.API.Commands
{
    [Authorize(Policy = "ServiceUsers")]
    [ApiController, Route(ExpensesApiRouteConsts.Prefix + "/v1/expenses/commands")]
    public class ExpenseCommandApiController: ExpenseApiControllerBase
    {
        private readonly IHandler<V1.Create, V1.CreateResponse> _createExpenseCommandHandler;
        private readonly IHandler<V1.Delete> _deleteExpenseCommandHandler;
        private readonly IHandler<V1.ChangeDescription> _changeDescriptionCommandHandler;
        private readonly IHandler<V1.ChangeTags> _changeTagsCommandHandler;

        public ExpenseCommandApiController(
            IHandler<V1.Create, V1.CreateResponse> createExpenseCommandHandler,
            IHandler<V1.Delete> deleteExpenseCommandHandler,
            IHandler<V1.ChangeDescription> changeDescriptionCommandHandler,
            IHandler<V1.ChangeTags> changeTagsCommandHandler)
        {
            _createExpenseCommandHandler = createExpenseCommandHandler;
            _deleteExpenseCommandHandler = deleteExpenseCommandHandler;
            _changeDescriptionCommandHandler = changeDescriptionCommandHandler;
            _changeTagsCommandHandler = changeTagsCommandHandler;
        }


        [HttpPost, Route("create")]
        public async Task<V1.CreateResponse> CreateExpense(
            V1.Create command)
        {
            return await _createExpenseCommandHandler.Handle(new MessageContext<V1.Create>
            {
                Message = command,
                CorrelationId = GetRequestCorrelationId()
            });
        }

        [HttpPost, Route("delete")]
        public async Task DeleteExpense(
            V1.Delete command)
        {
            await _deleteExpenseCommandHandler.Handle(new MessageContext<V1.Delete>
            {
                Message = command,
                CorrelationId = GetRequestCorrelationId()
            });
        }

        [HttpPost, Route("change-description")]
        public async Task ChangeDescription(
            V1.ChangeDescription command)
        {
            await _changeDescriptionCommandHandler.Handle(new MessageContext<V1.ChangeDescription>
            {
                Message = command,
                CorrelationId = GetRequestCorrelationId()
            });
        }

        [HttpPost, Route("change-tags")]
        public async Task ChangeTags(
            V1.ChangeTags command)
        {
            await _changeTagsCommandHandler.Handle(new MessageContext<V1.ChangeTags>
            {
                Message = command,
                CorrelationId = GetRequestCorrelationId()
            });
        }
    }
}