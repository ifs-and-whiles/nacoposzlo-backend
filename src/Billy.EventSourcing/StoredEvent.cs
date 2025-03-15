using System;

namespace Billy.EventSourcing
{
    public class StoredEvent
    {
        public StoredEvent(
            long version, 
            string streamId, 
            object data, 
            DateTime timestampUtc)
        {
            Version = version;
            StreamId = streamId;
            Data = data;
            TimestampUtc = timestampUtc;
        }

        public long Version { get; }

        public string StreamId { get; }
        
        public object Data { get; }

        public DateTime TimestampUtc { get; }
    }
}