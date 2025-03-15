using System.Threading.Tasks;
using Billy.Metrics;
using GreenPipes;
using MassTransit;

namespace Billy.Infrastructure.MassTransit
{
    //TODO:[FP] add missing registration to IOC
    public class MassTransitMetricsConsumerDecorator<TMessage> : IConsumer<TMessage> where TMessage : class
    {
        private readonly IConsumer<TMessage> _consumer;

        public MassTransitMetricsConsumerDecorator(IConsumer<TMessage> consumer)
        {
            _consumer = consumer;
        }
        
        public async Task Consume(ConsumeContext<TMessage> context)
        {
            using (new MessageProcessingTimer(typeof(TMessage).Name))
            { 
                await _consumer.Consume(context);
            }
        }
    }

    public class LoggingDecorator<T> : IConsumerFactory<T> where T : class
    {
        public Task Send<T1>(ConsumeContext<T1> context, IPipe<ConsumerConsumeContext<T, T1>> next) where T1 : class
        {
            throw new System.NotImplementedException();
        }

        public void Probe(ProbeContext context)
        {
            throw new System.NotImplementedException();
        }
    }
    
}