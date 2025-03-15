using System;
using Billy.Domain.Infrastructure.EventStore;
using Marten.Events;

namespace Billy.MartenEventStore
{
    public static class MartenEventExtensions
    {
        public static StoredEvent ToStoredEvent(this IEvent @event)
        {
            return new StoredEvent(
                globalSequenceNumber: @event.Sequence,
                streamId: @event.StreamKey,
                version: @event.Version,
                data: @event.Data,
                timestampUtc: @event.Timestamp.UtcDateTime
            );
        }
    }
}
