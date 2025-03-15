using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Serilog;

namespace Billy.Expenses.API.Commands
{
    public class DeleteExpenseCommandHandler : IHandler<Contracts.Commands.Expenses.V1.Delete>
    {
        private readonly ApplicationService<Expense, ExpenseStreamId> _applicationService;
        private readonly ILogger _logger = Log.ForContext<DeleteExpenseCommandHandler>();

        public DeleteExpenseCommandHandler(ApplicationService<Expense,ExpenseStreamId> applicationService)
        {
            _applicationService = applicationService;
        }

        public async Task Handle(MessageContext<Contracts.Commands.Expenses.V1.Delete> context)
        {
            await _applicationService.Update(
                cmd => ExpenseStreamId.From(
                    ExpenseId.From(context.Message.ExpenseId)),
                (expense, cmd) => expense.Delete(),
                context.Message);

            _logger
                .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Expense {expenseId} deleted", context.Message.ExpenseId);
        }
    }
}