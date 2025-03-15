using System;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Billy.Expenses.Domain.Shared;
using Billy.IdGeneration;
using Serilog;

namespace Billy.Expenses.API.Commands
{
    public class CreateExpenseCommandHandler : IHandler<
            Contracts.Commands.Expenses.V1.Create, 
            Contracts.Commands.Expenses.V1.CreateResponse>
    {
        private readonly ApplicationService<Expense, ExpenseStreamId> _applicationService;
        private readonly IIdGenerator _idGenerator;
        private readonly ILogger _logger = Log.ForContext<CreateExpenseCommandHandler>();

        public CreateExpenseCommandHandler(
            ApplicationService<Expense,ExpenseStreamId> applicationService,
            IIdGenerator idGenerator)
        {
            _applicationService = applicationService;
            _idGenerator = idGenerator;
        }
        
        public async Task<Contracts.Commands.Expenses.V1.CreateResponse> Handle(
            MessageContext<Contracts.Commands.Expenses.V1.Create> context)
        {
            var expenseId = _idGenerator.NewId();

            await _applicationService.Create(
                cmd => ExpenseStreamId.From(ExpenseId.From(expenseId)),
                (cmd) => Expense.Create(
                    ExpenseId.From(expenseId),
                    GlobalUserIdentifier.From(cmd.GlobalUserIdentifier),
                    ExpenseDate.From(cmd.Date),
                    ExpenseTitle.From(cmd.Title),
                    ExpenseTotalAmount.From(cmd.TotalAmount),
                    ExpenseSeller.From(
                        cmd.Seller.TaxNumber,
                        cmd.Seller.Location,
                        cmd.Seller.Name,
                        cmd.Seller.PostalCode),
                    Tags.From(cmd.Tags),
                    CreationDate.From(DateTimeOffset.UtcNow),
                    ReceiptId.From(cmd.ReceiptId),
                    ExpenseQuantity.From(cmd.Quantity),
                    ExpenseUnitPrice.From(cmd.UnitPrice),
                    Comments.From(cmd.Comments)),
                context.Message);

            _logger
                .ForContext("GlobalUserIdentifier", context.Message.GlobalUserIdentifier)
                .ForContext("CorrelationId", context.CorrelationId)
                .Information("Expense {expenseId} created", expenseId);

            return new Contracts.Commands.Expenses.V1.CreateResponse()
            {
                ExpenseId = expenseId
            };
        }
    }
}