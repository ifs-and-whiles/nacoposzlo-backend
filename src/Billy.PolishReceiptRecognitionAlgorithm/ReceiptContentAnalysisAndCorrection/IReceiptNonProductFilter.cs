using Billy.CodeReadability;

namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    public interface IReceiptNonProductFilter
    {
        bool IsNonProduct(Option<string> productName);
    }
}