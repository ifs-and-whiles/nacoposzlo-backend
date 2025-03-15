using System;
using System.Linq;
using System.Threading.Tasks;
using Billy.CodeReadability;
using Billy.Domain;
using Billy.Metrics;
using Billy.Receipts.Contracts;
using Billy.Receipts.Domain;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace Billy.Receipts.ReceiptRecognition.Consumers
{
    public class RecognizePrintedTextFromReceiptHandler : IConsumer<Messages.V1.ReceiptPrintedTextExtractedEvent>
    {
        private readonly IReceiptParser _receiptParser;
        private readonly ILogger _logger = Log.ForContext<RecognizePrintedTextFromReceiptHandler>();
        private readonly IReceiptRecognitionProcessStateStore _receiptRecognitionProcessStateStore;

        public RecognizePrintedTextFromReceiptHandler(
            IReceiptParser receiptParser,
            IReceiptRecognitionProcessStateStore receiptRecognitionProcessStateStore)
        {
            _receiptParser = receiptParser;
            _receiptRecognitionProcessStateStore = receiptRecognitionProcessStateStore;
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
                    var recognitionResult = RecognizeOcrResult(context);

                    await recognitionResult.Match(
                        async receiptParsed => await PublishReceiptRecognizedMessage(receiptParsed, context),
                        async receiptNotParsed =>
                        {
                            ThrowReceiptWasNotParsedCorrectly(receiptNotParsed, context);
                            await Task.CompletedTask;
                        });
                }
                catch (Exception exception)
                {
                    //We accept lack of idempotency here. User should be immediately inform that operation has failed
                    //in situation when Algorithm or Storage is not available because is waiting for the result.
                    //Receipts recognition limit won't be changed.
                    //User is able to retry operation by sending new image.
                    await MarkRecognitionAsFailed(exception, context);
                }
            }
        }

        private static void ThrowReceiptWasNotParsedCorrectly(
            ReceiptNotParsed receiptNotParsed, 
            ConsumeContext<Messages.V1.ReceiptPrintedTextExtractedEvent> context)
        {
            throw new DomainException(
                $"Receipt interpretation using algorithm {receiptNotParsed.AlgorithmName} failed for " +
                $"{context.Message.ReceiptId}", ErrorCodes.ReceiptInterpretationByAlgorithmFailed);
        }

        private async Task MarkRecognitionAsFailed(
            Exception exception,
            ConsumeContext<Messages.V1.ReceiptPrintedTextExtractedEvent> context)
        {
            _logger.Error(
                exception, 
                "Receipt {receiptId} printed text ocr recognition failed.",
                context.Message.ReceiptId);

            await _receiptRecognitionProcessStateStore.MarkRecognitionAsFailed(
                context.Message.ReceiptId);

            _logger.Information(
                "Receipt {receiptId} recognition process has been marked as failed.",
                context.Message.ReceiptId);
        }

        private static async Task PublishReceiptRecognizedMessage(
            ReceiptParsed receiptParsed,
            ConsumeContext<Messages.V1.ReceiptPrintedTextExtractedEvent> context)
        {
            await context.Publish(new Messages.V1.ReceiptRecognizedEvent()
            {
                ReceiptId = context.Message.ReceiptId,
                GlobalUserIdentifier = context.Message.GlobalUserIdentifier,
                CorrelationId = context.Message.CorrelationId,
                AlgorithmName = receiptParsed.AlgorithmName.Value,
                RecognizedReceipt = new Messages.V1.RecognizedReceipt()
                {
                    OriginalOrientation = new Messages.V1.RecognizedReceipt.Orientation
                    {
                        ValueInRadians = receiptParsed.RecognizedReceipt.OriginalOrientation.ValueInRadians
                    },
                    Amount = receiptParsed.RecognizedReceipt.Amount,
                    Date = receiptParsed.RecognizedReceipt.Date,
                    Seller = receiptParsed.RecognizedReceipt.Seller,
                    TaxNumber = receiptParsed.RecognizedReceipt.TaxNumber,
                    Problems = receiptParsed.RecognizedReceipt.Problems.Select(problem =>
                    {
                        return problem switch
                        {
                            RecognizedReceipt.Problem.AmountDifferentThanSumOfProducts =>
                            Messages.V1.RecognizedReceipt.Problem.AmountDifferentThanSumOfProducts,

                            _ => throw new NotSupportedException($"Problem '{problem}' is not recognized")
                        };
                    }).ToList(),
                    Products = receiptParsed.RecognizedReceipt.Products.Select(product =>
                        new Messages.V1.RecognizedReceipt.Product()
                        {
                            IsCorrupted = product.IsCorrupted,
                            Amount = product.Amount,
                            IsRecognized = product.IsRecognized,
                            Name = product.Name,
                            Quantity = product.Quantity,
                            UnitPrice = product.UnitPrice,
                            BoundingBox = MapDomainToMessageBoundingBox(
                                product.BoundingBox)
                        }).ToList()
                }
            });
        }

        private Either<ReceiptParsed, ReceiptNotParsed> RecognizeOcrResult(
            ConsumeContext<Messages.V1.ReceiptPrintedTextExtractedEvent> context)
        {
            var receiptImageDimensions =
                ReceiptDimensions.From(context.Message.ImageWidth, context.Message.ImageHeight);
            
            var printedTextRecognitionResult = new PrintedTextRecognitionResult(
                context.Message.PrintedTextRecognition.Detections.Select(detectedText => 
                    new DetectedText(
                        text: detectedText.Text,
                        
                        leftTop: ReceiptPoint.FromNormalized(
                            detectedText.LeftTop.X, 
                            detectedText.LeftTop.Y, 
                            receiptImageDimensions), 
                        
                        rightTop: ReceiptPoint.FromNormalized(
                            detectedText.RightTop.X, 
                            detectedText.RightTop.Y, 
                            receiptImageDimensions),
                        
                        leftBottom: ReceiptPoint.FromNormalized(
                            detectedText.LeftBottom.X, 
                            detectedText.LeftBottom.Y, 
                            receiptImageDimensions),
                        
                        rightBottom: ReceiptPoint.FromNormalized(
                            detectedText.RightBottom.X, 
                            detectedText.RightBottom.Y, 
                            receiptImageDimensions))));
            
            return _receiptParser.Parse(printedTextRecognitionResult, receiptImageDimensions);
        }

        private static Messages.V1.BoundingBox MapDomainToMessageBoundingBox(
            BoundingBox boundingBox)
        {
            return new Messages.V1.BoundingBox
            {
                RightTop = MapDomainToMessagePoint(boundingBox.RightTop),
                LeftTop = MapDomainToMessagePoint(boundingBox.LeftTop),
                LeftBottom = MapDomainToMessagePoint(boundingBox.LeftBottom),
                RightBottom = MapDomainToMessagePoint(boundingBox.RightBottom)
            };
        }

        private static Messages.V1.NormalizedPoint MapDomainToMessagePoint(
            ReceiptPoint receiptPoint)
        {
            return new Messages.V1.NormalizedPoint
            {
                X = receiptPoint.GetAbsolute().X,
                Y = receiptPoint.GetAbsolute().Y
            };
        }
    }
}
