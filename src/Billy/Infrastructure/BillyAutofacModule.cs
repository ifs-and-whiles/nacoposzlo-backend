using Autofac;
using Billy.EventSourcing;
using Billy.EventStore;
using Billy.Expenses.API.Commands;
using Billy.IdGeneration;
using Billy.Infrastructure.Configs;
using Billy.Infrastructure.Database;
using Billy.Infrastructure.IdGeneration;
using Billy.PostgreSQL.Marten;
using EventStore.ClientAPI;
using Marten;

namespace Billy.Infrastructure
{
    public class BillyAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<SequentialIdGenerator>().As<IIdGenerator>().SingleInstance();
            
            // Event Store
            builder.RegisterType<MartenAggregateStore>().As<IAggregateStore>();
            
            // Read DB
            builder.Register(ctx => DocumentStoreFactory.Create(ctx.Resolve<DatabaseConfig>()))
                .As<IDocumentStore>()
                .SingleInstance();


        }
    }
}
