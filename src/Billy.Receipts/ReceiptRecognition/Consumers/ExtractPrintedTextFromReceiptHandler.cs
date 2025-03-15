using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Billy.Domain;
using Billy.Metrics;
using Billy.Receipts.Contracts;
using Billy.Receipts.Domain;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace Billy.Receipts.ReceiptRecognition.Consumers
{
    public class ExtractPrintedTextFromReceiptHandler : IConsumer<Messages.V1.ReceiptAddedToPrintedTextRecognitionEvent>
    {
        private readonly ILogger _logger = Log.ForContext<ExtractPrintedTextFromReceiptHandler>();

        private readonly IOcr _ocr;
        private readonly IReceiptRecognitionProcessStateStore _receiptRecognitionProcessStateStore;

        public ExtractPrintedTextFromReceiptHandler(
            IOcr ocr, 
            IReceiptRecognitionProcessStateStore receiptRecognitionProcessStateStore)
        {
            _ocr = ocr;
            _receiptRecognitionProcessStateStore = receiptRecognitionProcessStateStore;
        }

        public async Task Consume(ConsumeContext<Messages.V1.ReceiptAddedToPrintedTextRecognitionEvent> context)
        {
            using (LogContext.PushProperty("CorrelationId", context.CorrelationId))
            using (LogContext.PushProperty("GlobalUserIdentifier", context.Message.GlobalUserIdentifier))
            using (new MessageProcessingTimer(context.Message.GetType().Name))
            {
                try
                {
                    var ocrResult = await RecognizePrintedTextOnImage(
                        ReceiptId.From(context.Message.ReceiptId),
                        ReceiptDimensions.From(
                            context.Message.ImageWidth, 
                            context.Message.ImageHeight));
                    
                    await PublishReceiptPrintedTextExtractedEvent(
                        context, 
                        ocrResult);
                }
                catch (Exception exception)
                {
                    //We accept lack of idempotency here. User should be immediately inform that operation has failed
                    //in situation when OCR provider or Storage is not available because is waiting for the result.
                    //Receipts recognition limit won't be changed.
                    //User is able to retry operation by sending new image.
                    await MarkRecognitionAsFailed(context, exception);
                }
            }
        }

        private async Task MarkRecognitionAsFailed(
            ConsumeContext<Messages.V1.ReceiptAddedToPrintedTextRecognitionEvent> context,
            Exception exception)
        {
            _logger.Error(
                exception, 
                "Receipt {receiptId} printed text recognition failed. Marking recognition as failed.",
                context.Message.ReceiptId);

            await _receiptRecognitionProcessStateStore.MarkRecognitionAsFailed(
                context.Message.ReceiptId);

            _logger.Information(
                "Receipt {receiptId} recognition process has been marked as failed",
                context.Message.ReceiptId);
        }

        private async Task PublishReceiptPrintedTextExtractedEvent(
            ConsumeContext<Messages.V1.ReceiptAddedToPrintedTextRecognitionEvent> context,
            PrintedTextRecognitionResult recognitionResult)
        {
            await context.Publish(new Messages.V1.ReceiptPrintedTextExtractedEvent()
            {
                ReceiptId = context.Message.ReceiptId,
                GlobalUserIdentifier = context.Message.GlobalUserIdentifier,
                CorrelationId = context.Message.CorrelationId,
                ImageHeight = context.Message.ImageHeight,
                ImageWidth = context.Message.ImageWidth,
                PrintedTextRecognition = new Messages.V1.PrintedTextRecognition(
                    recognitionResult.Detections.Select(detectedText =>
                            new Messages.V1.PrintedTextRecognition.DetectedText(
                                text: detectedText.Text,
                                leftTop: new Messages.V1.NormalizedPoint
                                {
                                        //TODO:[FP] move to method
                                        X = detectedText.LeftTop.GetNormalized().X,
                                    Y = detectedText.LeftTop.GetNormalized().Y
                                },
                                rightTop: new Messages.V1.NormalizedPoint
                                {
                                    X = detectedText.RightTop.GetNormalized().X,
                                    Y = detectedText.RightTop.GetNormalized().Y
                                },
                                leftBottom: new Messages.V1.NormalizedPoint
                                {
                                    X = detectedText.LeftBottom.GetNormalized().X,
                                    Y = detectedText.LeftBottom.GetNormalized().Y
                                },
                                rightBottom: new Messages.V1.NormalizedPoint
                                {
                                    X = detectedText.RightBottom.GetNormalized().X,
                                    Y = detectedText.RightBottom.GetNormalized().Y
                                }))
                        .ToList())
            });
        }

        private async Task<PrintedTextRecognitionResult> RecognizePrintedTextOnImage(
            ReceiptId receiptId,
            ReceiptDimensions receiptDimensions)
        {
            _logger.Information(
                "Receipt {receiptId} printed text recognition has been started using OCR provider",
                receiptId);

            var ocrResult = await _ocr.RecognizeImage(receiptId, receiptDimensions);

            _logger.Information(
                @"Receipt {receiptId} printed text recognition has been finished using OCR {{ProviderName}} provider. {{@details}}",
                receiptId,
                _ocr.ProviderName,
                new
                {
                    Results = new {
                        NumberOfRecognizedTextElements = ocrResult.Detections.Count
                    }
                });

            return ocrResult;
        }
    
    }
}
