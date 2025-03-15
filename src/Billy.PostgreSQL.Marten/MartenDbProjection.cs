using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Billy.EventSourcing;
using Billy.PostgreSQL.Marten.Logging;
using Marten;

namespace Billy.PostgreSQL.Marten
{
    public class MartenDbProjection<T> : ISubscription
    {
        private readonly IDocumentStore _documentStore;
        private readonly Projector _projector;
        static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        static readonly string ReadModelName = typeof(T).Name;

        public MartenDbProjection(IDocumentStore documentStore,
            Projector projector)
        {
            _documentStore = documentStore;
            _projector = projector;
        }

        public async Task Project(object @event)
        {
            using var session = _documentStore.OpenSession();

            var handler = _projector(session, @event);

            if (handler == null) return;

            Log.Debug("Projecting {event} to {model}", @event, ReadModelName);

            await handler();
            await session.SaveChangesAsync();
        }

        public delegate Func<Task> Projector(
            IDocumentSession session,
            object @event
        );
    }
}
