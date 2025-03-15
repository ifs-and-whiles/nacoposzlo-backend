using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Billy.CodeReadability;
using Index = Billy.CodeReadability.Index;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    public static class TaxNumberRegex
    {
        private static readonly Regex WithPrefixRegex = //eg. NIP 111-111-11-11
            new Regex(@"([nN][iI][pP])[ ]*((\d[- ]*){10})");

        private static readonly Regex Regex = //eg. NIP 111-111-11-11
            new Regex(@"([nN][iI][pP])?[ ]*((\d[- ]*){10})");

        public static Option<FindResult<string>> TryFindLine(IReadOnlyList<string> lines)
        {
            var taxNumberWithPrefix = TryFindLine(
                lines,
                TryMatchWithPrefix);

            if (taxNumberWithPrefix.TryGet(out var result))
                return result;

            var taxNumberWithoutPrefix = TryFindLine(
                lines,
                TryMatch);

            return taxNumberWithoutPrefix;
        }

        private static Option<FindResult<string>> TryFindLine(
            IReadOnlyList<string> lines,
            Func<string, Option<string>> matchingFunc)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var date = matchingFunc(line);

                if (date.TryGet(out var taxNumber))
                {
                    return new FindResult<string>(
                        new Index(i),
                        taxNumber);
                }
            }
            
            return Option<FindResult<string>>.None;
        }

        public static Option<string> TryMatchWithPrefix(string text)
        {
            var withPrefix = WithPrefixRegex.Match(text);

            return withPrefix.Success 
                ? ExtractTaxNumber(withPrefix) 
                : Option<string>.None;
        }

        public static Option<string> TryMatch(string text)
        {
            var match = Regex.Match(text);

            return match.Success 
                ? ExtractTaxNumber(match) 
                : Option<string>.None;
        }

        private static Option<string> ExtractTaxNumber(Match match)
        {
            var numbers = match.Groups[2].Value;

            return new string(numbers.Where(char.IsNumber).ToArray());
        }
    }
}