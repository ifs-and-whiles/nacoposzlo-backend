namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class IgnoreSpacesProcessor : IStringProcessor
    {
        public string Process(string input)
        {
            return input.Replace(" ", "");
        }
    }
}