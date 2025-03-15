using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billy.Domain.Infrastructure.EventStore;
using Billy.Metrics;
using Marten;

namespace Billy.MartenEventStore
{
    public class MartenEventStore : IEventStore
    {
        private readonly IDocumentStore _store;

        public MartenEventStore(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<IReadOnlyList<StoredEvent>> LoadStream(string streamId)
        {
            using (new DbQueryTimer("es_load_stream"))
            using (var session = _store.LightweightSession())
            {
                var events = await session.Events.FetchStreamAsync(streamId);
                return events.Select(e => e?.ToStoredEvent()).ToArray();
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> LoadAllEvents(long startAtSequence = 0, int pageSize = 1000)
        {
            using (new DbQueryTimer("es_load_all_events"))
            using (var session = _store.LightweightSession())
            {
                var events = await session.Events.QueryAllRawEvents()
                    .Where(e => e.Sequence > startAtSequence)
                    .OrderBy(x => x.Sequence)
                    .Take(pageSize)
                    .ToListAsync();

                return events.Select(e => e?.ToStoredEvent()).ToArray();
            }
        }

        public async Task<long> GetLastSequenceNumber()
        {
            using (new DbQueryTimer("es_get_last_sequence_number"))
            using (var session = _store.LightweightSession())
            {
                var eventQuery = session.Events.QueryAllRawEvents();

                return await eventQuery.AnyAsync() ?
                    await eventQuery.MaxAsync(x => x.Sequence) : 0L;
            }
        }

        public async Task AppendEvents(string streamId, IEnumerable<object> events, long? expectedVersion = null)
        {
            using (new DbQueryTimer("es_append_events"))
            using (var session = _store.LightweightSession())
            {
                if (expectedVersion.HasValue)
                    session.Events.Append(streamId, (int)expectedVersion.Value, events.ToArray());
                else
                    session.Events.Append(streamId, events.ToArray());

                await session.SaveChangesAsync();
            }
        }
    }
}
