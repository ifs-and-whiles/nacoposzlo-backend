using System;
using System.Linq;
using AutoFixture;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class ReceiptAmountVsReceiptProductAmountsSumComparisonTests
    {
        public static Fixture Fixture { get; } = new Fixture();

        [Fact]
        public void when_sum_of_product_amounts_is_same_as_receipt_amount_no_problem_is_detected()
        {
            //given
            var receipt = Receipt(
                amount: 10M,
                products: new[] { Product(amount: 5M), Product(amount: 5M) });

            //when
            var problems = new ReceiptAmountVsReceiptProductAmountsSumComparison().AnalyzeReceipt(receipt);

            //then
            problems.Should().BeEmpty();
        }

        [Fact]
        public void when_sum_of_product_amounts_is_different_than_receipt_amount_problem_is_detected()
        {
            //given
            var receipt = Receipt(
                amount: 10M,
                products: new[] {Product(amount: 5M), Product(amount: 6M)});

            //when
            var problems = new ReceiptAmountVsReceiptProductAmountsSumComparison().AnalyzeReceipt(receipt);

            //then
            problems.Should().BeEquivalentTo(ReceiptProblem.AmountDifferentThanSumOfProducts);
        }

        [Fact]
        public void unrecognized_products_do_not_affect_the_outcome_of_analysis()
        {
            //given
            var receipt = Receipt(
                amount: 10M,
                products: new IReceiptProduct[]
                {
                    UnrecognizedProduct(), 
                    Product(amount: 11M),
                    UnrecognizedProduct()
                });

            //when
            var problems = new ReceiptAmountVsReceiptProductAmountsSumComparison().AnalyzeReceipt(receipt);

            //then
            problems.Should().BeEquivalentTo(ReceiptProblem.AmountDifferentThanSumOfProducts);
        }

        [Fact]
        public void products_with_amount_not_found_are_treated_as_if_their_amount_would_be_0()
        {
            //given
            var receipt = Receipt(
                amount: 10M,
                products: new IReceiptProduct[]
                {
                    Product(amount: 11M),
                    Product(amount: null)
                });

            //when
            var problems = new ReceiptAmountVsReceiptProductAmountsSumComparison().AnalyzeReceipt(receipt);

            //then
            problems.Should().BeEquivalentTo(ReceiptProblem.AmountDifferentThanSumOfProducts);
        }

        [Fact]
        public void receipts_with_amount_not_found_are_treated_as_if_their_amount_would_be_0()
        {
            //given
            var receipt = Receipt(
                amount: null,
                products: new[] { Product(amount: 5M) });

            //when
            var problems = new ReceiptAmountVsReceiptProductAmountsSumComparison().AnalyzeReceipt(receipt);

            //then
            problems.Should().BeEquivalentTo(ReceiptProblem.AmountDifferentThanSumOfProducts);
        }

        public Receipt Receipt(decimal? amount, IReceiptProduct[] products)
        {
            return new Receipt(
                seller: Fixture.Create<BoxedParsingResult<string>>(),
                taxNumber: Fixture.Create<BoxedParsingResult<TaxNumber>>(),
                date: Fixture.Create<BoxedParsingResult<DateTime>>(),
                amount: amount.HasValue
                    ? BoxedParsingResult<decimal>.WithoutProblems(
                        amount.Value, 
                        amount.ToString(),
                        DefaultBoundingBox())
                    : BoxedParsingResult<decimal>.NotFound(),
                products: products.ToExplicitTypes().ToArray(),
                problems: new string[0],
                originalOrientation: new Receipt.Orientation(0));
        }

        private static UnrecognizedReceiptProduct UnrecognizedProduct()
        {
            return Fixture.Create<UnrecognizedReceiptProduct>();
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
                boundingBox: DefaultBoundingBox());
        }

        private static BoundingBox DefaultBoundingBox()
        {
            return new BoundingBox(
                new Point(0,0),
                new Point(0,0),
                new Point(0,0),
                new Point(0,0));
        }
    }
}
