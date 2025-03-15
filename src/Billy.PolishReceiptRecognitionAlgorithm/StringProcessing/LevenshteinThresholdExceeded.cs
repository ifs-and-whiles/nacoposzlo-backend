namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class LevenshteinThresholdExceeded
    {
        public LevenshteinThresholdExceeded(int levenshteinDistance)
        {
            LevenshteinDistance = levenshteinDistance;
        }

        public int LevenshteinDistance { get; }

        public override string ToString()
        {
            return $"Levenshtein distance: {LevenshteinDistance}";
        }
    }
}