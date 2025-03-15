using System.Collections.Generic;
using System.Linq;
using Billy.CodeReadability;
using Billy.CollectionTools;
using Index = Billy.CodeReadability.Index;

namespace Billy.PolishReceiptRecognitionAlgorithm.StringProcessing
{
    public class StringAnalyzer
    {
        private readonly string[] _lines;
        private readonly List<IStringProcessor> _analysisPreprocessors;

        private StringAnalyzer(string[] lines)
        {
            _lines = lines;
            _analysisPreprocessors = new List<IStringProcessor>();
        }

        public static StringAnalyzer ForLines(IEnumerable<string> lines) => 
            new StringAnalyzer(lines
                .Select(l => l.ToLowerInvariant())
                .ToArray());

        public StringAnalyzer WithPreprocessing(params IStringProcessor[] processors)
        {
            _analysisPreprocessors.AddRange(processors);
            return this;
        }

        public Either<Index, MatchingPhraseNotFound> LocateLineWithPhraseUsingLevenshteinDistance(
            string searchedPhrases, int levenshteinDistanceThreshold)
        {
            return LocateLineWithOneOfPhrasesUsingLevenshteinDistance(new[] {searchedPhrases}, levenshteinDistanceThreshold);
        }

        public Either<Index, MatchingPhraseNotFound> LocateLineWithOneOfPhrasesUsingLevenshteinDistance(
            IEnumerable<string> searchedPhrases, int levenshteinDistanceThreshold)
        {
            var matchingResults = new Dictionary<Index, LevenshteinMatchingResult>();
            var preprocessedLines = _lines.Select(Preprocess).ToArray();

            foreach (var targetedPhrase in searchedPhrases)
            {
                var locatingResult = LocateLineWithPhrase(preprocessedLines, targetedPhrase, levenshteinDistanceThreshold);

                if (locatingResult.TryGetLeft(out var line))
                {
                    if (line.matchingResult.IsPerfectlyMatched)
                        return line.index;

                    matchingResults[line.index] = line.matchingResult;
                }
            }

            var bestMatchingLine = matchingResults
                .OrderBy(kvp => kvp.Value.LevenshteinDistance)
                .TryGetFirst();

            return bestMatchingLine.ToEither(
                record => record.Key,
                () => new MatchingPhraseNotFound());
        }

        private Either<(Index index, LevenshteinMatchingResult matchingResult), MatchingPhraseNotFound> LocateLineWithPhrase(
            IEnumerable<string> lines, 
            string targetedPhrase, 
            int levenshteinDistanceThreshold)
        {
            var targetedPhraseLower = targetedPhrase.ToLowerInvariant();

            var itemsWithIndex = lines.Select((item, index) => new
            {
                Phrase = item,
                Index = index
            });

            var currentBestMatch = LevenshteinMatchingResult.WorstPossibleMatch();
            var closestPhraseIndex = Option<int>.None;

            foreach (var item in itemsWithIndex)
            {
                var matchingResult = StringAlgorithm.TryMatchWithLevenshteinDistance(
                    first: item.Phrase, 
                    second: targetedPhraseLower, 
                    threshold: levenshteinDistanceThreshold);

                if (matchingResult.TryGetLeft(out var result))
                {
                    if (result.IsPerfectlyMatched)
                        return (item.Index, result);

                    if (result.IsMatchedBetterThan(currentBestMatch))
                    {
                        currentBestMatch = result;
                        closestPhraseIndex = item.Index;
                    }
                }
            }

            return closestPhraseIndex.ToEither(
                index => ((Index, LevenshteinMatchingResult)) (index, currentBestMatch),
                () => new MatchingPhraseNotFound());
        }

        private string Preprocess(string input)
        {
            return _analysisPreprocessors
                .Aggregate(input, (acc, processor) => processor.Process(acc));
        }
    }
}