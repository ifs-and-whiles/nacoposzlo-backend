using System.Collections.Generic;
using System.Linq;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class IgnorePhrasesProcessor : IStringProcessor
    {
        private readonly string[] _phrasesToIgnore;

        public IgnorePhrasesProcessor(IEnumerable<string> phrasesToIgnore)
        {
            _phrasesToIgnore = phrasesToIgnore.ToArray();
        }

        public string Process(string input)
        {
            return _phrasesToIgnore.Aggregate(input, (acc, replace) => acc.Replace(replace, ""));
        }
    }
}