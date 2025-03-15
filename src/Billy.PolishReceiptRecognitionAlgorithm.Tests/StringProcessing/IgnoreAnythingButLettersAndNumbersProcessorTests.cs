using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class IgnoreAnythingButLettersAndNumbersProcessorTests
    {
        [Theory]
        [InlineData("a bc def ghij 123", "abcdefghij")]
        [InlineData("  -- - - ", "")]
        [InlineData("1-2-a-b", "ab")]
        [InlineData("1.2,a:b;c+d=e", "abcde")]
        [InlineData("1.2,a:b;c+d=e źżąęłńćó", "abcdeźżąęłńćó")]
        [InlineData("1.2,a:b;c+d=e ŹŻĄĘŁŃĆÓ", "abcdeŹŻĄĘŁŃĆÓ")]
        public void can_ignore_phrases(string input, string expectedOutput)
        {
            //given
            var processors = new IgnoreAnythingButLettersProcessor();

            //when
            var output = processors.Process(input);

            //then
            output.Should().Be(expectedOutput);
        }
    }
}