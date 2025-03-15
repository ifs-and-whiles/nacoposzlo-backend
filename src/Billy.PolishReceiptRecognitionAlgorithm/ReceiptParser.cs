using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Billy.CodeReadability;
using Billy.CollectionTools;
using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public static class ReceiptParser
    { 
        //todo test failing scenarios
        public static Either<Receipt, ReceiptNotParsed> Parse(
            OcrResult ocrResult, 
            Action<Exception> logException = null)
        {
            var logger = GetLoggerOrDefault(logException);

            var ocrParser = new ReceiptOcrParser(
                new RaysHitCounterSpatialAnalysis(),
                new AverageOrientationFixAlgorithm(), 
                new ArtifactsRemovalAlgorithm());

            try
            {
                var parserResult = ocrParser.GetLines(
                    ocrResult);

                return ParseReceipt(
                    parserResult, 
                    logger);
            }
            catch (Exception e)
            {
                logger(e);
                return new ReceiptNotParsed();
            }
        }

        private static Receipt ParseReceipt(
           ReceiptOcrParser.Result parserResult, 
            Action<Exception> logger)
        {
            var receiptSections = TryGetSections(parserResult.Lines, logger);

            var seller = TryExecute(
                operation: () => ReceiptHeaderParser.ParseSeller(receiptSections.Header),
                inCaseOfFailure: () => BoxedParsingResult<string>.ExceptionOccured,
                logger: logger);

            var taxNumber = TryExecute(
                operation: () => ReceiptHeaderParser.ParseTaxNumber(receiptSections.Header),
                inCaseOfFailure: () => BoxedParsingResult<TaxNumber>.ExceptionOccured,
                logger: logger);

            var date = TryExecute(
                operation: () => ReceiptDateParser.ParseDate(receiptSections.Header, receiptSections.Footer),
                inCaseOfFailure: () => BoxedParsingResult<DateTime>.ExceptionOccured,
                logger: logger);

            var amount = TryExecute(
                operation: () => ReceiptAmountParser.GetAmount(receiptSections.Amount),
                inCaseOfFailure: () => BoxedParsingResult<decimal>.ExceptionOccured,
                logger: logger);

            var products = TryParseProducts(
                receiptSections, 
                logger);

            var receipt = new Receipt(
                seller: seller,
                taxNumber: taxNumber,
                date: date,
                amount: amount,
                products: products,
                problems: new string[0],
                originalOrientation: new Receipt.Orientation(
                    parserResult.OriginalReceiptOrientationInRadians));

            var enhancedReceipt = ReceiptPostProcessor.TryAnalyzeAndFix(
                receipt);

            return enhancedReceipt;
        }

        private static Action<Exception> GetLoggerOrDefault(Action<Exception> logException)
        {
            return logException ?? DefaultLogAction;

            void DefaultLogAction(Exception exc){}
        }

        private static Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>[] TryParseProducts(
            ReceiptSections receiptSections, 
            Action<Exception> logger)
        {
            var products = TryExecute(
                operation: () => ReceiptProductsParser.ParseProducts(
                    receiptSections.Products),
                inCaseOfFailure: () => receiptSections
                    .Products
                    .Lines
                    .Select(
                        line => (IReceiptProduct) new UnrecognizedReceiptProduct(
                            text: line.Text,
                            boundingBox: line.BoundingBox))
                    .ToArray(),
                logger: logger);

           return products.ToExplicitTypes().ToArray();
        }

        private static ReceiptSections TryGetSections(
            ReceiptLine[] lines, 
            Action<Exception> logger)
        {
            return TryExecute(
                operation: () => ReceiptSectionsParser.GetSections(lines),
                inCaseOfFailure: () => new ReceiptSections(
                    header: new ReceiptSections.HeaderSection(
                        new ReceiptLine[0]),
                    products: new ReceiptSections.ProductsSection(
                        lines),
                    taxes: new ReceiptSections.TaxesSection(
                        new ReceiptLine[0]),
                    amount: new ReceiptSections.AmountSection(
                        null), 
                    footer: new ReceiptSections.FooterSection(
                        new ReceiptLine[0])),
                logger: logger);
        }

        private static T TryExecute<T>(
            Func<T> operation, 
            Func<T> inCaseOfFailure,
            Action<Exception> logger)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                logger(ex);
                return inCaseOfFailure();
            }
        }
    }
}
