
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billy.CodeReadability;
using Billy.Metrics;
using Billy.PolishReceiptRecognitionAlgorithm;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.Receipts.Domain;
using Newtonsoft.Json;
using Serilog;
using ReceiptNotParsed = Billy.Receipts.Domain.ReceiptNotParsed;

namespace Billy.Receipts.Recognition.PolishReceiptRecognitionAlgorithm
{
    public class PolishReceiptParser : IReceiptParser
    {
        private static readonly ILogger Logger = Log.ForContext<PolishReceiptParser>();
        private static readonly AlgorithmName AlgorithmName = AlgorithmName.From("PolishReceiptRecognitionAlgorithm");

        public Either<ReceiptParsed, ReceiptNotParsed> Parse(
            PrintedTextRecognitionResult printedTextRecognitionResult,
            ReceiptDimensions receiptDimensions)
        {
            using (new RecognitionAlgorithmTimer(AlgorithmName.Value))
            {
                var ocrResult = MapPrintedTextRecognitionResultToAlgorithmInput(
                    printedTextRecognitionResult);

                var parserResult = ReceiptParser.Parse(
                    ocrResult: ocrResult,
                    logException: exception => Logger.Error(
                        exception: exception,
                        messageTemplate: "Problem occured during parsing a receipt"));

                return parserResult.Match(
                    receipt => MapReceipt(receipt, receiptDimensions),
                    receiptNotParsed => HandleReceiptWasNotParsedCorrectly());
            }
        }

        private OcrResult MapPrintedTextRecognitionResultToAlgorithmInput(
            PrintedTextRecognitionResult printedTextRecognitionResult)
        {
            var detections = printedTextRecognitionResult
                .Detections
                .Select(detection => new RawDetectedText
                {
                    Text = detection.Text,
                    Box = MapDomainToAlgorithmBoundingBox(detection)
                })
                .ToList();

            return new OcrResult(
                detections);
        }


        private Either<ReceiptParsed, ReceiptNotParsed> HandleReceiptWasNotParsedCorrectly() => 
            new Either<ReceiptParsed, ReceiptNotParsed>(new ReceiptNotParsed(AlgorithmName));
        
        private static ReceiptParsed MapReceipt(
            Receipt receipt,
            ReceiptDimensions receiptDimensions)
        {
            var problems = MapReceiptProblems(receipt);
            var products = MapProducts(receipt.Products, receiptDimensions);

            var recognizedReceipt = new RecognizedReceipt
            {
                Amount = receipt.Amount.Value.Match(
                    value => (decimal?) value,
                    () => null),

                Date = receipt.Date.Value.Match(
                    value => (DateTime?) value,
                    () => null),

                Seller = receipt.Seller.Value.GetOrElse(null),

                TaxNumber = receipt.TaxNumber.Value.Match(
                    value => value.Value,
                    () => null),
                
                Problems = problems.ToList(),
                Products = products.ToList(),

                OriginalOrientation = new RecognizedReceipt.Orientation(
                    receipt.OriginalOrientation.ValueInRadians)
            };
            
            return new ReceiptParsed(
                recognizedReceipt, 
                AlgorithmName, 
                RawAlgorithmResult.From(ToJson(receipt)));

        }

        private static IEnumerable<RecognizedReceipt.Problem> MapReceiptProblems(Receipt receipt)
        {
            return receipt.Problems.Select(problem =>
            {
                return problem switch
                {
                    ReceiptProblem.AmountDifferentThanSumOfProducts => 
                        RecognizedReceipt.Problem.AmountDifferentThanSumOfProducts,

                    _ => throw new NotSupportedException($"Problem '{problem}' is not recognized")
                };
            });
        }

        private static IEnumerable<RecognizedReceipt.Product> MapProducts(
            IEnumerable<Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>> products,
            ReceiptDimensions receiptDimensions)
        {
            return products.Select(
                leftFunc: MapRecognizedProduct,
                rightFunc: MapUnrecognizedProduct);
            
            RecognizedReceipt.Product MapRecognizedProduct(
                RecognizedReceiptProduct recognized)
            {
                var isCorrupted = recognized.Quantity.Problems.Any()
                                  || recognized.Amount.Problems.Any()
                                  || recognized.Name.Problems.Any()
                                  || recognized.UnitPrice.Problems.Any();

                return new RecognizedReceipt.Product
                {
                    IsRecognized = true,
                    IsCorrupted = isCorrupted,
                    Amount = recognized.Amount.Value.Match(value => (decimal?) value, () => null),
                    Name = recognized.Name.Value.GetOrElse(null),
                    Quantity = recognized.Quantity.Value.Match(value => (decimal?) value, () => null),
                    UnitPrice = recognized.UnitPrice.Value.Match(value => (decimal?) value, () => null),
                    BoundingBox = MapAlgorithmToDomainBoundingBox(recognized.BoundingBox, receiptDimensions)
                };
            }
            
            RecognizedReceipt.Product MapUnrecognizedProduct(
                UnrecognizedReceiptProduct unrecognized)
            {
                return new RecognizedReceipt.Product
                {
                    IsRecognized = false, 
                    IsCorrupted = true, 
                    Name = unrecognized.RawText,
                    BoundingBox = MapAlgorithmToDomainBoundingBox(
                        unrecognized.BoundingBox,
                        receiptDimensions)
                };
            }
        }
        

        private static string ToJson(Receipt receipt) => JsonConvert.SerializeObject(receipt);

        private static Billy.PolishReceiptRecognitionAlgorithm.OcrJson.BoundingBox MapDomainToAlgorithmBoundingBox(
            DetectedText detectedText)
        {
            return new Billy.PolishReceiptRecognitionAlgorithm.OcrJson.BoundingBox(
                leftTop: MapDomainToAlgorithmPoint(detectedText.LeftTop),
                leftBottom: MapDomainToAlgorithmPoint(detectedText.LeftBottom),
                rightTop: MapDomainToAlgorithmPoint(detectedText.RightTop),
                rightBottom: MapDomainToAlgorithmPoint(detectedText.RightBottom));
        }

        private static Point MapDomainToAlgorithmPoint(
            ReceiptPoint receiptPoint)
        {
            return new Point(
                x: receiptPoint.GetAbsolute().X,
                y: receiptPoint.GetAbsolute().Y);
        }

        private static Domain.BoundingBox MapAlgorithmToDomainBoundingBox(
            Billy.PolishReceiptRecognitionAlgorithm.OcrJson.BoundingBox boundingBox,
            ReceiptDimensions receiptDimensions)
        {
            return new Domain.BoundingBox
            {
                RightTop = MapAlgorithmToDomainPoint(boundingBox.RightTop),
                LeftTop = MapAlgorithmToDomainPoint(boundingBox.LeftTop),
                LeftBottom = MapAlgorithmToDomainPoint(boundingBox.LeftBottom),
                RightBottom = MapAlgorithmToDomainPoint(boundingBox.RightBottom)
            };
            
            ReceiptPoint MapAlgorithmToDomainPoint(
                Point point)
            {
                return ReceiptPoint.FromAbsolute(
                    point.X,
                    point.Y,
                    receiptDimensions);
            }
        }

    }
}
