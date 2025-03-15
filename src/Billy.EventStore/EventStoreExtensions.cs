using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billy.EventSourcing;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Billy.EventStore
{
    public static class EventStoreExtensions
    {
        public static Task AppendEvents(
            this IEventStoreConnection connection,
            string streamName,
            long version,
            params object[] events)
        {
            if (events == null || !events.Any()) return Task.CompletedTask;

            var preparedEvents = events
                .Select(
                    @event =>
                        new EventData(
                            Guid.NewGuid(),
                            TypeMapper.GetTypeName(@event.GetType()),
                            true,
                            Serialize(@event),
                            Serialize(
                                new EventMetadata
                                {
                                    ClrType = @event.GetType().FullName
                                }
                            )
                        )
                )
                .ToArray();

            return connection.AppendToStreamAsync(
                streamName,
                version,
                preparedEvents
            );
        }

        static byte[] Serialize(object data) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

        public static ResolvedEvent[] OnlyCustomEvents(this ResolvedEvent[] events)
        {
            var systemEventsPrefix = "$";
            return events.Where(x => !x.Event.EventType.StartsWith(systemEventsPrefix)).ToArray();
        }

        public static ResolvedEvent[] WithAssignedOriginalPosition(this ResolvedEvent[] events) =>
            events.Where(x => x.OriginalPosition.HasValue).ToArray();
       
        public static ResolvedEvent GetTheNewestCustomEvent(this ResolvedEvent[] events)
        {
            return events
                .OnlyCustomEvents()
                .WithAssignedOriginalPosition()
                .OrderByDescending(x => x.OriginalPosition.Value.CommitPosition)
                .FirstOrDefault();
        }


    }
}