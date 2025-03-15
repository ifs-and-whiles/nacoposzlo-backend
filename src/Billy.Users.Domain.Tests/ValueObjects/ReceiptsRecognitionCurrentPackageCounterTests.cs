using System;
using Billy.Domain;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.ValueObjects
{
    public class ReceiptsRecognitionCurrentPackageCounterTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(0)]
        public void should_pass_validation_for_correct_counter(int value)
        {
            var counter = ReceiptsRecognitionCurrentPackageCounter.From(value);
            counter.Value.Should().Be(value);
        }

        [Fact]
        public void should_use_default_value_for_empty_counter()
        {
            var counter = ReceiptsRecognitionCurrentPackageCounter.From(null);
            counter.Value.Should().Be(ReceiptsRecognitionCurrentPackageCounter.Default);
        }

        [Fact]
        public void should_fail_validation_for_negative_values()
        {
            Action action = () => ReceiptsRecognitionCurrentPackageCounter.From(-20);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.InvalidReceiptsRecognitionCurrentPackageCounter);
        }
        
    }
}