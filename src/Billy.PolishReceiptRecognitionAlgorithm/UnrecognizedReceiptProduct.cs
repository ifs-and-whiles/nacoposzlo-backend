using System;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public class UnrecognizedReceiptProduct : IReceiptProduct
    {
        public string RawText { get; }

        public BoundingBox BoundingBox { get; }

        public UnrecognizedReceiptProduct(string text, BoundingBox boundingBox)
        {
            RawText = text ?? throw new ArgumentNullException(nameof(text));
            BoundingBox = boundingBox ?? throw new ArgumentNullException(nameof(boundingBox));
        }

        public override string ToString()
        {
            return $"Unrecognized: {RawText}";
        }
    }
}