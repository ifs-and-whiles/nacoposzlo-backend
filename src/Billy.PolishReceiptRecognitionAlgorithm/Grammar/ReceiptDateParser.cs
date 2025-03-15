using System;
using System.Linq;
using Billy.CodeReadability;
using Billy.CollectionTools;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    public class ReceiptDateParser
    {
        public static BoxedParsingResult<DateTime> ParseDate(
            ReceiptSections.HeaderSection headerSection,
            ReceiptSections.FooterSection footerSection)
        {
            var lines = Arrays.Concat(
                headerSection.Lines, 
                footerSection.Lines);

            return DateRegex
                .TryFindLine(
                    lines.Select(line => line.Text).ToArray())
                .Match(
                    result =>
                    {
                        var correspondingLine = lines
                            .ElementAt(result.Index);

                        return BoxedParsingResult<DateTime>.WithoutProblems(
                            value: result.Value,
                            rawValue: correspondingLine.Text,
                            boundingBox: correspondingLine.BoundingBox);
                    },
                    BoxedParsingResult<DateTime>.NotFound);
        }
    }
}