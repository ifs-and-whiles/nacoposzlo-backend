using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;

namespace Billy.PolishReceiptRecognitionAlgorithm
{
    public interface IReceiptProduct
    {
        BoundingBox BoundingBox { get; }
    }
}