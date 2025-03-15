namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class LevenshteinMatchingResult
    {
        //explanation: the lower the distance the better the matching. If distance is equal to 0
        //then both phrases were exactly the same
        public int LevenshteinDistance { get; }

        public LevenshteinMatchingResult(int levenshteinLevenshteinDistance)
        {
            LevenshteinDistance = levenshteinLevenshteinDistance;
        }

        public bool IsPerfectlyMatched => LevenshteinDistance == 0;

        public bool IsMatchedBetterThan(LevenshteinMatchingResult other) => LevenshteinDistance < other.LevenshteinDistance;
        
        public static LevenshteinMatchingResult WorstPossibleMatch() => 
            new LevenshteinMatchingResult(int.MaxValue);
    }
}