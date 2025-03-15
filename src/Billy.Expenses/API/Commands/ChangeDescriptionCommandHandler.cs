using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Billy.Expenses.Domain.Shared;
using Serilog;

namespace Billy.Expenses.API.Commands
{
    public class ChangeDescriptionCommandHandler : IHandler<Contracts.Commands.Expenses.V1.ChangeDescription>
    {
        private readonly ApplicationService<Expense,ExpenseStreamId> _applicationService;
        private readonly ILogger _logger = Log.ForContext<ChangeDescriptionCommandHandler>();

        public ChangeDescriptionCommandHandler(ApplicationService<Expense,ExpenseStreamId> applicationService)
        {
            _applicationService = applicationService;
        }
        
        public async Task Handle(
            MessageContext<Contracts.Commands.Expenses.V1.ChangeDescription> context)
        {
            await _applicationService.Update(
                cmd => ExpenseStreamId.From(
                    ExpenseId.From(context.Message.ExpenseId)),
                (expense, cmd) => expense.ChangeDescription(
                    ExpenseDate.From(cmd.Date),
                    ExpenseTitle.From(cmd.Title),
                    ExpenseTotalAmount.From(cmd.TotalAmount),
                    ExpenseSeller.From(
                        cmd.Seller.TaxNumber,
                        cmd.Seller.Location,
                        cmd.Seller.Name,
                        cmd.Seller.PostalCode),
                    Tags.From(cmd.Tags),
                    ExpenseQuantity.From(cmd.Quantity),
                    ExpenseUnitPrice.From(cmd.UnitPrice),
                    Comments.From(cmd.Comments)),
                context.Message);

            _logger
                .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Expense {expenseId} description changed", context.Message.ExpenseId);
        }
    }
}