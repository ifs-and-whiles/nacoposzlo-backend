using System;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.StringProcessing
{
    public class IgnorePhrasesProcessorTests
    {
        [Theory]
        [InlineData("a bc def ghij", "", "a bc def ghij")]
        [InlineData("a bc def ghij", "a", " bc def ghij")]
        [InlineData("a bc def ghij", "a,de", " bc f ghij")]
        [InlineData("a bc def ghij", "a,de,de", " bc f ghij")]
        [InlineData("a bc def ghij", "ijk", "a bc def ghij")]
        public void can_ignore_phrases(string input, string phrases, string expectedOutput)
        {
            //given
            var toIgnore = phrases.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var processors = new IgnorePhrasesProcessor(toIgnore);

            //when
            var output = processors.Process(input);

            //then
            output.Should().Be(expectedOutput);
        }
    }
}