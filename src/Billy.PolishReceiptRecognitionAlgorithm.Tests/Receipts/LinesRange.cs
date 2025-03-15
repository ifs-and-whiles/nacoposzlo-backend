using System.Collections.Generic;
using System.Linq;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts
{
    public class LinesRange
    {
        private readonly int _from;
        private readonly int _to;

        public LinesRange(int from, int to)
        {
            _from = @from;
            _to = to;
        }

        public string[] Pick(IEnumerable<string> lines)
        {
            return lines
                .Skip(_from)
                .Take(_to - _from + 1)
                .ToArray();
        }
    }
}