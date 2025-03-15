using System.Threading.Tasks;
using Billy.CodeReadability;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Billy.Expenses.Domain.Shared;
using Serilog;

namespace Billy.Expenses.API.Commands
{
    public class ChangeTagsCommandHandler : IHandler<Contracts.Commands.Expenses.V1.ChangeTags>
    {
        private readonly ApplicationService<Expense, ExpenseStreamId> _applicationService;
        private readonly ILogger _logger = Log.ForContext<ChangeTagsCommandHandler>();
        public ChangeTagsCommandHandler(ApplicationService<Expense,ExpenseStreamId> applicationService)
        {
            _applicationService = applicationService;
        }
        
        public async Task Handle(
            MessageContext<Contracts.Commands.Expenses.V1.ChangeTags> context)
        {
            await _applicationService.Update(
                cmd => ExpenseStreamId.From(
                    ExpenseId.From(context.Message.ExpenseId)),
                (expense, cmd) => expense.ChangeTags(
                    Tags.From(cmd.Tags)),
                context.Message);

            _logger
                .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Expense {expenseId} tags changed", context.Message.ExpenseId);
        }
    }
}
