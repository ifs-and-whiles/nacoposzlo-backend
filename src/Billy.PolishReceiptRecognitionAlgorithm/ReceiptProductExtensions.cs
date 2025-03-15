using System;
using System.Collections.Generic;
using System.Linq;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public static class ReceiptProductExtensions
    {
        public static IEnumerable<Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>> ToExplicitTypes(
            this IEnumerable<IReceiptProduct> enumerable)
        {
            return enumerable.Select(product =>
            {
                switch (product)
                {
                    case RecognizedReceiptProduct recognizedReceiptProduct:
                        return new Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>(
                            recognizedReceiptProduct);
                    case UnrecognizedReceiptProduct unrecognizedReceiptProduct:
                        return new Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>(
                            unrecognizedReceiptProduct);
                    default:
                        throw new InvalidOperationException(
                            $"Unknown product type: {((Object) product).GetType()}");
                }
            });
        }
    }
}