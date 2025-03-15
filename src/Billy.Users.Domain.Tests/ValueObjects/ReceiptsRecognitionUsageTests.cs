using System;
using Billy.Domain;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.ValueObjects
{
    public class ReceiptsRecognitionUsageTests
    {
        [Fact]
        public void should_fail_validation_when_limit_less_than_counter()
        {
            Action action = () => ReceiptsRecognitionUsage.From(
                ReceiptsRecognitionLimit.From(5), 
                ReceiptsRecognitionCurrentPackageCounter.From(10));

            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.ReceiptsRecognitionLimitLessThatCurrentCounter);
        }

        [Fact]
        public void should_set_infinite_limit()
        {
            Action action = () => ReceiptsRecognitionUsage.From(
                ReceiptsRecognitionLimit.From(-1),
                ReceiptsRecognitionCurrentPackageCounter.From(10));
            
            action.Should().NotThrow();
        }

        [Fact]
        public void limit_should_be_reached_when_counter_reaches_limit()
        {
            var usage = ReceiptsRecognitionUsage.From(
                ReceiptsRecognitionLimit.From(10),
                ReceiptsRecognitionCurrentPackageCounter.From(10));

            usage.LimitReached.Should().BeTrue();
        }

        [Fact]
        public void should_increase_counter()
        {
            var usage = ReceiptsRecognitionUsage.From(
                ReceiptsRecognitionLimit.From(10),
                ReceiptsRecognitionCurrentPackageCounter.From(0));
            
            usage.IncreaseCounter();

            usage.CurrentPackageCounter.Value.Should().Be(1);
        }

        [Fact]
        public void should_assign_new_limit()
        {
            var usage = ReceiptsRecognitionUsage.From(
                ReceiptsRecognitionLimit.From(10),
                ReceiptsRecognitionCurrentPackageCounter.From(0));
            
            usage.AssignLimit(20);

            usage.Limit.Value.Should().Be(20);
        }

        [Fact]
        public void should_fail_validation_when_limit_less_than_current_counter()
        {
            var usage = ReceiptsRecognitionUsage.From(
                ReceiptsRecognitionLimit.From(20),
                ReceiptsRecognitionCurrentPackageCounter.From(10));

            Action action = () => usage.AssignLimit(5);
            
            action.Should().Throw<InvalidValueException>().And
                .ErrorCode.Should().Be(ErrorCodes.ReceiptsRecognitionLimitLessThatCurrentCounter);
        }
        
    }
}