using System.Collections.Generic;
using System.Linq;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;

namespace Billy.PolishReceiptRecognitionAlgorithm.Sections
{
    public class ReceiptSections
    {
        public HeaderSection Header { get; }

        public ProductsSection Products { get; }

        public TaxesSection Taxes { get; }

        public AmountSection Amount { get; }

        public FooterSection Footer { get; }

        public ReceiptSections(
            HeaderSection header,
            ProductsSection products, 
            TaxesSection taxes, 
            AmountSection amount, 
            FooterSection footer)
        {
            Header = header;
            Products = products;
            Taxes = taxes;
            Amount = amount;
            Footer = footer;
        }

        public interface IReceiptSection
        {
            IReadOnlyCollection<ReceiptLine> Lines { get; }
        }

        public class HeaderSection : IReceiptSection
        {
            public IReadOnlyCollection<ReceiptLine> Lines { get; }

            public HeaderSection(IEnumerable<ReceiptLine> lines)
            {
                Lines = lines.ToArray();
            }
        }

        public class ProductsSection : IReceiptSection
        {
            public IReadOnlyCollection<ReceiptLine> Lines { get; }

            public ProductsSection(IEnumerable<ReceiptLine> lines)
            {
                Lines = lines
                    .Select(line => line.Trim())
                    .ToArray();
            }
        }

        public class TaxesSection : IReceiptSection
        {
            public IReadOnlyCollection<ReceiptLine> Lines { get; }

            public TaxesSection(IEnumerable<ReceiptLine> lines)
            {
                Lines = lines.ToArray();
            }
        }

        public class AmountSection : IReceiptSection
        {
            public ReceiptLine Line { get; }

            public IReadOnlyCollection<ReceiptLine> Lines =>
                Line != null ? new[] {Line} : new ReceiptLine[0];

            public AmountSection(ReceiptLine line)
            {
                Line = line;
            }
        }

        public class FooterSection : IReceiptSection
        {
            public IReadOnlyCollection<ReceiptLine> Lines { get; }

            public FooterSection(IEnumerable<ReceiptLine> lines)
            {
                Lines = lines.ToArray();
            }
        }
    }
}