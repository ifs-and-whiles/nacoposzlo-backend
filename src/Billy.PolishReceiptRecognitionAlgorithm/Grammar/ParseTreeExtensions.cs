using System.Collections.Generic;
using Antlr4.Runtime.Tree;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    public static class ParseTreeExtensions
    {
        public static IEnumerable<IParseTree> GetChildren(this IParseTree tree)
        {
            var i = 0;

            while (true)
            {
                var child = tree.GetChild(i);

                if (child == null)
                {
                    yield break;
                }

                i = i + 1;

                yield return child;
            }
        }
    }
}