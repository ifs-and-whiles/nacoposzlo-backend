using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Billy.Infrastructure.Configs;
using Billy.IntegrationTests.Infrastructure;
using Billy.IntegrationTests.Infrastructure.Api;
using Billy.IntegrationTests.Infrastructure.Queue;
using Billy.Receipts.Domain;
using Billy.Receipts.Infrastructure.Configs;
using Billy.Receipts.ReceiptRecognition.Consumers;
using Billy.Receipts.Storage.Minio;
using Marten.Util;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Billy.Receipts.Tests
{
    public class ReceiptsTestFixture : TestFixture
    {
        private IBusControl _busControl;
        private RabbitMqQueue _queue;
        private RabbitMqConfig _rabbitMqConfig;
        private readonly IMinioStorage _minioStorage;
        private ApiMock _apiMock;

        public ReceiptsTestFixture(HostFixture hostFixture, ITestOutputHelper output)
            : base(hostFixture, output)
        {
            _rabbitMqConfig = hostFixture.AppConfiguration.GetSection(ConfigKeys.QueueConfig).Get<QueueConfig>().RabbitMqConfig;
         
            var minioStorageConfig = hostFixture.AppConfiguration.GetSection(ConfigKeys.StorageConfig).Get<StorageConfig>().MinioStorageConfig;

            _queue = new RabbitMqQueue(_rabbitMqConfig);
            _minioStorage = new MinioStorage(minioStorageConfig);
        }

        protected void delete_all_receipts_queues()
        {
            _queue.TryPurgeQueue("receipts-to-printed-text-recognition-queue");
            _queue.TryPurgeQueue("receipts-to-recognize-queue");
            _queue.TryPurgeQueue("recognized-receipts-to-process-queue");
            _queue.TryPurgeQueue("completed-receipt-recognitions-queue");
            _queue.TryPurgeQueue("completed-receipt-recognitions-queue_skipped");
        }

        protected void start_message_bus()
        {
            _busControl = IntegrationTestsMassTransitConfiguration.ConfigureBus(_rabbitMqConfig);
            _busControl.Start();
        }

        protected async Task publish_message<TMessage>(TMessage message) where TMessage : class =>
            await _busControl.Publish(message);



        protected async Task<TExpectedMessage> wait_for_queue_message<TExpectedMessage>(
            string expectedQueueName,
            params (string faultQueue, Type faultMessageType)[] faultQueues)
        {
            var expectedMessageExchangeName = $"{typeof(TExpectedMessage).Namespace}:{GetTypeName(typeof(TExpectedMessage))}";

            var expectedFaultQueues = faultQueues.Select(x => ($"{x.faultQueue}", $"{x.faultMessageType.Namespace}:{GetTypeName(x.faultMessageType)}"));

            var message = await _queue.StartPolling($"{expectedQueueName}",$"{expectedMessageExchangeName}",
                expectedFaultQueues.ToArray());
            
            var messageBody = message.Body.ConvertToString();

            return JsonConvert.DeserializeObject<MassTransitDefaultMessage<TExpectedMessage>>(messageBody).Message;
        }

        private string GetTypeName(Type type)
        {
            if (type.MemberType == MemberTypes.NestedType)
            {
                return string.Concat(GetTypeName(type.DeclaringType), "-", type.Name);
            }
            return type.Name;
        }

        protected async Task clear_receipts_storage() =>
            await _minioStorage.ClearBucket();

        protected async Task add_to_storage<TModel>(TModel jsonModel, string relativePath, string fileName) =>
            await _minioStorage.Save(JsonConvert.SerializeObject(jsonModel), relativePath, fileName);

        protected async Task<byte[]> read_image_from_storage(string filePath) =>
            await _minioStorage.ReadBinaryFile(filePath);

        protected async Task<T> read_json_from_storage<T>(string filePath) =>
            await _minioStorage.ReadJsonFile<T>(filePath);

        protected async Task<string> read_string_from_storage(string filePath) =>
            await _minioStorage.ReadStringFile(filePath);

        protected byte[] load_test_receipt_image(params string[] paths)
        {
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            using var stream = embeddedProvider.GetFileInfo(Path.Combine(paths)).CreateReadStream();

            using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        protected string read_test_json_as_string(params string[] paths)
        {
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            using var reader = embeddedProvider.GetFileInfo(Path.Combine(paths)).CreateReadStream();
            using var streamReader = new StreamReader(reader);

            return streamReader.ReadToEnd();
        }
        protected T read_test_json<T>(params string[] paths)
        {
            var content = read_test_json_as_string(paths);
            return JsonConvert.DeserializeObject<T>(content);
        }
        

        public override void Dispose()
        {
            base.Dispose();
            _apiMock?.Dispose();
        }

        class MassTransitDefaultMessage<TMessage>
        {
            public TMessage Message { get; set; }
        }

    }

    public static class ReceiptsExtensions
    {
        public static string ToBase64(this byte[] image) =>
            Convert.ToBase64String(image);
    }

}
