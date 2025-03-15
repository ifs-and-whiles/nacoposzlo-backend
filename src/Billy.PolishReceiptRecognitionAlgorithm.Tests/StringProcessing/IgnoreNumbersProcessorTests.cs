using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class IgnoreNumbersProcessorTests
    {
        [Theory]
        [InlineData("string without any number", "string without any number")]
        [InlineData("string 1 with 12 some 123.321 numbers", "string  with  some  numbers")]
        public void can_ignore_numbers(string input, string expectedOutput)
        {
            //given
            var processor = new IgnoreNumbersProcessor();

            //when
            var output = processor.Process(input);

            //then
            output.Should().Be(expectedOutput);
        }
    }
}