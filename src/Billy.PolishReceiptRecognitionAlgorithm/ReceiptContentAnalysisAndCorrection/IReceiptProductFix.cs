namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    public interface IReceiptProductFix
    {
        RecognizedReceiptProduct TryFix(RecognizedReceiptProduct original);
    }
}