
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    public class ReceiptProductsParser
    {
        public static IReceiptProduct[] ParseProducts(ReceiptSections.ProductsSection products)
        {
            var productLines = Merge(products);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(productLines));
            using var streamReader = new StreamReader(stream, Encoding.UTF8);

            var lexer = new ExpressionLexer(new AntlrInputStream(streamReader));
            var parser = new ExpressionParser(new CommonTokenStream(lexer));
            var tree = parser.productsexp();

            ThrowIfAnyRecognitionExceptionFound(tree);

            var output = new ReceiptContentEvaluator(products.Lines).Visit(tree);

            return (IReceiptProduct[]) output;
        }
        
        private static void ThrowIfAnyRecognitionExceptionFound(ExpressionParser.ProductsexpContext tree)
        {
            var recognitionExceptions = RecognitionErrors
                .FindAll(tree);

            if (recognitionExceptions.Any())
            {
                throw new AntlrException(
                    $"Following problems were found while parsing the expression: {tree}", recognitionExceptions);
            }
        }

        private static string Merge(ReceiptSections.IReceiptSection section)
        {
            return string.Join(
                GrammarConsts.NewLineTag, 
                section.Lines.Select(line => line.Text));
        }
    }
}
