using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billy.Infrastructure.Configs;
using Polly;
using RabbitMQ.Client;

namespace Billy.IntegrationTests.Infrastructure.Queue
{
    public class RabbitMqQueue : IDisposable
    {
        private readonly RabbitMqConfig _config;
        private IConnection _connection;


        public RabbitMqQueue(RabbitMqConfig config)
        {
            _config = config;
            _connection = ConfigureConnection(config);
        }

        public void TryPurgeQueue(string queue)
        {
            // RabbitMQ throws exception when queue does not exist. There is no possibility to check that queue exists.
            try
            {
                using var channel = _connection.CreateModel();

                channel.QueuePurge(queue);
            }
            catch (Exception e)
            { }
        }
 
        public async Task<BasicGetResult> StartPolling(
            string queueName,
            string expectedMessageType, 
            params (string queueName, string messageType)[] faultQueues)
        {
            using var channel = _connection.CreateModel();

            ConfigureListeningQueue(queueName, expectedMessageType, channel);

            ConfigureFaultQueues(faultQueues, channel);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var response = await Retry.WhenResult(
                result => result == null,
                async () => await ReadFromQueues(channel, queueName, faultQueues.Select(x=> x.queueName).ToArray()));

            if (response == null)
                throw new Exception("No response in queue.");

            return response;
        }

        private void ConfigureListeningQueue(string queueName, string messageType, IModel channel)
        {
            channel.QueueDeclare(queueName, false, false);
            channel.QueueBind(queueName, messageType, routingKey: "", arguments: null);
        }

        private void ConfigureFaultQueues((string queueName, string messageType)[] faultQueues, IModel channel)
        {
            foreach (var faultQueue in faultQueues)
            {
                channel.QueueDelete(faultQueue.queueName);
                channel.QueueDeclare(faultQueue.queueName,false, false);
                channel.ExchangeDeclare($"MassTransit:Fault--{faultQueue.messageType}--", "fanout", true);
                channel.QueueBind(faultQueue.queueName, $"MassTransit:Fault--{faultQueue.messageType}--", routingKey: "", arguments: null);
            }
        }

        private async Task<BasicGetResult> ReadFromQueues(IModel channel, string responseQueue, string[] errorQueues)
        {
            return await Task.Run(() =>
            {
                foreach (var errorQueue in errorQueues)
                {
                    var error = channel.BasicGet(errorQueue, true);

                    if (error != null)
                        throw new Exception($"There is an error message in the error queue. {error.Body.ConvertToString()}");
                }
                return channel.BasicGet(responseQueue, true);
            });
        }

        private IConnection ConfigureConnection(RabbitMqConfig config)
        {
            var hostUri = new Uri(config.HostUrl);
            var connectionFactory = new ConnectionFactory
            {
                HostName = hostUri.Host,
                UserName = config.User,
                Password = config.Password,
                Port = hostUri.Port
            };

            return connectionFactory.CreateConnection();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }

    public class Retry
    {
        private static int _retryCount = 10;
        private static int _retryInterval = 500;

        public static Task<TResult> WhenResult<TResult>(Func<TResult, bool> retryPredicate, Func<Task<TResult>> action)
        {
            return Policy
                .HandleResult(retryPredicate)
                .WaitAndRetryAsync(
                    retryCount: _retryCount,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromMilliseconds(_retryInterval)).ExecuteAsync(action);
        }
    }

    public static class ByteExtensions
    {
        public static string ConvertToString(this ReadOnlyMemory<byte> body)
        {
            using var stream = new MemoryStream(body.Span.ToArray());
            using var streamReader = new StreamReader(stream);

            return streamReader.ReadToEnd();
        }
            
    }
}
