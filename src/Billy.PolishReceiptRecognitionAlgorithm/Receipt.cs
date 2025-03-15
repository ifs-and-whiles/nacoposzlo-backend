using System;
using System.Collections.Generic;
using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public class ReceiptNotParsed
    {

    }

    public class Receipt
    {
        public Orientation OriginalOrientation { get; }
        public BoxedParsingResult<string> Seller { get; }
        public BoxedParsingResult<TaxNumber> TaxNumber { get; }
        public BoxedParsingResult<DateTime> Date { get; }
        public BoxedParsingResult<decimal> Amount { get; }
        public IReadOnlyCollection<Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>> Products { get; }
        public IReadOnlyCollection<string> Problems { get; }

        public Receipt(
            BoxedParsingResult<string> seller,
            BoxedParsingResult<TaxNumber> taxNumber,
            BoxedParsingResult<DateTime> date,
            BoxedParsingResult<decimal> amount,
            IReadOnlyCollection<Either<RecognizedReceiptProduct, UnrecognizedReceiptProduct>> products, 
            IReadOnlyCollection<string> problems,
            Orientation originalOrientation)
        {
            Seller = seller;
            TaxNumber = taxNumber;
            Date = date;
            Amount = amount;
            Products = products;
            Problems = problems;
            OriginalOrientation = originalOrientation;
        }

        public class Orientation
        {
            public double ValueInRadians { get;  }

            public Orientation(double valueInRadians)
            {
                ValueInRadians = valueInRadians;
            }
        }
    }
}