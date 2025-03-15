using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Expenses.Projections;
using Billy.PostgreSQL.Marten;
using Marten;
using QueriesContacts = Billy.Expenses.Contracts.Queries.Expenses;

namespace Billy.Expenses.API.Queries
{
    public class GetExpensesQueryHandler : IHandler<
        QueriesContacts.V1.GetExpenses, 
        QueriesContacts.V1.PagedResult<QueriesContacts.V1.ReadModels.Expense>>
    {
        private readonly IDocumentStore _documentStore;

        public GetExpensesQueryHandler(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public async Task<QueriesContacts.V1.PagedResult<QueriesContacts.V1.ReadModels.Expense>> Handle(
            MessageContext<QueriesContacts.V1.GetExpenses> context)
        {
            using var session = _documentStore.OpenSession();

            var databaseQuery = session.Query<QueriesContacts.V1.ReadModels.Expense>()
                .Where(x => x.OwnerId == context.Message.GlobalUserIdentifier);

            var totalCount = await databaseQuery.CountAsync();

            var expenses = await databaseQuery
                .Page(context.Message.PageSize, context.Message.PageNumber)
                .ToListAsync();

            return new QueriesContacts.V1.PagedResult<QueriesContacts.V1.ReadModels.Expense>()
            {
                Items = expenses.ToList(),
                TotalCount = totalCount
            };
        }
    }
}
