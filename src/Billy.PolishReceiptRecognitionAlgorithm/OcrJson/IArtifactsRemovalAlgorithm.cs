namespace Billy.PolishReceiptRecognitionAlgorithm.OcrJson
{
    public interface IArtifactsRemovalAlgorithm
    {
        OcrResult RemoveArtifacts(OcrResult ocrResult);
    }
}