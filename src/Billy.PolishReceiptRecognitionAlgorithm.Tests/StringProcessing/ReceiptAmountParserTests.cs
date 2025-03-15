using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class ReceiptAmountParserTests
    {
        [Theory]
        [InlineData("Suma PLN 100", 100)]
        [InlineData("Suma PLN 100,50", 100.5)]
        [InlineData("Suma PLN 100, 50", 100.5)]
        [InlineData("Suma PLN 100 , 50", 100.5)]
        [InlineData("Suma PLN 100.50", 100.5)]
        [InlineData("Suma PLN 100. 50", 100.5)]
        [InlineData("Suma PLN 100 . 50", 100.5)]
        [InlineData("Suma PLN 10 0,50", 100.5)]
        [InlineData("Suma PLN 10 0,0 5", 100.05)]
        public void can_parse_receipt_amount(string rawAmount, decimal expectedAmount)
        {
            ReceiptAmountParser
                .GetAmount(new ReceiptSections.AmountSection(Line(rawAmount)))
                .Value
                .Match(
                    value => value.Should().Be(expectedAmount),
                    () => throw new WrongResultException("Should parse amount correctly"));
        }

        [Theory]
        [InlineData("Suma PLN o.oo")]
        public void when_there_is_no_amount_it_should_not_be_found(string rawAmount)
        {
            ReceiptAmountParser
                .GetAmount(new ReceiptSections.AmountSection(Line(rawAmount)))
                .Value
                .Match(
                    value => throw new WrongResultException("Should not parse amount at all"),
                    () => true);
        }

        public static ReceiptLine Line(string text)
        {
            return new ReceiptLine(
                text: text,
                 boundingBox: new BoundingBox(
                     new Point(0,0), 
                     new Point(0,0), 
                     new Point(0,0), 
                     new Point(0,0)));
        }
    }
}