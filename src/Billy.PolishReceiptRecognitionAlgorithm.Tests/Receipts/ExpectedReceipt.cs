using System;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts
{
    public class ExpectedReceipt
    {
        public RecognizedReceiptProduct[] Products { get; }
        public ParsingResult<string> Seller { get; }
        public ParsingResult<DateTime> Date { get; }
        public ParsingResult<TaxNumber> TaxNumber { get; }
        public ParsingResult<decimal> Amount { get; }

        public ExpectedReceipt(
            RecognizedReceiptProduct[] products,
            ParsingResult<string> seller,
            ParsingResult<DateTime> date,
            ParsingResult<TaxNumber> taxNumber,
            ParsingResult<decimal> amount)
        {
            Products = products;
            Seller = seller;
            Date = date;
            TaxNumber = taxNumber;
            Amount = amount;
        }
    }
}