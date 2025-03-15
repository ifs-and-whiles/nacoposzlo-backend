namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class RawDetectedText
    {
        public string Text { get; set; }
        public BoundingBox Box { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}