using System;

namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class ReceiptLine
    {
        public string Text { get; }
        public BoundingBox BoundingBox { get; }

        public ReceiptLine(string text, BoundingBox boundingBox)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            BoundingBox = boundingBox ?? throw new ArgumentNullException(nameof(boundingBox));
        }

        public ReceiptLine Trim()
        {
            return new ReceiptLine(
                text: Text.Trim(),
                boundingBox: BoundingBox);
        }

        public override string ToString()
        {
            return Text;
        }
    }
}