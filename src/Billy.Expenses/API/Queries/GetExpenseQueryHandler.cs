using System.Linq;
using System.Threading.Tasks;
using Billy.CQRS;
using Marten;

namespace Billy.Expenses.API.Queries
{
    public class GetExpenseQueryHandler : IHandler<
            Contracts.Queries.Expenses.V1.GetExpense, 
            Contracts.Queries.Expenses.V1.ReadModels.Expense>
    {
        private readonly IDocumentStore _documentStore;

        public GetExpenseQueryHandler(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public async Task<Contracts.Queries.Expenses.V1.ReadModels.Expense> Handle(
            MessageContext<Contracts.Queries.Expenses.V1.GetExpense> context)
        {
            using var session = _documentStore.OpenSession();

            var expense = await session
                .Query<Contracts.Queries.Expenses.V1.ReadModels.Expense>()
                .Where(x => x.Id == context.Message.ExpenseId)
                .Where(x => x.OwnerId == context.Message.GlobalUserIdentifier)
                .FirstOrDefaultAsync();

            if (expense == null)
                throw new NotFoundException(
                    $"Expense with Id {context.Message.ExpenseId} and OwnerId {context.Message.GlobalUserIdentifier} not found");

            return expense;
        }
    }
}