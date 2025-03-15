using System.Text.RegularExpressions;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class IgnoreNumbersProcessor : IStringProcessor
    {
        private static readonly Regex NumberRegex = new Regex(@"\d+([.|,]\d+)?");

        public string Process(string input)
        {
            return NumberRegex.Replace(input, "");
        }
    }
}