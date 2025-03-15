using System.Text.RegularExpressions;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class IgnoreAnythingButLettersOrNumbersProcessor : IStringProcessor
    {
        public string Process(string input)
        {
            return Regex.Replace(input, "[^a-ząćęłńóśźżA-ZĄĆĘŁŃÓŚŹŻ0-9]", "");
        }
    }
}