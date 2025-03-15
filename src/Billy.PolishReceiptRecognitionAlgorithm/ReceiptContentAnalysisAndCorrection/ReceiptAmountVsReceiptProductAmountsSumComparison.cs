using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    public class ReceiptAmountVsReceiptProductAmountsSumComparison : IReceiptAnalyzer
    {
        public string[] AnalyzeReceipt(Receipt receipt)
        {
            return receipt.Amount.Value.Match(
                amount => CompareAmountWithSumOfProducts(amount, receipt.Products),
                () => CompareAmountWithSumOfProducts(0, receipt.Products));
        }

        public string[] CompareAmountWithSumOfProducts(
            decimal receiptAmount, 
            IEnumerable<Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>> products)
        {
            var sumOfProductAmounts = products
                .AllLeftValues()
                .Sum(recognizedProduct => recognizedProduct.Amount.Value.GetOrElse(0));

            return receiptAmount == sumOfProductAmounts
                ? new string[0]
                : new[]
                {
                    ReceiptProblem.AmountDifferentThanSumOfProducts
                };
        }
    }
}