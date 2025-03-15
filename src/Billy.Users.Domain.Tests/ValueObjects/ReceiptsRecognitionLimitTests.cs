using System;
using Billy.Domain;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.ValueObjects
{
    public class ReceiptsRecognitionLimitTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(0)]
        [InlineData(-1)]
        public void should_pass_validation_for_correct_limit(int value)
        {
            var limit = ReceiptsRecognitionLimit.From(value);
            limit.Value.Should().Be(value);
        }

        [Fact]
        public void should_use_default_value_for_empty_limit()
        {
            var limit = ReceiptsRecognitionLimit.From(null);
            limit.Value.Should().Be(ReceiptsRecognitionLimit.Default);
        }

        [Fact]
        public void should_fail_validation_for_negative_values()
        {
            Action action = () => ReceiptsRecognitionLimit.From(-20);

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.InvalidReceiptsRecognitionLimit);
        }

        [Fact]
        public void should_have_infinite_limit()
        {
            var limit = ReceiptsRecognitionLimit.From(-1);
            
            limit.Value.Should().Be(-1);
            limit.IsUnlimited.Should().BeTrue();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(0)]
        public void should_not_have_infinite_limit_for_positive_value(int value)
        {
            var limit = ReceiptsRecognitionLimit.From(value);
            limit.Value.Should().Be(value);
            limit.IsUnlimited.Should().BeFalse();
        } 
    }
}