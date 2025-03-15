using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class IgnoreLoneLetterStringProcessorTests
    {
        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("a bc d", " bc")]
        [InlineData("a bc d ef g h i", " bc ef")]
        public void can_ignore_phrases(string input, string expectedOutput)
        {
            //given
            var processors = new IgnoreLoneLettersProcessor();;

            //when
            var output = processors.Process(input);

            //then
            output.Should().Be(expectedOutput);
        }
    }
}