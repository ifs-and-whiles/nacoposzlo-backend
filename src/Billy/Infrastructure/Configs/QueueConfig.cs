namespace Billy.Infrastructure.Configs
{
    public class QueueConfig
    {
        public QueueProvider QueueProvider { get; set; } = QueueProvider.InMemory;
        public RabbitMqConfig RabbitMqConfig { get; set; }
        public SQSConfig SqsConfig { get; set; }
    }

    public enum QueueProvider
    {
        InMemory = 1,
        RabbitMQ = 2,
        SQS = 3
    }
}