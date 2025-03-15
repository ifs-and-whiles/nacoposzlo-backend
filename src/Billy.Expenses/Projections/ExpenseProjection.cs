using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Marten;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Billy.CQRS;
using Billy.Domain;
using Billy.Expenses.Contracts;
using Billy.Metrics;
using Marten.Events.Projections;
using Marten.Events.Projections.Async;
using Marten.Storage;

namespace Billy.Expenses.Projections
{
    public class ExpenseProjection : IProjection
    {
        public Type[] Consumes { get; } =
        {
            typeof(Events.Expenses.V1.ExpenseAdded),
            typeof(Events.Expenses.V1.ExpenseDeleted),
            typeof(Events.Expenses.V1.ExpenseDescriptionChanged),
            typeof(Events.Expenses.V1.ExpenseTagsChanged)
        };

        public void Apply(IDocumentSession session, EventPage page) =>
            throw new NotImplementedException("Synchronous version is not used");

        public async Task ApplyAsync(IDocumentSession session, EventPage page, CancellationToken token)
        {
            foreach (var @event in page.Events)
                await Apply(@event.Data, session);
        }
        
        private static async Task<Queries.Expenses.V1.ReadModels.Expense> LoadOrTrow(
            IDocumentSession session, Guid id)
        {
            var result = await session.LoadAsync<Queries.Expenses.V1.ReadModels.Expense>(id);
            if(result == null)
                throw new NotFoundException($"Expense for {id} does not exist");
            return result;
        }
        
        private async Task Apply(object @event, IDocumentSession session)
        {
            using (new ProjectionEventTimer(nameof(ExpenseProjection), @event))
            {
                switch (@event)
                {
                    case Events.Expenses.V1.ExpenseAdded e:
                        await Create(e);
                        break;
                    case Events.Expenses.V1.ExpenseDeleted e:
                        await Delete(e);
                        break;
                    case Events.Expenses.V1.ExpenseTagsChanged e:
                        await Update(e.ExpenseId, expense =>
                        {
                            expense.Tags = e.Tags;
                        });
                        break;
                    case Events.Expenses.V1.ExpenseDescriptionChanged e:
                        await Update(e.ExpenseId, expense =>
                        {
                            expense.Date = e.Date;
                            expense.TotalAmount = e.TotalAmount;
                            expense.Tags = e.Tags;
                            expense.Title = e.Title;
                            expense.UnitPrice = e.UnitPrice;
                            expense.Quantity = e.Quantity;
                            expense.Comments = e.Comments;
                            expense.Seller = new Queries.Expenses.V1.ReadModels.Seller()
                            {
                                Location = e.Seller?.Location,
                                Name = e.Seller?.Name,
                                TaxNumber = e.Seller?.TaxNumber,
                                PostalCode = e.Seller?.PostalCode
                            };
                        });
                        break;
                }
            }

            async Task Create(Events.Expenses.V1.ExpenseAdded e)
            {
                var expense = new Queries.Expenses.V1.ReadModels.Expense()
                {
                    Id = e.ExpenseId,
                    OwnerId = e.OwnerId,
                    Date = e.Date,
                    ReceiptId = e.ReceiptId,
                    Seller = new Queries.Expenses.V1.ReadModels.Seller()
                    {
                        Location = e.Seller?.Location,
                        Name = e.Seller?.Name,
                        TaxNumber = e.Seller?.TaxNumber,
                        PostalCode = e.Seller?.PostalCode
                    },
                    Title = e.Title,
                    TotalAmount = e.TotalAmount,
                    Quantity = e.Quantity,
                    UnitPrice = e.UnitPrice,
                    Tags = e.Tags ?? new List<string>(),
                    Comments = e.Comments
                };

                session.Store(expense);
                await session.SaveChangesAsync();
            }

            async Task Delete(Events.Expenses.V1.ExpenseDeleted e)
            {
                var expense = await LoadOrTrow(session, e.ExpenseId);

                session.Delete(expense);

                await session.SaveChangesAsync();
            }

            async Task Update(Guid id, Action<Queries.Expenses.V1.ReadModels.Expense> updateAction)
            {
                var expense = await LoadOrTrow(session, id);

                updateAction.Invoke(expense);

                session.Store(expense);

                await session.SaveChangesAsync();
            }
        }

        public void EnsureStorageExists(ITenant tenant)
        {
        }

        public AsyncOptions AsyncOptions { get; } = new AsyncOptions();
    }
}
