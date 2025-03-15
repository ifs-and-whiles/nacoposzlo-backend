using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    public class TooManyDecimalPlacesInProductAmountFix : IReceiptProductFix
    {
        public RecognizedReceiptProduct TryFix(RecognizedReceiptProduct original)
        {
            return original.Amount.Value.Match(
                value => TryFixAmountDecimalPlaces(original, value),
                () => original);
        }

        private static RecognizedReceiptProduct TryFixAmountDecimalPlaces(
            RecognizedReceiptProduct original, decimal amount)
        {
            if (HasTwoDecimalPlaces(amount)) return original;

            return new RecognizedReceiptProduct(
                name: original.Name,
                quantity: original.Quantity,
                unit: original.Unit,
                unitPrice: original.UnitPrice,
                amount: ParsingResult<decimal>.WithProblems(
                    value: decimal.Round(amount, 2, MidpointRounding.ToNegativeInfinity),
                    rawValue: original.Amount.RawValue,
                    problems: original.Amount.Problems), 
                taxTag: original.TaxTag,
                boundingBox: original.BoundingBox);
        }

        private static bool HasTwoDecimalPlaces(decimal value)
        {
            return decimal.Round(value, 2) == value;
        }
    }
}