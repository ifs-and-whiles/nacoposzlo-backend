namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public class OcrResultWithFixedOrientation
    {
        public double OriginalOrientationInRadians { get; }

        public OcrResult TransformedOcr { get; }

        public OcrResultWithFixedOrientation(
            double originalOrientationInRadians,
            OcrResult transformedOcr)
        {
            OriginalOrientationInRadians = originalOrientationInRadians;
            TransformedOcr = transformedOcr;
        }
    }
}