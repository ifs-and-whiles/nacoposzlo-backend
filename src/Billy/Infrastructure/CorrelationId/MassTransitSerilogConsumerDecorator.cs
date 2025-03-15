using System;
using System.Threading.Tasks;
using MassTransit;
using Serilog.Context;

namespace Billy.Infrastructure.CorrelationId
{
    public class MassTransitSerilogConsumerDecorator<TMessage> : IConsumer<TMessage> where TMessage : class
    {
        private readonly IConsumer<TMessage> _consumer;

        public MassTransitSerilogConsumerDecorator(IConsumer<TMessage> consumer)
        {
            _consumer = consumer;
        }
        public Task Consume(ConsumeContext<TMessage> context)
        {
            using (LogContext.PushProperty("CorrelationId", context.CorrelationId))
            {
                return _consumer.Consume(context);
            }
        }
    }

    public class MassTransitMessageCorrelationIdNotFound : Exception
    {
        public MassTransitMessageCorrelationIdNotFound(string message) : base(message)
        {
            
        }
    }
}
