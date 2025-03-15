using System.Globalization;
using AutoFixture;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class TooManyDecimalPlacesInProductAmountFixTests
    {
        public static Fixture Fixture { get; } = new Fixture();

        [Fact]
        public void when_amount_has_no_decimal_places_product_is_not_changed()
        {
            //given
            var product = Product(
                amount: 30M);

            //when
            var fixedProduct = new TooManyDecimalPlacesInProductAmountFix().TryFix(product);

            //then
            fixedProduct.Should().Be(product);
        }

        [Fact]
        public void when_amount_has_only_two_decimal_places_product_is_not_changed()
        {
            //given
            var product = Product(
                amount: 30.01M);

            //when
            var fixedProduct = new TooManyDecimalPlacesInProductAmountFix().TryFix(product);

            //then
            fixedProduct.Should().Be(product);
        }

        [Fact]
        public void when_amount_was_not_found_product_is_not_changed()
        {
            //given
            var product = Product(
                amount: null);

            //when
            var fixedProduct = new TooManyDecimalPlacesInProductAmountFix().TryFix(product);

            //then
            fixedProduct.Should().Be(product);
        }

        [Theory]
        [InlineData("30.011", "30.01")]
        [InlineData("30.015", "30.01")]
        [InlineData("30.019", "30.01")]
        [InlineData("30.01999999", "30.01")]
        [InlineData("30.01000001", "30.01")]
        public void when_amount_has_more_than_two_decimal_places_product_is_corrected(
            string amount, string expectedAmountAfterFix)
        {
            //given
            var product = Product(
                amount: Decimal(amount));

            //when
            var fixedProduct = new TooManyDecimalPlacesInProductAmountFix().TryFix(product);

            //then
            fixedProduct.Should().BeEquivalentTo(product, options =>
                options.Excluding(x => x.Amount));

            fixedProduct.Amount.Should().BeEquivalentTo(ParsingResult<decimal>.WithProblems(
                value: Decimal(expectedAmountAfterFix),
                rawValue: product.Amount.RawValue,
                problems: product.Amount.Problems));
        }

        private static decimal Decimal(string value)
        {
            return System.Decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        private static RecognizedReceiptProduct Product(decimal? amount)
        {
            return new RecognizedReceiptProduct(
                name: Fixture.Create<ParsingResult<string>>(),
                quantity: Fixture.Create<ParsingResult<decimal>>(),
                unit: Fixture.Create<ParsingResult<string>>(),
                unitPrice: Fixture.Create<ParsingResult<decimal>>(),
                amount: amount.HasValue
                    ? ParsingResult<decimal>.WithoutProblems(amount.Value, amount.ToString())
                    : ParsingResult<decimal>.NotFound(),
                taxTag: Fixture.Create<ParsingResult<string>>(),
                boundingBox: new BoundingBox(
                    new Point(0,0),
                    new Point(0,0),
                    new Point(0,0),
                    new Point(0,0)));
        }
    }
}