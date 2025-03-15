using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;

namespace Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection
{
    public class KnownNonProductsFilter : IReceiptNonProductFilter
    {
        private static string[] NonProductNames = {
            "podsuma",
            "bezrecepty",
            "razem"
        };

        public bool IsNonProduct(Option<string> productName)
        {
            return productName.Match(
                name => CheckIfNonProduct(name),
                () => false);
        }

        private static bool CheckIfNonProduct(string name)
        {
            return StringAnalyzer
                .ForLines(new[] {name})
                .WithPreprocessing(
                   new IgnoreAnythingButLettersProcessor())
                .LocateLineWithOneOfPhrasesUsingLevenshteinDistance(
                    searchedPhrases: NonProductNames,
                    levenshteinDistanceThreshold: 2)
                .Match(
                    foundIndex => true,
                    notFound => false);
        }
    }
}