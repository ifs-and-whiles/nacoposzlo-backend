using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Billy.EventSourcing;
using Billy.Expenses.Domain.Expenses;
using Billy.Expenses.Projections;
using Billy.Infrastructure.Configs;
using Billy.Receipts.Contracts;
using Billy.Receipts.ReceiptRecognition;
using Marten;
using Marten.Events;
using Marten.Schema.Identity;
using Baseline.Reflection;
using Billy.Users.API.Queries;
using Billy.Users.Projections;

namespace Billy.Infrastructure.Database
{
    public class DocumentStoreFactory
    {
        public static IDocumentStore Create(DatabaseConfig config)
        {
            return DocumentStore.For(_ =>
            {
                _.Connection(config.ConnectionString);
                _.UseDefaultSerialization(casing: Casing.CamelCase);
                _.DefaultIdStrategy = (mapping, storeOptions) => new CombGuidIdGeneration();
                
                _.Storage.MappingFor(typeof(Expenses.Contracts.Queries.Expenses.V1.ReadModels.Expense));
                _.Storage.MappingFor(typeof(Queries.Receipts.V1.ReadModels.Receipt));
                _.Storage.MappingFor(typeof(Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState));
                _.Storage.MappingFor(typeof(Users.Contracts.Queries.Users.V1.ReadModels.User));

                var columns = new Expression<Func<Expenses.Contracts.Queries.Expenses.V1.ReadModels.Expense, object>>[]
                {
                    x => x.OwnerId,
                    x => x.Id
                };
                _.Schema.For<Expenses.Contracts.Queries.Expenses.V1.ReadModels.Expense>().Index(columns);
                
                _.Schema.For<Users.Contracts.Queries.Users.V1.ReadModels.User>().Index(x => x.GlobalUserIdentifier);

                _.Schema.For<Queries.Receipts.V1.ReadModels.ReceiptRecognitionProcessState>().Index(x => x.Id);

                _.Events.StreamIdentity = StreamIdentity.AsString;
                
                _.Events.AsyncProjections.Add(new ExpenseProjection());
                _.Events.AsyncProjections.Add(new UserProjection());
                
                _.Events.AddEventTypes(GetEventTypes().ToList());
            });
        }
        
        private static IEnumerable<Type> GetEventTypes()
        {
            var assemblies = new[]
            {
                typeof(Events.Expenses.V1.ExpenseAdded).Assembly, 
                typeof(Users.Domain.Events.Users.V1.UserAdded).Assembly
            };
            
            foreach (var assembly in assemblies)
            {
                foreach (var @event in assembly.GetTypes().Where(t => t.HasAttribute<EventAttribute>()))
                {
                    yield return @event;
                }
            }
        }
    }
}
