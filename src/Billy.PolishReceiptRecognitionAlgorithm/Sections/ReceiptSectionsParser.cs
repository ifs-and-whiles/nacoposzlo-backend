using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;
using Index = Billy.CodeReadability.Index;

namespace Billy.PolishReceiptRecognitionAlgorithm.Sections
{
    public class ReceiptSectionsParser
    {
        private const string HeaderToken = "paragonfiskalny";

        private static readonly string[] TaxesTokens = {
            "sprzedazopadatk",
            "sprzedopod",
            "sprzedazopodatkowana",
            "sprzedaopodatkowana",
            "spop",
            "spopA",
            "spopB",
            "spopC",
            "spzwe"
        };

        private static readonly string[] TaxesSumTokens =
        {
            "podatekptu",
            "sumaptu",
            "razemptu"
        };

        private static readonly string[] TaxesTokenPhrasesToIgnore =
        {
            "ptu a", "ptu b", "ptu c", "ptu d", "ptu", ".", "%", ",", ":", "x"
            // explanation: for example "SP.OP.C: 13.70 PTU 5.00% 0.65"
        };

        private const string AmountToken = "sumapln";
        
        public static ReceiptSections GetSections(ReceiptLine[] lines)
        {
            var receiptMap = new ReceiptMap();
            var receiptLines = new ReceiptLines(lines);

            receiptMap.HeaderTokenLocation = FindHeaderToken(receiptLines);

            if (receiptMap.HeaderTokenLocation == null)
            {
                receiptMap.HeaderDateLocation = FindHeaderDate(receiptLines);

                if (receiptMap.HeaderDateLocation == null)
                {
                    receiptMap.HeaderTaxNumberLocation = FindTaxNumber(receiptLines);
                }
            }

            receiptMap.TaxesTokenLocation = FindTaxesToken(receiptLines, receiptMap);
            receiptMap.TaxesSumTokenLocation = FindTaxesSumToken(receiptLines, receiptMap);
            receiptMap.SumTokenLocation = FindSumToken(receiptLines, receiptMap);

            return receiptMap.BuildSections(receiptLines);
        }

        private static Index FindHeaderToken(ReceiptLines lines)
        {
            return StringAnalyzer
                .ForLines(
                    lines.AllTexts)
                .WithPreprocessing(
                    new IgnoreSpacesProcessor())
                .LocateLineWithPhraseUsingLevenshteinDistance(
                    searchedPhrases: HeaderToken,
                    levenshteinDistanceThreshold: 5)
                .Match(
                    tokenIndex => tokenIndex,
                    phraseNotFound => null);
        }

        private static Index FindHeaderDate(ReceiptLines lines)
        {
            return DateRegex
                .TryFindLine(lines.FirstHalf())
                .Match(
                    result => result.Index,
                    () => null);
        }

        private static Index FindTaxNumber(ReceiptLines lines)
        {
            return TaxNumberRegex
                .TryFindLine(lines.FirstHalf())
                .Match(
                    result => result.Index,
                    () => null);
        }

        private static Index FindTaxesToken(ReceiptLines lines, ReceiptMap map)
        {
            return StringAnalyzer
                .ForLines(lines.AllTexts.Skip(map.EndOfHeader))
                .WithPreprocessing(
                    new LowerCaseProcessor(),
                    new ReplacePolishCharactersProcessor(),
                    new IgnoreLoneLettersProcessor(),
                    new IgnorePhrasesProcessor(TaxesTokenPhrasesToIgnore),
                    new IgnoreAnythingButLettersProcessor())
                .LocateLineWithOneOfPhrasesUsingLevenshteinDistance(
                    searchedPhrases: TaxesTokens,
                    levenshteinDistanceThreshold: 2)
                .Match(
                    tokenIndex => (Index)(tokenIndex + map.EndOfHeader),
                    phraseNotFound => null);
        }

        private static Index FindTaxesSumToken(ReceiptLines lines, ReceiptMap map)
        {
            var linesToSkip = map.TaxesTokenLocation ?? map.EndOfHeader;
            var linesToAnalyze = lines.AllTexts.Skip(linesToSkip);

            return StringAnalyzer
                .ForLines(linesToAnalyze)
                .WithPreprocessing(
                    new IgnoreAnythingButLettersProcessor(),
                    new IgnorePhrasesProcessor(new[]
                    {
                        AmountToken
                    }))
                .LocateLineWithOneOfPhrasesUsingLevenshteinDistance(
                    searchedPhrases: TaxesSumTokens,
                    levenshteinDistanceThreshold: 2)
                .Match(
                    tokenIndex => (Index)(tokenIndex + linesToSkip),
                    phraseNotFound => null);
        }

        private static Index FindSumToken(ReceiptLines lines, ReceiptMap map)
        {
            var linesToSkip = 
                map.TaxesSumTokenLocation 
                ?? map.TaxesTokenLocation
                ?? map.EndOfHeader;

            var linesToAnalyze = lines.AllTexts.Skip(linesToSkip);
            
            return StringAnalyzer
                .ForLines(linesToAnalyze)
                .WithPreprocessing(
                    new IgnoreAnythingButLettersProcessor())
                .LocateLineWithPhraseUsingLevenshteinDistance(
                    searchedPhrases: AmountToken,
                    levenshteinDistanceThreshold: 2)
                .Match(
                    tokenIndex => (Index)(tokenIndex + linesToSkip),
                    phraseNotFound => null);
        }
        
        private class ReceiptLines
        {
            public IReadOnlyList<string> AllTexts { get; }
            public IReadOnlyList<ReceiptLine> All { get; }

            public ReceiptLines(ReceiptLine[] lines)
            {
                All = lines;
                AllTexts = lines.Select(line => line.Text).ToArray();
            }
            
            //this is not meant to be precise half of receipt lines
            //its only a little optimization when we know that some
            //tokens are located somewhere rather at the beginning of the receipt 
            //and definitely not at the end
            public string[] FirstHalf() => AllTexts
                .Take((AllTexts.Count * 3) / 5)
                .ToArray();

            public Index LastIndex => AllTexts.Count - 1;
        }

        private class ReceiptMap
        {
            public Index HeaderTaxNumberLocation { get; set; }
            public Index HeaderDateLocation { get; set; }
            public Index HeaderTokenLocation { get; set; }
            public Index TaxesTokenLocation { get; set; }
            public Index TaxesSumTokenLocation { get; set; }
            public Index SumTokenLocation { get; set; }

            public Index EndOfHeader =>
                HeaderTokenLocation
                ?? HeaderDateLocation
                ?? HeaderTaxNumberLocation
                ?? 0; //we assume that first line of receipt must always be a header
            
            public ReceiptSections BuildSections(ReceiptLines lines)
            {
                var headerSection = BuildHeader(lines);
                var productsSection = BuildProducts(lines);
                var taxesSection = BuildTaxes(lines);
                var amountSection = BuildAmount(lines);
                var footerSection = BuildFooter(lines);

                return new ReceiptSections(
                    headerSection,
                    productsSection, 
                    taxesSection, 
                    amountSection, 
                    footerSection);
            }

            private ReceiptSections.HeaderSection BuildHeader(ReceiptLines lines)
            {
                return new ReceiptSections.HeaderSection(
                    TakeBetween(lines.All, 0, EndOfHeader));
            }

            private ReceiptSections.ProductsSection BuildProducts(ReceiptLines lines)
            {
                if (TaxesTokenLocation != null)
                {
                    return new ReceiptSections.ProductsSection(
                        TakeBetween(lines.All, EndOfHeader + 1, TaxesTokenLocation - 1));
                }

                if (TaxesSumTokenLocation != null)
                {
                    return new ReceiptSections.ProductsSection(
                        TakeBetween(lines.All, EndOfHeader + 1, TaxesSumTokenLocation - 1));
                }

                if (SumTokenLocation != null)
                {
                    return new ReceiptSections.ProductsSection(
                        TakeBetween(lines.All, EndOfHeader + 1, SumTokenLocation - 1));
                }

                return new ReceiptSections.ProductsSection(
                    TakeBetween(lines.All, EndOfHeader + 1, lines.LastIndex));
            }

            private ReceiptSections.TaxesSection BuildTaxes(ReceiptLines lines)
            {
                if (TaxesTokenLocation != null && TaxesSumTokenLocation != null)
                {
                    return new ReceiptSections.TaxesSection(
                        TakeBetween(lines.All, TaxesTokenLocation, TaxesSumTokenLocation));
                }

                if (TaxesTokenLocation != null && SumTokenLocation != null)
                {
                    return new ReceiptSections.TaxesSection(
                        TakeBetween(lines.All, TaxesTokenLocation, SumTokenLocation - 1));
                }

                if (TaxesSumTokenLocation != null)
                {
                    return new ReceiptSections.TaxesSection(
                        TakeBetween(lines.All, TaxesSumTokenLocation, TaxesSumTokenLocation));
                }
                
                return new ReceiptSections.TaxesSection(
                    Enumerable.Empty<ReceiptLine>());
            }

            private ReceiptSections.AmountSection BuildAmount(ReceiptLines lines)
            {
                if (SumTokenLocation != null)
                {
                    return new ReceiptSections.AmountSection(
                        lines.All[SumTokenLocation]);
                }

                return new ReceiptSections.AmountSection(null);
            }

            private ReceiptSections.FooterSection BuildFooter(ReceiptLines lines)
            {
                if (SumTokenLocation != null)
                {
                    return new ReceiptSections.FooterSection(
                        TakeBetween(lines.All, SumTokenLocation + 1, lines.LastIndex));
                }

                if (TaxesSumTokenLocation != null)
                {
                    return new ReceiptSections.FooterSection(
                        TakeBetween(lines.All, TaxesSumTokenLocation + 1, lines.LastIndex));
                }

                if (TaxesTokenLocation != null)
                {
                    return new ReceiptSections.FooterSection(
                        TakeBetween(lines.All, TaxesTokenLocation, lines.LastIndex));
                }

                return new ReceiptSections.FooterSection(
                    Enumerable.Empty<ReceiptLine>());
            }

            private IEnumerable<T> TakeBetween<T>(IReadOnlyList<T> lines, Index first, Index last)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (i >= first && i <= last)
                    {
                        yield return lines[i];
                    }
                }
            }
        }
    }
}
