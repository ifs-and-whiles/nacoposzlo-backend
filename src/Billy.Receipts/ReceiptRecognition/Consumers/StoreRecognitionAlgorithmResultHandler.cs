using System.Threading.Tasks;
using Billy.Metrics;
using Billy.Receipts.Contracts;
using Billy.Receipts.Domain;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace Billy.Receipts.ReceiptRecognition.Consumers
{
    public class StoreRecognitionAlgorithmResultHandler : IConsumer<Messages.V1.ReceiptRecognizedEvent>
    {
        private readonly IReceiptStore _receiptStore;
        private readonly ILogger _logger = Log.ForContext<StoreRecognitionAlgorithmResultHandler>();
        
        public StoreRecognitionAlgorithmResultHandler(IReceiptStore receiptStore)
        {
            _receiptStore = receiptStore;
        }
        
        public async Task Consume(ConsumeContext<Messages.V1.ReceiptRecognizedEvent> context)
        {
            //I need to find a way how to create generic decorator for mass transit consumer.
            using (LogContext.PushProperty("CorrelationId", context.CorrelationId))
            using (LogContext.PushProperty("GlobalUserIdentifier", context.Message.GlobalUserIdentifier))
            using (LogContext.PushProperty("AlgorithmName", context.Message.AlgorithmName))
            using (new MessageProcessingTimer(context.Message.GetType().Name))
            {
                try
                {
                    //RecognitionAlgorithmResult is stored only due to maintenance reason.
                    //Stored model is not used by any component.
                    //That's the reason why we can store queue model directly

                    var recognizedReceiptStorageId = await _receiptStore.StoreRecognizedReceipt(
                        receipt: new
                        {
                            context.Message.AlgorithmName,
                            context.Message.RecognizedReceipt
                        }, 
                        receiptId: context.Message.ReceiptId);

                    _logger.Information(
                        "Recognized receipt {receiptId} has been successfully " +
                        "stored on {receiptStoreName} with id {storageId}.",
                        context.Message.ReceiptId,
                        _receiptStore.StorageName,
                        recognizedReceiptStorageId);
                }
                catch (KeyAlreadyExistsOnStorageException exception)
                {
                    _logger.Warning(
                        "Recognized receipt {receiptId} already exist on storage. {@details}. " +
                        "Skipping saving operation.",
                        context.Message.ReceiptId,
                        new
                        {
                            DuplicatedKey = exception.KeyValue,
                            _receiptStore.StorageName,
                        });
                }
            }
        }
    }
}