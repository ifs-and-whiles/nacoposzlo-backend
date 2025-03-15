using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billy.EventSourcing;
using Billy.EventStore.Logging;
using EventStore.ClientAPI;

namespace Billy.EventStore
{
    public class EsAggregateStore : IAggregateStore
    {
        static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        readonly IEventStoreConnection _connection;

        public EsAggregateStore(IEventStoreConnection connection) 
            => _connection = connection;

        public async Task<bool> Exists<TIdType>(TIdType streamId)
        {
            var result = await _connection.ReadEventAsync(streamId.ToString(), 0, false);
            return result.Status != EventReadStatus.NoStream;
        }

        public async Task Save<TAggregateRoot, TIdType>(TAggregateRoot aggregate) where TAggregateRoot : AggregateRoot<TIdType>
        {
            var streamName = aggregate.Id.ToString();
            var changes = aggregate.GetChanges().ToArray();

            foreach (var change in changes)
                Log.Debug("Persisting event {event}", change.ToString());

            await _connection.AppendEvents(streamName, aggregate.Version, changes);

            aggregate.ClearChanges();
        }

        public async Task<TAggregateRoot> Load<TAggregateRoot, TIdType>(TIdType streamId) where TAggregateRoot : AggregateRoot<TIdType>
        {
            var stream = streamId.ToString();
            var aggregate = (TAggregateRoot) Activator.CreateInstance(typeof(TAggregateRoot), true);

            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            var streamEvents = new List<ResolvedEvent>();

            do
            {
                currentSlice = await _connection.ReadStreamEventsForwardAsync(stream, nextSliceStart, 200, false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);

            } while (!currentSlice.IsEndOfStream);

            Log.Debug("Loading events for the aggregate {aggregate}", aggregate.ToString());

            aggregate.Load(
                streamEvents.Select(
                    resolvedEvent => new StoredEvent(
                        version: resolvedEvent.Event.EventNumber,
                        streamId: resolvedEvent.Event.EventStreamId,
                        data: resolvedEvent.Deserialze(),
                        timestampUtc: resolvedEvent.Event.Created.ToUniversalTime())).ToArray()
            );
            
            return aggregate;
        }

        public Task<List<TAggregateRoot>> LoadAll<TAggregateRoot, TIdType>(string streamPrefix) where TAggregateRoot : AggregateRoot<TIdType>
        {
            //TODO:[FP] consider to remove Event Store implementation. Every change requires 2 implementation - Marten + EventStore. 
            //Probably we will not use it 
            throw new NotImplementedException();
        }
    }
}