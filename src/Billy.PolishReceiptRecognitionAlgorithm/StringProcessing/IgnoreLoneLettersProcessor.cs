using System.Text.RegularExpressions;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class IgnoreLoneLettersProcessor : IStringProcessor
    {
        private static readonly Regex LoneLetterRegex = new Regex(@"(^|\s+)\w(?=\s+|$)");

        public string Process(string input)
        {
            return LoneLetterRegex.Replace(input, "");
        }
    }
}