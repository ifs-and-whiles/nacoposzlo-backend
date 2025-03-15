using System.Linq;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class ReplacePolishCharactersProcessor : IStringProcessor
    {
        public string Process(string input)
        {
            return new string(input.ToCharArray().Select(NormalizeChar).ToArray());
        }

        private char NormalizeChar(char c)
        {
            switch (c)
            {
                case 'ą':
                    return 'a';
                case 'ć':
                    return 'c';
                case 'ę':
                    return 'e';
                case 'ł':
                    return 'l';
                case 'ń':
                    return 'n';
                case 'ó':
                    return 'o';
                case 'ś':
                    return 's';
                case 'ż':
                case 'ź':
                    return 'z';
            }
            return c;
        }
    }
}