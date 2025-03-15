using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure
{
    public static class ReceiptProductAssertionExtensions
    {
        public static void IsRecognizedAnd(
            this IReceiptProduct product, 
            params Action<RecognizedReceiptProductAssertions>[] assertions)
        {
            switch (product)
            {
                case UnrecognizedReceiptProduct unrecognized:
                    throw new InvalidOperationException(
                        $"Product was not recognized: {unrecognized.RawText}");
                case RecognizedReceiptProduct recognized:
                {
                    var assertion = new RecognizedReceiptProductAssertions(recognized);
                    
                    foreach (var action in assertions)
                    {
                        action(assertion);
                    }

                    return;
                }
                default:
                    throw new InvalidOperationException(
                        $"Unknown product implementation: {product.GetType()}");
            }

        }

        public static void IsNotRecognizedAnd(this IReceiptProduct product, Action<string> rawTextAssertion)
        {
            switch (product)
            {
                case UnrecognizedReceiptProduct unrecognized:
                {
                    rawTextAssertion(unrecognized.RawText);
                    return;
                }
                    
                case RecognizedReceiptProduct recognized:
                    throw new InvalidOperationException(
                        $"Product was recognized: {recognized}");
                default:
                    throw new InvalidOperationException(
                        $"Unknown product implementation: {product.GetType()}");
            }
        }
    }
}