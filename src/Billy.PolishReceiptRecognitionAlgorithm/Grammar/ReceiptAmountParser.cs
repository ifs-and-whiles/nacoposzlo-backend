using System;
using System.Text.RegularExpressions;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;

namespace Billy.PolishReceiptRecognitionAlgorithm.Grammar
{
    public class ReceiptAmountParser
    {
        private static readonly Regex AmountRegex = new Regex(@"\d(?:\d| )*(?:[ ]*[.|,][ ]*(?:(?:\d| )*\d))?");

        public static BoxedParsingResult<decimal> GetAmount(ReceiptSections.AmountSection amountSection)
        {
            if(amountSection.Line == null)
                return BoxedParsingResult<decimal>.NotFound();

            var match = AmountRegex.Match(
                amountSection.Line.Text);

            if (match.Success)
                return StringAlgorithm
                    .ToDecimal(match.Groups[0].Value)
                    .Match(
                        value => BoxedParsingResult<decimal>.WithoutProblems(
                            value, 
                            amountSection.Line.Text,
                            amountSection.Line.BoundingBox),
                        notANumber => throw new InvalidOperationException(
                            $"Value '{notANumber.Text}' is not a number"));

            return BoxedParsingResult<decimal>.WithProblems(
                amountSection.Line.Text,
                amountSection.Line.BoundingBox,
                new[]
                {
                    ParsingProblem.UnexpectedCharactersFound
                });
        }
    }
}