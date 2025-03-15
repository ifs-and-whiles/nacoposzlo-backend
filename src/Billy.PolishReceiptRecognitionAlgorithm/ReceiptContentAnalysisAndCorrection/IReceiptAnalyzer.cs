namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    public interface IReceiptAnalyzer
    {
        string[] AnalyzeReceipt(Receipt receipt);
    }
}