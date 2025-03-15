namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class LowerCaseProcessor : IStringProcessor
    {
        public string Process(string input)
        {
            return input.ToLowerInvariant();
        }
    }
}