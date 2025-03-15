using System;
using System.Threading.Tasks;
using Billy.Metrics;
using Billy.Receipts.Contracts;
using Billy.Receipts.Domain;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace Billy.Receipts.ReceiptRecognition.Consumers
{
    public class StorePrintedTextRecognitionResultHandler : IConsumer<Messages.V1.ReceiptPrintedTextExtractedEvent>
    {
        private readonly IReceiptStore _receiptStore;
        private readonly ILogger _logger = Log.ForContext<StorePrintedTextRecognitionResultHandler>();
        
        public StorePrintedTextRecognitionResultHandler(IReceiptStore receiptStore)
        {
            _receiptStore = receiptStore;
        }
        
        public async Task Consume(ConsumeContext<Messages.V1.ReceiptPrintedTextExtractedEvent> context)
        {
            //I need to find a way how to create generic decorator for mass transit consumer.
            using (LogContext.PushProperty("CorrelationId", context.CorrelationId))
            using (LogContext.PushProperty("GlobalUserIdentifier", context.Message.GlobalUserIdentifier))
            using (new MessageProcessingTimer(context.Message.GetType().Name))
            {
                try
                {
                    //PrintedTextRecognition is stored only due to maintenance reason. Stored model is not used by any component.
                    //That's the reason why we can store queue model directly
                    var printedTextRecognitionStoreId = await _receiptStore.StorePrintedTextRecognitionResult(
                        context.Message.PrintedTextRecognition, 
                        context.Message.ReceiptId);
                
                    _logger.Information(
                        "Receipt {receiptId} PrintedTextRecognition has been successfully " +
                        "stored on {receiptStoreName} with id {storageId}.",
                        context.Message.ReceiptId,
                        _receiptStore.StorageName,
                        printedTextRecognitionStoreId);
                }
                catch (KeyAlreadyExistsOnStorageException exception)
                {
                    _logger.Information(
                        "Receipt {receiptId} PrintedTextRecognition already exist on storage. {@errorDescription}. " +
                        "Saving operation skipped.",
                        context.Message.ReceiptId,
                        new
                        {
                            _receiptStore.StorageName,
                            DuplicatedKey = exception.KeyValue
                        });
                }
            }
        }
    }
}