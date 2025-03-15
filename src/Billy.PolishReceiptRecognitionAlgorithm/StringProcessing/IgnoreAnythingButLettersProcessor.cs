using System.Text.RegularExpressions;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class IgnoreAnythingButLettersProcessor : IStringProcessor
    {
        public string Process(string input)
        {
            return Regex.Replace(input, "[^a-ząćęłńóśźżA-ZĄĆĘŁŃÓŚŹŻ]", "");
        }
    }
}