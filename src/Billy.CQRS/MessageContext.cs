using System;

namespace Billy.CQRS
{
    public class MessageContext<TCommand>
    {
        public TCommand Message { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
