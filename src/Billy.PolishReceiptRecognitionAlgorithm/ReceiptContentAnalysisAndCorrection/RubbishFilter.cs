using System.Linq;
using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;

namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    /// <summary>
    /// sometimes receipts contain line build of '-' to separate sections,
    /// this line is being recognized as product
    ///
    /// any name which does not contain a least one letter or number will be recognized
    /// as non-product
    /// </summary>
    public class RubbishFilter : IReceiptNonProductFilter
    {
        private static readonly IStringProcessor Processor = new IgnoreAnythingButLettersOrNumbersProcessor();

        public bool IsNonProduct(Option<string> productName)
        {
            return productName.Match(
                name =>
                {
                    var processed = Processor.Process(name);

                    //if there was no letters and no numbers that clearly wasn't a product name
                    return string.IsNullOrEmpty(processed);
                },
                () => false);
        }
    }
}