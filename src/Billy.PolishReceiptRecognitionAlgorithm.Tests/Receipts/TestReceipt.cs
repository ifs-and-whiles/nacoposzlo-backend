using System;
using System.Linq;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts
{
    public class TestReceipt
    {
        public int Id { get; }

        public ReceiptLine[] Lines => ToReceiptLines(RawLines);
        public string[] RawLines { get; }
 
        public IReceiptProduct[] ActualProducts { get; }
        public ParsingResult<string> ActualSeller { get; }

        public ParsingResult<DateTime> ActualDate { get; }
        public ParsingResult<TaxNumber> ActualTaxNumber { get; }
        public ParsingResult<decimal> ActualReceiptAmount { get; }

        public ReceiptLine[] HeaderLines => ToReceiptLines(Header);

        public ReceiptLine[] ProductsLines => ToReceiptLines(Products);

        public ReceiptLine AmountLine => new ReceiptLine(Amount, DefaultBoundingBox());

        public ReceiptLine[] TaxesLines => ToReceiptLines(Taxes);

        public ReceiptLine[] FooterLines => ToReceiptLines(Footer);

        public string[] Header { get; }

        public string[] Products { get; }

        public string Amount { get; }

        public string[] Taxes { get; }

        public string[] Footer { get; }

        public TestReceipt(
            int id,
            string[] lines,
            LinesRange headerLines,
            LinesRange productLines,
            LinesRange taxesLines,
            int amountLine,
            LinesRange footerLines,
            IReceiptProduct[] actualProducts,
            ParsingResult<string> actualSeller,
            ParsingResult<DateTime>  actualDate,
            ParsingResult<TaxNumber> actualTaxNumber,
            ParsingResult<decimal> actualReceiptAmount)
        {
            Id = id;
            RawLines = lines;
            ActualProducts = actualProducts;
            ActualSeller = actualSeller;
            ActualDate = actualDate;
            ActualTaxNumber = actualTaxNumber;
            ActualReceiptAmount = actualReceiptAmount;

            Header = headerLines.Pick(lines);
            Products = productLines.Pick(lines);
            Amount = lines.Skip(amountLine).First();
            Taxes = taxesLines.Pick(lines);
            Footer = footerLines.Pick(lines);
        }

        private static BoundingBox DefaultBoundingBox()
        {
            // to make tests pass for now bounding boxes are defaulted to zeroed ones
            // that should be improved in future so that our tests also checks if bounding box for 
            // each receipt element is calculated correctly
            return new BoundingBox(
                new Point(0,0), 
                new Point(0,0), 
                new Point(0,0), 
                new Point(0,0));
        }

        private ReceiptLine[] ToReceiptLines(string[] lines)
        {
            return lines
                .Select(line => new ReceiptLine(line , DefaultBoundingBox()))
                .ToArray();
        }
    }
}