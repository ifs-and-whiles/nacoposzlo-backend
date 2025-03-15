using Billy.CodeReadability;
using Billy.PolishReceiptRecognitionAlgorithm.ReceiptContentAnalysisAndCorrection;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class RubbishFilterTests
    {
        [Theory]
        [InlineData("something", false)]
        [InlineData("123 something", false)]
        [InlineData("123", false)]
        [InlineData("---- - -   ----", true)]
        [InlineData(".,/!!@#%$# - - ^&*()* ----", true)]
        [InlineData(".,/!!@#%$# -a- ^&*()* ----", false)]
        [InlineData("-.....", true)]
        public void can_detect_rubbish_non_products(string name, bool isNonProduct)
        {
            //given
            var filter = new RubbishFilter();

            //when
            var actualIsNonProduct = filter.IsNonProduct(
                Option<string>.Some(name));

            //then
            actualIsNonProduct.Should().Be(isNonProduct);
        }
    }
}