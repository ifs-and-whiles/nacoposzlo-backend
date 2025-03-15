using System;
using System.Text;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public class RecognizedReceiptProduct : IReceiptProduct
    {
        public ParsingResult<string> Name { get; }
        public ParsingResult<string> Unit { get; }
        public ParsingResult<decimal> UnitPrice { get;}
        public ParsingResult<decimal> Quantity { get;}
        public ParsingResult<decimal> Amount { get;  }
        public ParsingResult<string> TaxTag { get; }
        public BoundingBox BoundingBox { get; }
        
        public RecognizedReceiptProduct(
            ParsingResult<string> name,
            ParsingResult<decimal> quantity, 
            ParsingResult<string> unit, 
            ParsingResult<decimal> unitPrice,
            ParsingResult<decimal> amount,
            ParsingResult<string> taxTag,
            BoundingBox boundingBox)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
            Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            TaxTag = taxTag ?? throw new ArgumentNullException(nameof(taxTag));
            BoundingBox = boundingBox ?? throw new ArgumentNullException(nameof(boundingBox));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            
            Name.Value.Match(
                value => builder.AppendLine($"N:{value}"), 
                () => builder.AppendLine("N: ---"));

            Quantity.Value.Match(
                value => builder.AppendLine($" Q:{value}"), 
                () => builder.AppendLine($" Q: ---"));

            Unit.Value.Match(
                value => builder.AppendLine($" U:{value}"), 
                () => builder.AppendLine($" U: ---"));

            UnitPrice.Value.Match(
                value => builder.AppendLine($" UP:{value}"), 
                () => builder.AppendLine($" UP: ---"));

            Amount.Value.Match(
                value => builder.AppendLine($" A:{value}"), 
                () => builder.AppendLine($" A: ---"));

            TaxTag.Value.Match(
                value => builder.AppendLine($" TT:{value}"),
                () => builder.AppendLine($" TT: ---"));

            return builder.ToString();
        }
    }
}