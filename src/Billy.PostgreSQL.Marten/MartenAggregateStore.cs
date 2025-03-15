using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billy.EventSourcing;
using Marten;
using Marten.Events;

namespace Billy.PostgreSQL.Marten
{
    
    public class MartenAggregateStore: IAggregateStore
    {
        private readonly IDocumentStore _store;

        public MartenAggregateStore(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<bool> Exists<TIdType>(TIdType streamId)
        {
            using var session = _store.LightweightSession();
            
            var state = await session.Events.FetchStreamStateAsync(streamId.ToString());
            
            return state != null;
        }

        public async Task Save<TAggregateRoot, TIdType>(TAggregateRoot aggregate) where TAggregateRoot : AggregateRoot<TIdType>
        {
            using var session = _store.LightweightSession();

            var streamName = aggregate.Id.ToString();
            
            session.Events.Append(streamName, (int)aggregate.Version, aggregate.GetChanges());

            await session.SaveChangesAsync();
        }

        public async Task<TAggregateRoot> Load<TAggregateRoot, TIdType>(TIdType streamId) where TAggregateRoot : AggregateRoot<TIdType>
        {    
            using var session = _store.LightweightSession();
            
            var aggregate = (TAggregateRoot) Activator.CreateInstance(typeof(TAggregateRoot), true);
            
            var events = await session.Events.FetchStreamAsync(streamId.ToString());
            
            aggregate.Load(events.Select(@event => new StoredEvent(
                streamId: @event.StreamKey,
                version: @event.Version,
                data: @event.Data,
                timestampUtc: @event.Timestamp.UtcDateTime
            )));
                
            return aggregate;
        }

        public async Task<List<TAggregateRoot>> LoadAll<TAggregateRoot, TIdType>(string streamPrefix) 
            where TAggregateRoot : AggregateRoot<TIdType>
        {
            using var session = _store.LightweightSession();
            
            var events = await session.Events
                .QueryAllRawEvents()
                .Where(e => e.StreamKey.StartsWith(streamPrefix))
                .ToListAsync();

            return events
                .GroupBy(events => events.StreamKey)
                .Select(groupedEventsByStreamKey =>
                {
                    var aggregate = (TAggregateRoot) Activator.CreateInstance(typeof(TAggregateRoot), true);

                    var eventsToAggregateInProperOrder = groupedEventsByStreamKey
                        .OrderBy(@event => @event.Sequence)
                        .ToList();
                        
                    aggregate.Load(eventsToAggregateInProperOrder
                        .Select(@event => new StoredEvent(
                            streamId: @event.StreamKey,
                            version: @event.Version,
                            data: @event.Data,
                            timestampUtc: @event.Timestamp.UtcDateTime
                        )));
                
                    return aggregate;
        
                })
                .ToList();
        }

    }
}